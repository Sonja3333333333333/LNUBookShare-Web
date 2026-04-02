using System;

namespace LNUBookShare.Application.Common
{
    public class Result
    {
        protected Result(bool isSuccess, string error)
        {
            if (isSuccess && !string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException("Успішний результат не може містити помилку.");
            }

            if (!isSuccess && string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException("Помилковий результат повинен містити повідомлення про помилку.");
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public string Error { get; }

        public static Result Success() => new Result(true, string.Empty);

        public static Result Failure(string error) => new Result(false, error);
    }
}