namespace Kuna.DependencyRegistrationValidator;

public class Result
{
    private const string separator = "-----------------------------------------";

    public HashSet<string> FailureMessages { get; set; } = new();

    public HashSet<Type> FailedTypes { get; set; } = new();

    public Type[] TypesToVerify { get; set; } = Array.Empty<Type>();

    public HashSet<Type> SuccessfullyResolved { get; set; } = new();

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("Number of types to verify: " + this.TypesToVerify.Length);
        sb.AppendLine("");

        if (this.FailureMessages.Any())
        {
            sb.AppendLine("Failure messages:");
            sb.AppendLine(separator);

            foreach (var item in this.FailureMessages)
            {
                sb.AppendLine(item);
            }
        }

        sb.AppendLine("");

        if (this.FailedTypes.Any())
        {
            sb.AppendLine("Failed to resolve:");
            sb.AppendLine(separator);

            foreach (var item in this.FailedTypes)
            {
                sb.AppendLine(item.FullName);
            }
        }

        sb.AppendLine("");
        sb.AppendLine("Resolved successfuly:");
        sb.AppendLine(separator);

        foreach (var item in this.SuccessfullyResolved)
        {
            sb.AppendLine(item.FullName);
        }

        return sb.ToString();
    }
}
