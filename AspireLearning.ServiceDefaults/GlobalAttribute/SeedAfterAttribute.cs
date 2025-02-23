namespace AspireLearning.ServiceDefaults.GlobalAttribute;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SeedAfterAttribute : Attribute
{
    public string SeederName { get; }

    public SeedAfterAttribute(string seederName)
    {
        SeederName = seederName;
    }
}