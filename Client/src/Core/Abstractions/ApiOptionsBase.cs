namespace AlertOne.Core.Abstractions;

public abstract class ApiOptionsBase
{
    public string BaseUri { get; set; } = string.Empty;

    public string FunctionKey { get; set; } = string.Empty;
}
