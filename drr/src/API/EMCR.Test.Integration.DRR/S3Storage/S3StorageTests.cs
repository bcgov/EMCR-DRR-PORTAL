using System.Text;
using EMCR.DRR.API.Services.S3;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EMCR.Tests.Integration.DRR.S3Storage
{
    public class S3StorageTests
    {
        //private string TestPrefix = "autotest-dev";

        [Test]
        public async Task CanUploadFileToS3()
        {
            var host = Application.Host;
            var storageProvider = host.Services.GetRequiredService<IS3Provider>();

            var body = DateTime.Now.ToString();
            var key = "autotest-file-key";
            var fileName = "ᓀᐦᐃᔭᐍᐏᐣ (Nēhiyawēwin).txt";
            byte[] bytes = Encoding.ASCII.GetBytes(body);
            var file = new S3File { FileName = fileName, Content = bytes, ContentType = "text/plain", };

            var ret = await storageProvider.HandleCommand(new UploadFileCommand { Folder = "autotest-dev", Key = key, File = file });
            ret.ShouldBe(key);

            var uploadedFile = await storageProvider.HandleQuery(new FileQuery { Key = key, Folder = "autotest-dev" });
            uploadedFile.ShouldNotBeNull().ShouldBeOfType<FileQueryResult>();
            var fileRes = (FileQueryResult)uploadedFile;
            fileRes.File.FileName.ShouldBe(fileName);
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        [Test]
        public async Task CanAddTags()
        {
            var host = Application.Host;
            var storageProvider = host.Services.GetRequiredService<IS3Provider>();

            var body = DateTime.Now.ToString();
            var fileName = "test-tags.txt";
            byte[] bytes = Encoding.ASCII.GetBytes(body);
            var file = new S3File { FileName = fileName, Content = bytes, ContentType = "text/plain", };

            var ret = await storageProvider.HandleCommand(new UploadFileCommand { Folder = "autotest-dev", Key = fileName, File = file });
            ret.ShouldBe(fileName);

            var uploadedFile = (FileQueryResult)await storageProvider.HandleQuery(new FileQuery { Key = fileName, Folder = "autotest-dev" });
            uploadedFile.ShouldNotBeNull().ShouldBeOfType<FileQueryResult>();
            uploadedFile.FileTag.Tags.ShouldBeEmpty();

            await storageProvider.HandleCommand(new UpdateTagsCommand { Key = fileName, Folder = "autotest-dev", FileTag = new FileTag { Tags = new[] { new Tag { Key = "Deleted", Value = "true" } } } });

            var taggedFile = (FileQueryResult)await storageProvider.HandleQuery(new FileQuery { Key = fileName, Folder = "autotest-dev" });
            taggedFile.FileTag.Tags.ShouldHaveSingleItem().Key.ShouldBe("Deleted");
        }

        [Test]
        public async Task CanUpdateTags()
        {
            var host = Application.Host;
            var storageProvider = host.Services.GetRequiredService<IS3Provider>();

            var body = DateTime.Now.ToString();
            var fileName = "test-tags.txt";
            byte[] bytes = Encoding.ASCII.GetBytes(body);
            var file = new S3File { FileName = fileName, Content = bytes, ContentType = "text/plain", };

            var ret = await storageProvider.HandleCommand(new UploadFileCommand { Folder = "autotest-dev", Key = fileName, File = file, FileTag = new FileTag { Tags = new[] { new Tag { Key = "Deleted", Value = "true" } } } });
            ret.ShouldBe(fileName);

            var uploadedFile = (FileQueryResult)await storageProvider.HandleQuery(new FileQuery { Key = fileName, Folder = "autotest-dev" });
            uploadedFile.ShouldNotBeNull().ShouldBeOfType<FileQueryResult>();
            uploadedFile.FileTag.Tags.ShouldHaveSingleItem().Key.ShouldBe("Deleted");
            uploadedFile.FileTag.Tags.ShouldHaveSingleItem().Value.ShouldBe("true");

            await storageProvider.HandleCommand(new UpdateTagsCommand { Key = fileName, Folder = "autotest-dev", FileTag = new FileTag { Tags = new[] { new Tag { Key = "Deleted", Value = "false" } } } });

            var untaggedFile = (FileQueryResult)await storageProvider.HandleQuery(new FileQuery { Key = fileName, Folder = "autotest-dev" });
            untaggedFile.FileTag.Tags.ShouldHaveSingleItem().Key.ShouldBe("Deleted");
            untaggedFile.FileTag.Tags.ShouldHaveSingleItem().Value.ShouldBe("false");
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
