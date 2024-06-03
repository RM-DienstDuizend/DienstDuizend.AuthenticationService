using System.Net;
using ApplicationException = DienstDuizend.AuthenticationService.Infrastructure.Exceptions.ApplicationException;

namespace DienstDuizend.AuthenticationService.Infrastructure.Exceptions;


public static class Error
{
    public static ApplicationException Failure(string code, string description) =>
        new(code, description, HttpStatusCode.BadRequest);

    public static ApplicationException Unexpected(string code, string description) =>
        new(code, description, HttpStatusCode.BadRequest);

    public static ApplicationException Validation(string code, string description) =>
        new(code, description, HttpStatusCode.BadRequest);

    public static ApplicationException Conflict(string code, string description) =>
        new(code, description, HttpStatusCode.Conflict);

    public static ApplicationException NotFound(string code, string description) =>
        new(code, description, HttpStatusCode.NotFound);
    
    public static ApplicationException Forbidden(string code, string description) =>
        new(code, description, HttpStatusCode.Forbidden);
    
    public static ApplicationException InternalError(string code, string description) =>
        new(code, description, HttpStatusCode.InternalServerError);

    public static ApplicationException NotFound<T>()
    {
        var name = typeof(T).FullName.Split(".").Last();
        
        return new(
            $"{name}.NotFound", 
            $"{name} not found.", 
            HttpStatusCode.NotFound
            );
    }
        
        
    public static ApplicationException Custom(HttpStatusCode statusCode, string code, string description) =>
        new(code, description, statusCode);
}
