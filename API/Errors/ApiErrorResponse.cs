namespace API.Errors;

public sealed record ApiErrorResponse(int StatusCode, string Message, string? Details);
