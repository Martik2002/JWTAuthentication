using System.Net;

namespace JWTAuthentication.Common.Exception;

public class RestApiResponseException(int errorCode, string message) : System.Exception(message)
{
    public RestApiResponseException(string message) : this((int) HttpStatusCode.BadRequest, message)
    {
            
    }

    public int ErrorCode { get; } = errorCode;
}