namespace SharedKernel.Common;

public sealed record DomainError(string Code, string Message)
{
    public static DomainError Validation(string message) => new("validation_error", message);
    public static DomainError Unauthorized(string message = "Unauthorized request") => new("unauthorized", message);
}
