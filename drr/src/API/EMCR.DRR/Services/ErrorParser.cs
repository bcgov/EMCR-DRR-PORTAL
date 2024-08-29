using System.Net;
using System.Runtime.Serialization;
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
        public ActionResult Parse(Exception ex)
        {
            return ex switch
            {
                NotFoundException e => new NotFoundObjectResult(new ProblemDetails { Type = "NotFoundException", Title = "Not Found", Detail = e.Message }) { StatusCode = (int)HttpStatusCode.NotFound },
                ForbiddenException e => new ObjectResult(new ProblemDetails { Type = "ForbiddenException", Title = "Forbidden", Detail = e.Message }) { StatusCode = (int)HttpStatusCode.Forbidden },
                _ => new BadRequestObjectResult(new ProblemDetails { Type = "Unknown", Title = "Unexpected error", Detail = ex.Message }) { StatusCode = (int)HttpStatusCode.BadRequest },
            };
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
