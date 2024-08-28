using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace EMCR.DRR.API.Services
{
    public class ServerException : Exception
    {
        public ServerException(string type, string message) : base(message)
        {
            Type = type;
        }

        public string Type { get; }

        public override string ToString()
        {
            return $"{Type}: {Message}";
        }
    }

    [KnownType(typeof(NotFoundException))]
    public abstract class DrrApplicationException : Exception
    {
        protected DrrApplicationException(string message) : base(message)
        {
        }
    }

    public class NotFoundException : DrrApplicationException
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, string id) : base(message)
        {
            Id = id;
        }

        public string Id { get; }
    }

    public class ForbiddenException : DrrApplicationException
    {
        public ForbiddenException(string message) : base(message)
        {
        }

        public ForbiddenException(string message, string id) : base(message)
        {
            Id = id;
        }

        public string Id { get; }
    }

    public class ErrorParser
    {
        public ActionResult Parse(Exception ex, ILogger logger)
        {
            logger.LogError(ex, "Application Error");
            var details = TryGetCrmErrorDetails(ex);
            if (string.IsNullOrEmpty(details)) { details = ex.Message; }
            return ex switch
            {
                NotFoundException e => new NotFoundObjectResult(new ProblemDetails { Type = "NotFoundException", Title = "Not Found", Detail = e.Message }) { StatusCode = (int)HttpStatusCode.NotFound },
                ForbiddenException e => new ObjectResult(new ProblemDetails { Type = "ForbiddenException", Title = "Forbidden", Detail = e.Message }) { StatusCode = (int)HttpStatusCode.Forbidden },
                ArgumentNullException e => new BadRequestObjectResult(new ProblemDetails { Type = "BadRequest", Title = "Null Argument", Detail = e.Message }) { StatusCode = (int)HttpStatusCode.BadRequest },
                _ => new BadRequestObjectResult(new ProblemDetails { Type = "Unknown", Title = "Unexpected error", Detail = details }) { StatusCode = (int)HttpStatusCode.BadRequest },
            };
        }

        private string TryGetCrmErrorDetails(Exception ex)
        {
            var errorMessage = ex.InnerException?.Message ?? string.Empty;
            var regex = new System.Text.RegularExpressions.Regex("\\{(?<CRMError>.*)\\}");
            var match = regex.Match(errorMessage);
            if (match.Success) errorMessage = "{" + match.Groups["CRMError"].Value + "}";
            else return ex.Message;

            var crmError = JsonSerializer.Deserialize<CRMError>(errorMessage);
            if (crmError != null && crmError.error != null && crmError.error.message != null) { return $"CRM Error: {crmError.error.message}"; }

            return ex.Message;
        }
    }

    public class CRMError
    {
        public ErrorDetails error { get; set; }
    }

    public class ErrorDetails
    {
        public required string code { get; set; }
        public required string message { get; set; }
    }

}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
