using Kuna.Extensions.DependencyInjection.Validation.Exceptions;
using Kuna.Extensions.DependencyInjection.Validation.Model;

namespace Kuna.Extensions.DependencyInjection.Validation;

public class RegistrationValidator<TProgram>
    where TProgram : class
{
    private readonly IEnumerable<Assembly> assemblies;

    /// <summary>
    ///
    /// </summary>
    /// <param name="assemblies">assemblies to validate</param>
    public RegistrationValidator(IEnumerable<Assembly> assemblies)
    {
        this.assemblies = assemblies;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="typesToVerify">Types to verify if all dependencies needed to create them are registered. Concrete types of open generic interfaces are supported. Controllers will be added automatically.</param>
    /// <exception cref="FailureException">thrown if any of the types cannot be resolved from container. It must be handled in the unit test.</exception>
    /// <exception cref="SuccessException">thrown if all types could be resolved from container. It must be handled in the unit test. </exception>
    public void Validate(params Type[] typesToVerify)
    {
        var app = new WebApplicationFactory<TProgram>()
            .WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(
                        sc =>
                        {
                            var toVerify = this.GetTypesToVerify(typesToVerify)
                                               .ToList();

                            AddControllers(toVerify, sc);

                            var sp = sc.BuildServiceProvider();

                            var result = InternalValidate(
                                sp,
                                toVerify.ToArray());

                            if (result.FailedTypes.Any())
                            {
                                throw new FailureException(result);
                            }

                            throw new SuccessException(result);
                        });
                });

        app.CreateClient();
    }

    private static void AddControllers(IEnumerable<Type> toVerify, IServiceCollection sc)
    {
        // because controllers are not added to service collection
        // let's add them so we can try to resolve them
        var controllers = toVerify.Where(x => x.IsAssignableTo(typeof(Controller)));

        foreach (var controller in controllers)
        {
            sc.AddScoped(controller);
        }
    }

    private static Result InternalValidate(IServiceProvider sp, params Type[] typesToVerify)
    {
        var toReturn = new Result
        {
            TypesToVerify = typesToVerify,
        };

        using var scope = sp.CreateScope();

        foreach (var item in typesToVerify)
        {
            try
            {
                var result = scope.ServiceProvider.GetServices(item);

                if (!result.Any())
                {
                    throw new Exception($"Could not resolve services for {item.FullName}");
                }

                toReturn.SuccessfullyResolved.Add(item);
            }
            catch (Exception e)
            {
                toReturn.FailureMessages.Add(e.Message);
                toReturn.FailedTypes.Add(item);
            }
        }

        return toReturn;
    }

    private IEnumerable<Type> GetTypesToVerify(IEnumerable<Type> typesToVerify)
    {
        var toReturn = new HashSet<Type>();

        // just make sure we get all assemblies where we have command and event handlers
        var exportedTypes = this.assemblies
                                .SelectMany(a => a.GetExportedTypes())
                                .ToArray();

        foreach (var candidateType in typesToVerify)
        {
            if (candidateType.IsGenericTypeDefinition
                && candidateType.IsInterface)
            {
                // get all types that implement open generic interface type such as
                // INotificationHandler<>, IRequest<>, IRequest<,>, IHandleCommand<>, IHandleEvent<>
                var closedTypes = exportedTypes.Where(
                                                   x => x.GetInterfaces()
                                                         .Any(
                                                             y => y.IsGenericType
                                                                  && y.GetGenericTypeDefinition() == candidateType))
                                               .Where(x => !x.IsAbstract);

                foreach (var closedType in closedTypes)
                {
                    var closedInterface = closedType.GetInterfaces()
                                                    .Single(x => x.GetGenericTypeDefinition() == candidateType);

                    toReturn.Add(closedInterface);
                }
            }
            else
            {
                var types = exportedTypes.Where(x => candidateType.IsAssignableFrom(x))
                                         .Where(x => !x.IsAbstract);

                foreach (var type in types)
                {
                    toReturn.Add(type);
                }
            }
        }

        return toReturn;
    }
}
