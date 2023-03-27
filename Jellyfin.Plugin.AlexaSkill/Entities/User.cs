#pragma warning disable CS8618

using System;
using Jellyfin.Plugin.AlexaSkill.Alexa;
using Jellyfin.Plugin.AlexaSkill.Lwa;

namespace Jellyfin.Plugin.AlexaSkill.Entities;

/// <summary>
/// Represents a skill user.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets Id of the user, equal to the Jellyfin username.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the token for the Jellyfin API.
    /// </summary>
    public string JellyfinToken { get; set; }

    /// <summary>
    /// Gets or sets the device token for accessing SMAPI.
    /// </summary>
    public DeviceToken SmapiDeviceToken { get; set; }

    /// <summary>
    /// Gets or sets the user skill.
    /// </summary>
    public UserSkill UserSkill { get; set; }

    /// <summary>
    /// Gets the smapi Smapi Management object for this user.
    /// </summary>
    public SmapiManagement? SmapiManagement
    {
        get
        {
            if (this.SmapiDeviceToken == null)
            {
                return null;
            }
            
            return new SmapiManagement(this.SmapiDeviceToken);
        }
    }
}