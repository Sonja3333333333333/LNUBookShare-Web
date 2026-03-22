using System;
using System.Collections.Generic;
using System.Text;

namespace LNUBookShare.Application.Common
{
    public class Result<T> : Result
    {
        private readonly T? _value;

        protected Result(T? value, bool isSuccess, string error)
           : base(isSuccess, error)
        {
            _value = value;
        }

        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Не можна отримати значення з результату з помилкою.");

        public static Result<T> Success(T value) => new Result<T>(value, true, string.Empty);
        public static new Result<T> Failure(string error) => new Result<T>(default, false, error);
    }
}
