namespace Jellyfin.Plugin.AlexaSkill.Exceptions;

/// <summary>
/// Parsing exception thrown when a json file can not be parsed to an object.
/// </summary>
public class JsonParsingException : System.Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonParsingException"/> class.
    /// </summary>
    /// <param name="message">Message of the exception.</param>
    public JsonParsingException(string message) : base(message)
    {
    }
}
