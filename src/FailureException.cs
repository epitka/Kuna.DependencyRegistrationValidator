namespace Kuna.DependencyRegistrationValidator;

public class FailureException : Exception
{
    public FailureException(Result result)
    {
        this.Result = result;
    }

    public Result Result { get; }
}