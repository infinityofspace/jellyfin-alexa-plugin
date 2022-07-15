using Alexa.NET.Management.Api;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Interface;

/// <summary>
/// Audio Player Interface for custom api interface.
/// </summary>
public class AudioPlayerInterface : CustomApiInterface
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioPlayerInterface"/> class.
    /// </summary>
    public AudioPlayerInterface()
    {
    }

    /// <summary>
    /// Gets the type of the interface.
    /// </summary>
    [JsonProperty("type")]
    public override string Type { get; } = "AUDIO_PLAYER";
}
