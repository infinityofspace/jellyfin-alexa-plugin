using Jellyfin.Plugin.AlexaSkill.Entities;

namespace Jellyfin.Plugin.AlexaSkill.Configuration;

/// <summary>
/// Represents a skill user  on the configuration page.
/// </summary>
public class ConfigUserSkill
{
    /// <summary>
    /// Gets or sets the skill id.
    /// </summary>
    public string SkillId { get; set; }

    /// <summary>
    /// Gets or sets the skill status.
    /// </summary>
    public UserSkillStatus SkillStatus { get; set; }

    /// <summary>
    /// Gets or sets the invocation name.
    /// </summary>
    public string InvocationName { get; set; }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public string UserId { get; set; }
}