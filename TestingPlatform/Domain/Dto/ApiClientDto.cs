namespace TestingPlatform.Domain.Dto
{
    public sealed class ApiClientResponse
    {
        public abstract class ResponseBase
        {
            public string? Status { get; init; }

            public string? Message { get; init; }

            public ResponseBase(string? status = null, string? message = null)
            {
                Status = status;
                Message = message;
            }
        }

        public sealed class Authentication(string? status = null, string? message = null) : ResponseBase(status, message) { }

        public sealed class Time : ResponseBase
        {
            public TimeData? Data { get; init; }

            public Time(string? status = null, string? message = null, TimeData? data = null)
                : base(status, message)
            {
                Data = data;
            }

            public sealed record TimeData(string? Time);
        }
    }

    public sealed class ApiClientRequest
    {
        public sealed record Authentication(
                string Id,
                string DisplayName,
                string Name,
                string Surname,
                string Mail
            );
    }
}