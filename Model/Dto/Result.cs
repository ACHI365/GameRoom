﻿public class Result
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }

    protected Result(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success()
    {
        return new Result(true, null);
    }

    public static Result Error(string errorMessage)
    {
        return new Result(false, errorMessage);
    }
}

public class Result<T> : Result
{
    public T Data { get; }

    protected Result(bool isSuccess, string errorMessage, T data)
        : base(isSuccess, errorMessage)
    {
        Data = data;
    }

    public static Result<T> Success(T data)
    {
        return new Result<T>(true, null, data);
    }

    public new static Result<T> Error(string errorMessage)
    {
        return new Result<T>(false, errorMessage, default);
    }
}