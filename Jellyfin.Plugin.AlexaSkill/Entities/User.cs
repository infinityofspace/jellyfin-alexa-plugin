#pragma warning disable CS8618

namespace Jellyfin.Plugin.AlexaSkill.Entities;

/// <summary>
/// Represents a skill user.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets Id of the user, equal to the Jellyfin username.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the token for the Jellyfin API.
    /// </summary>
    public string Token { get; set; }
}