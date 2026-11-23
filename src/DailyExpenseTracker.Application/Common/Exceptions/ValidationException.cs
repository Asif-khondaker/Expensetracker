namespace DailyExpenseTracker.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>
        {
            { string.Empty, new[] { message } }
        };
    }

    public ValidationException(string propertyName, string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { message } }
        };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }
}
