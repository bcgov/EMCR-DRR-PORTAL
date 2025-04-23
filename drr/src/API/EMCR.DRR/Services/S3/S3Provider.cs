using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace EMCR.DRR.API.Services.S3
{
    public class S3Provider : IS3Provider
    {
        protected readonly IAmazonS3 _amazonS3Client;
        private string? bucketName;

        public S3Provider(IAmazonS3 amazonS3Client, IConfiguration configuration)
        {
            _amazonS3Client = amazonS3Client;
            bucketName = configuration.GetValue<string>("S3:BucketName");
        }

        public async Task<string> HandleCommand(StorageCommand cmd)
        {
            var ct = new CancellationTokenSource().Token;
            return cmd switch
            {
                UploadFileCommand c => await UploadStorageItem(c, ct),
                UploadFileStreamCommand c => await UploadStorageItemStream(c, ct),
                UpdateTagsCommand c => await UpdateTags(c, ct),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<StorageQueryResults> HandleQuery(StorageQuery query)
        {
            var ct = new CancellationTokenSource().Token;
            return query switch
            {
                FileQuery q => await DownloadStorageItem(q.Key, q.Folder, ct),
                FileStreamQuery q => await DownloadStorageItemStreamed(q.Key, q.Folder, ct),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };
        }

        private async Task<string> UploadStorageItem(UploadFileCommand cmd, CancellationToken cancellationToken)
        {
            var folder = cmd.Folder == null ? "" : $"{cmd.Folder}/";
            var key = $"{folder}{cmd.Key}";

            var request = new PutObjectRequest
            {
                Key = key,
                ContentType = !string.IsNullOrEmpty(cmd.File.ContentType) ? cmd.File.ContentType : null,
                InputStream = new MemoryStream(cmd.File.Content),
                BucketName = bucketName,
                TagSet = GetTagSet(cmd.FileTag?.Tags ?? []),
            };
            request.Metadata.Add("contenttype", cmd.File.ContentType);

            //save an ascii safe version the file name for reference
            //not used - just nice if you need to browse S3
            request.Metadata.Add("filenameref", Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(cmd.File.FileName)));
            //Convert file name to base64 string to encode special characters not supported by S3
            request.Metadata.Add("filename", Convert.ToBase64String(Encoding.UTF8.GetBytes(cmd.File.FileName)));
            if (cmd.File.Metadata != null)
            {
                foreach (FileMetadata md in cmd.File.Metadata)
                    request.Metadata.Add(md.Key, md.Value);
            }

            var response = await _amazonS3Client.PutObjectAsync(request, cancellationToken);
            response.EnsureSuccess();

            return cmd.Key;
        }

        private async Task<string> UploadStorageItemStream(UploadFileStreamCommand cmd, CancellationToken cancellationToken)
        {
            var folder = cmd.Folder == null ? "" : $"{cmd.Folder}/";
            var key = $"{folder}{cmd.Key}";

            using var stream = cmd.FileStream.File.OpenReadStream();

            var s3Request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = stream,
                ContentType = cmd.FileStream.ContentType,
            };

            s3Request.Metadata.Add("contenttype", cmd.FileStream.ContentType);
            s3Request.Metadata.Add("filenameref", Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(cmd.FileStream.File.FileName)));
            s3Request.Metadata.Add("filename", Convert.ToBase64String(Encoding.UTF8.GetBytes(cmd.FileStream.File.FileName)));
            if (cmd.FileStream.Metadata != null)
            {
                foreach (FileMetadata md in cmd.FileStream.Metadata)
                    s3Request.Metadata.Add(md.Key, md.Value);
            }

            var response = await _amazonS3Client.PutObjectAsync(s3Request, cancellationToken);
            response.EnsureSuccess();
            return cmd.Key;
        }

        private async Task<FileQueryResult> DownloadStorageItem(string key, string? folder, CancellationToken ct)
        {
            var dir = folder == null ? "" : $"{folder}/";
            var requestKey = $"{dir}{key}";

            //get object
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = requestKey,
            };
            var response = await _amazonS3Client.GetObjectAsync(request, ct);
            response.EnsureSuccess();
            using var contentStream = response.ResponseStream;
            using var ms = new MemoryStream();
            await contentStream.CopyToAsync(ms, ct);
            await contentStream.FlushAsync(ct);

            //get tagging
            var tagResponse = await _amazonS3Client.GetObjectTaggingAsync(
                new GetObjectTaggingRequest
                {
                    BucketName = bucketName,
                    Key = requestKey,
                }, ct);
            tagResponse.EnsureSuccess();

            return new FileQueryResult
            {
                Key = key,
                Folder = folder,
                File = new S3File
                {
                    ContentType = response.Metadata["contenttype"],
                    FileName = GetSafeFileName(response.Metadata["filename"]),
                    Content = ms.ToArray(),
                    Metadata = GetMetadata(response.Metadata).AsEnumerable(),
                },
                FileTag = new FileTag
                {
                    Tags = GetTags(tagResponse.Tagging).AsEnumerable()
                }
            };
        }

        private async Task<FileStreamQueryResult> DownloadStorageItemStreamed(string key, string? folder, CancellationToken ct)
        {
            var dir = folder == null ? "" : $"{folder}/";
            var requestKey = $"{dir}{key}";

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = requestKey,
            };

            var response = await _amazonS3Client.GetObjectAsync(request, ct);
            response.EnsureSuccess();

            // get file metadata
            var contentType = response.Metadata["contenttype"];
            var fileName = GetSafeFileName(response.Metadata["filename"]);

            //get tagging
            var tagResponse = await _amazonS3Client.GetObjectTaggingAsync(
                new GetObjectTaggingRequest
                {
                    BucketName = bucketName,
                    Key = requestKey,
                }, ct);
            tagResponse.EnsureSuccess();

            return new FileStreamQueryResult
            {
                Key = key,
                Folder = folder,
                File = new S3FileStreamResult
                {
                    ContentStream = response.ResponseStream,
                    ContentType = contentType,
                    FileName = fileName,
                    Metadata = GetMetadata(response.Metadata).AsEnumerable(),
                },
                FileTag = new FileTag
                {
                    Tags = GetTags(tagResponse.Tagging).AsEnumerable()
                }
            };
        }

        private async Task<string> UpdateTags(UpdateTagsCommand cmd, CancellationToken cancellationToken)
        {
            var folder = cmd.Folder == null ? "" : $"{cmd.Folder}/";
            var key = $"{folder}{cmd.Key}";

            var request = new PutObjectTaggingRequest
            {
                Key = key,
                BucketName = bucketName,
                Tagging = new Tagging { TagSet = GetTagSet(cmd.FileTag?.Tags ?? []) }
            };

            var response = await _amazonS3Client.PutObjectTaggingAsync(request, cancellationToken);
            response.EnsureSuccess();

            return cmd.Key;
        }

        private static List<Amazon.S3.Model.Tag> GetTagSet(IEnumerable<Tag> tags)
            =>
            tags.Select(tag => new Amazon.S3.Model.Tag()
            {
                Key = tag.Key,
                Value = tag.Value
            }).ToList();

        private static List<FileMetadata> GetMetadata(MetadataCollection mc) =>
            mc.Keys.Where(key => key != "contenttype" && key != "fileName")
                .Select(key => new FileMetadata { Key = key, Value = mc[key] })
                .ToList();

        private static List<Tag> GetTags(List<Amazon.S3.Model.Tag> tags) =>
            tags.ConvertAll(tag => new Tag { Key = tag.Key, Value = tag.Value });

        //File name might be base64 encoded - so return correct result
        private static string GetSafeFileName(string possiblyBase64)
        {
            if (IsBase64String(possiblyBase64))
            {
                try
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(possiblyBase64));
                }
                catch
                {
                    // fallback if something went wrong
                    return possiblyBase64;
                }
            }
            return possiblyBase64;
        }

        private static bool IsBase64String(string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s.Length % 4 != 0)
                return false;

            try
            {
                Convert.FromBase64String(s);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    internal static class S3ClientEx
    {
        public static void EnsureSuccess(this AmazonWebServiceResponse response)
        {
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                throw new InvalidOperationException($"Operation failed with status {response.HttpStatusCode}");
        }

        public static void EnsureNoContent(this AmazonWebServiceResponse response)
        {
            if (response.HttpStatusCode != System.Net.HttpStatusCode.NoContent)
                throw new InvalidOperationException($"Delete Operation failed with status {response.HttpStatusCode}");
        }
    }
}
