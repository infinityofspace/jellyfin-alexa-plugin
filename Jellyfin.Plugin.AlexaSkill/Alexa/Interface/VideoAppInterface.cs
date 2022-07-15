using Alexa.NET.Management.Api;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Interface;

/// <summary>
/// Video App Interface for custom api interface.
/// </summary>
public class VideoAppInterface : CustomApiInterface
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VideoAppInterface"/> class.
    /// </summary>
    public VideoAppInterface()
    {
    }

    /// <summary>
    /// Gets the type of the interface.
    /// </summary>
    [JsonProperty("type")]
    public override string Type { get; } = "VIDEO_APP";
}
