namespace NetUtility.CommandParser;

public partial class CommandLineParser
{
    private readonly bool _enableParamArgs;
    private readonly Dictionary<string, string> _optionalArgs;
    private readonly List<string> _requiredParam;

    private readonly List<string> _switchArgs;

    private CommandLineParser(CommandLineParserBuilder builder)
    {
        _switchArgs = builder.SwitchArgs;
        _optionalArgs = builder.OptionalArgs;
        _enableParamArgs = builder.EnableParamArgs;
        _requiredParam = builder.RequiredParam;
    }

    public CommandParseResult Parse(params string[] args)
    {
        List<CommandArgument> rets = [];
        var isDefault = false;
        for (var i = 0; i < args.Length; i++)
        {
            if (isDefault)
            {
                rets.Add(new CommandArgument(CommandArgument.ArgumentType.Default, args[i]));
                continue;
            }

            switch (CheckType(args[i], out var dv))
            {
                case CommandArgument.ArgumentType.Switch:
                    rets.Add(new CommandArgument(CommandArgument.ArgumentType.Switch, args[i]));
                    continue;
                case CommandArgument.ArgumentType.Optional:
                    rets.Add(new CommandArgument(CommandArgument.ArgumentType.Optional, args[i],
                        i + 1 < args.Length ? args[i + 1] : dv));
                    i++;
                    continue;
                case CommandArgument.ArgumentType.Required:
                    rets.Add(new CommandArgument(CommandArgument.ArgumentType.Required, args[i],
                        i + 1 < args.Length
                            ? args[i + 1]
                            : throw new ArgumentException($"key {args[i]} is required.")));
                    i++;
                    continue;
                case CommandArgument.ArgumentType.Default:
                default:
                    rets.Add(new CommandArgument(CommandArgument.ArgumentType.Default, args[i]));
                    isDefault = true;
                    continue;
            }
        }

        if (_requiredParam.Count is not 0) throw new ArgumentException($"key {_requiredParam[0]} is required.");

        rets.AddRange(_optionalArgs.Select(optionalArg =>
            new CommandArgument(CommandArgument.ArgumentType.Optional, optionalArg.Key, optionalArg.Value)));

        return new CommandParseResult(rets, _enableParamArgs);
    }

    private CommandArgument.ArgumentType CheckType(string name, out string defaultValue)
    {
        if (_switchArgs.Contains(name))
        {
            defaultValue = string.Empty;
            _switchArgs.Remove(name);
            return CommandArgument.ArgumentType.Switch;
        }

        if (_optionalArgs.TryGetValue(name, out var value))
        {
            defaultValue = value;
            _optionalArgs.Remove(name);
            return CommandArgument.ArgumentType.Optional;
        }

        if (_requiredParam.Contains(name))
        {
            defaultValue = string.Empty;
            _requiredParam.Remove(name);
            return CommandArgument.ArgumentType.Required;
        }

        defaultValue = string.Empty;
        return CommandArgument.ArgumentType.Default;
    }

    public static CommandLineParserBuilder Builder()
    {
        return new CommandLineParserBuilder();
    }

    public void ShowHelp()
    {
    }

    public partial class CommandLineParserBuilder
    {
        private static readonly Regex ArgNameRegex = ArgNameRegexGenerator();

        internal readonly Dictionary<string, string> OptionalArgs = new();

        internal readonly List<string> RequiredParam = [];

        internal readonly List<string> SwitchArgs = [];

        internal bool EnableParamArgs;

        internal CommandLineParserBuilder()
        {
        }

        public CommandLineParser Build()
        {
            return new CommandLineParser(this);
        }

        public CommandLineParserBuilder AddSwitchArgument(string argName)
        {
            if (!ArgNameRegex.IsMatch(argName))
                throw new ArgumentException($"Invalid argument name: {argName}");
            SwitchArgs.Add($"-{argName}");
            return this;
        }

        public CommandLineParserBuilder AddOptionalArguments(string argName, string defaultValue)
        {
            if (!ArgNameRegex.IsMatch(argName))
                throw new ArgumentException($"Invalid argument name: {argName}");
            OptionalArgs[$"--{argName}"] = defaultValue;
            return this;
        }

        public CommandLineParserBuilder AddRequiredArguments(string argName)
        {
            if (!ArgNameRegex.IsMatch(argName))
                throw new ArgumentException($"Invalid argument name: {argName}");
            RequiredParam.Add($"--{argName}");
            return this;
        }

        public CommandLineParserBuilder AddParamArguments()
        {
            EnableParamArgs = true;
            return this;
        }

        [GeneratedRegex("^[a-zA-Z0-9_]+[a-zA-Z0-9_-]*$")]
        private static partial Regex ArgNameRegexGenerator();
    }
}