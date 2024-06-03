using System.Net;

namespace DienstDuizend.AuthenticationService.Infrastructure.Exceptions;

public class ApplicationException : Exception
{
    public string ErrorCode { get; }
    public string ErrorMessage { get; }
    public HttpStatusCode StatusCode { get; }

    public ApplicationException(string errorCode, string errorMessage, HttpStatusCode statusCode)
        : base(errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
        
    }

    public ApplicationException(string errorCode, string errorMessage, HttpStatusCode statusCode, Exception innerException)
        : base(errorMessage, innerException)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }
}