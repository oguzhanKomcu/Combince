
using System.Net;

namespace Combince.Modules.PostComments.Core.Common;

public class Result
{
    protected Result(bool isSuccess, string error, HttpStatusCode statusCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public HttpStatusCode StatusCode { get; }

    public static Result Success()
    {
        return new Result(true, string.Empty, HttpStatusCode.OK);
    }

    public static Result Failure(string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return new Result(false, error, statusCode);
    }
}

public class Result<T> : Result
{
    private Result(T? value, bool isSuccess, string error, HttpStatusCode statusCode)
        : base(isSuccess, error, statusCode)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value, true, string.Empty, HttpStatusCode.OK);
    }

    public static new Result<T> Failure(string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return new Result<T>(default, false, error, statusCode);
    }
}