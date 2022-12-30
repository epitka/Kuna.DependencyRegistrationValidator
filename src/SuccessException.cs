namespace Kuna.DependencyRegistrationValidator;

public class SuccessException : Exception
{
    public SuccessException(Result result)
    {
        this.Result = result;
    }

    public Result Result { get; }
}
