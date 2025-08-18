using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.Crmf;

namespace EventApi.ExeptionHandling
{
    public record Error(string Code, string Description);
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public Error Error { get; }

        private Result(T value)
        {
            IsSuccess = true;
            Value = value;
            Error = default;
        }

        private Result(Error error)
        {
            IsSuccess = false;
            Value = default;
            Error = error;
        }

        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(Error error) => new(error);
    }

    public class Result
    {
        public bool IsSuccess { get; }
        public Error Error { get; }

        private Result(bool isSuccess,Error error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new (true, default);
        public static Result Failure(Error error) => new(false,error);
    }
}