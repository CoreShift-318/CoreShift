namespace CoreShift.Data.Content;

public sealed class ContentLoadException : Exception
{
    public ContentLoadException(string message) : base(message) { }
    public ContentLoadException(string message, Exception inner) : base(message, inner) { }
}
