using CoreShift.Core.Progression;

namespace CoreShift.Data.Serialization;

public class SaveLoadException : Exception
{
    public SaveLoadException(string message) : base(message) { }
    public SaveLoadException(string message, Exception inner) : base(message, inner) { }
}

public sealed class SaveVersionException : SaveLoadException
{
    public SaveVersionException(string message) : base(message) { }
}

internal static class SaveValidation
{
    public static void CheckVersion(SaveData data, string source)
    {
        if (data.SchemaVersion > SaveData.CurrentSchemaVersion)
        {
            throw new SaveVersionException(
                "Save '" + source + "' uses schema version " + data.SchemaVersion +
                ", but this build supports only up to version " + SaveData.CurrentSchemaVersion + ".");
        }
    }
}
