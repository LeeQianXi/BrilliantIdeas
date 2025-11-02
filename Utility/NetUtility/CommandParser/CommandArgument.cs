namespace NetUtility.CommandParser;

internal partial record CommandArgument(CommandArgument.ArgumentType ArgType, string ArgName, string Value = "")
{
    public string ArgName { get; set; } = ArgName;
    public string Value { get; set; } = Value;
    public ArgumentType ArgType { get; set; } = ArgType;
}