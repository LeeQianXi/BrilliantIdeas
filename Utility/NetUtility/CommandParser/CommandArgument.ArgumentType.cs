namespace NetUtility.CommandParser;

internal partial record CommandArgument
{
    public enum ArgumentType
    {
        Default,
        Switch,
        Optional,
        Required
    }
}