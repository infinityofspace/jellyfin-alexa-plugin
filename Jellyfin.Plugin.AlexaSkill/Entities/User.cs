#pragma warning disable CS8618

using System;
using System.Text.Json.Serialization;
using Jellyfin.Plugin.AlexaSkill.Alexa;
using Jellyfin.Plugin.AlexaSkill.Lwa;

namespace Jellyfin.Plugin.AlexaSkill.Entities;

/// <summary>
/// Represents a skill user.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets Id of the user, equal to the Jellyfin id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets the username of the user, equal to the Jellyfin username.
    /// </summary>
    public string Username
    {
        get
        {
            if (Id == Guid.Empty)
            {
                return string.Empty;
            }

            return Plugin.Instance!.UserManager.GetUserById(Id).Username;
        }
    }

    /// <summary>
    /// Gets or sets the token for the Jellyfin API.
    /// </summary>
    [JsonIgnore]
    public string? JellyfinToken { get; set; }

    /// <summary>
    /// Gets or sets the device token for accessing SMAPI.
    /// </summary>
    [JsonIgnore]
    public DeviceToken? SmapiDeviceToken { get; set; }

    /// <summary>
    /// Gets or sets the user skill.
    /// </summary>
    public UserSkill? UserSkill { get; set; }

    /// <summary>
    /// Gets or sets the skill status.
    /// </summary>
    public UserSkillStatus? UserSkillStatus { get; set; }

    /// <summary>
    /// Gets or sets the invocation name.
    /// </summary>
    public string InvocationName { get; set; }

    /// <summary>
    /// Gets the smapi Smapi Management object for this user.
    /// </summary>
    [JsonIgnore]
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