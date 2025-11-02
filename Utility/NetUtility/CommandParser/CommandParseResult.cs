namespace NetUtility.CommandParser;

public record CommandParseResult
{
    private readonly Dictionary<string, CommandArgument> _argumentMap;

    private readonly bool _enableParamArgs;

    internal CommandParseResult(IEnumerable<CommandArgument> arguments, bool enableParamArgs)
    {
        _enableParamArgs = enableParamArgs;
        _argumentMap = arguments.ToDictionary(a => a.ArgName, a => a);
    }

    public bool HasSwitchArg(string argName)
    {
        if (!_argumentMap.TryGetValue($"-{argName}", out var value))
            return false;
        return value.ArgType is CommandArgument.ArgumentType.Switch
            ? true
            : throw new ArgumentException($"Argument '-{argName}' is not a valid switch");
    }

    public string GetOptionalArg(string argName)
    {
        if (!_argumentMap.TryGetValue($"--{argName}", out var value))
            throw new ArgumentException($"Argument '--{argName}' is not a valid option");
        return value.ArgType is CommandArgument.ArgumentType.Optional
            ? value.Value
            : throw new ArgumentException($"Argument '--{argName}' is not a valid option");
    }

    public string GetRequiredArg(string argName)
    {
        if (!_argumentMap.TryGetValue($"--{argName}", out var value))
            throw new ArgumentException($"Argument '--{argName}' is not a valid required");

        return value.ArgType is CommandArgument.ArgumentType.Required
            ? value.Value
            : throw new ArgumentException($"Argument '--{argName}' is not a valid option");
    }

    public IEnumerable<string> GetParamArgs()
    {
        foreach (var entry in _argumentMap.Values.Where(a => a.ArgType is CommandArgument.ArgumentType.Default))
        {
            yield return entry.ArgName;
            if (!_enableParamArgs) yield break;
        }
    }
}