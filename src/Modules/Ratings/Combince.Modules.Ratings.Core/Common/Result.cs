using System.Net;

namespace Combince.Modules.Ratings.Core.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    public HttpStatusCode StatusCode { get; }

    private Result(bool isSuccess, T value, string error, HttpStatusCode statusCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty, HttpStatusCode.OK);
    public static Result<T> Failure(string error, HttpStatusCode statusCode) => new(false, default!, error, statusCode);
}