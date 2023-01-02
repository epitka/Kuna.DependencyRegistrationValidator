using Kuna.Extensions.DependencyInjection.Validation.Model;

namespace Kuna.Extensions.DependencyInjection.Validation.Exceptions;

public class FailureException : Exception
{
    public FailureException(Result result)
    {
        this.Result = result;
    }

    public Result Result { get; }
}