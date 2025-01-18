using System;

namespace MetaParser.Models;

public sealed class MetaParserException : Exception
{
    public MetaParserException(string message) : base(message)
    {
        Expected = null;
        Actual = null;
    }

    public MetaParserException(string message, string expected, string actual)
        : base($"{message} (Expected: '{expected}'; Actual: '{actual}')")
    { }

    public MetaParserException(string message, Type expectedType, string actual)
        : base($"{message} (Expected: <{expectedType.Name}>; Actual: '{actual}')")
    { }

    public string Expected { get; init; }

    public string Actual { get; init; }
}
