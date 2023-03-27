namespace Jellyfin.Plugin.AlexaSkill.Entities;

/// <summary>
/// Represents the status of a skill.
/// </summary>
public enum UserSkillStatus
{
    /// <summary>
    /// Authentication for LWA is pending.
    /// </summary>
    LwaAuthPending,

    /// <summary>
    /// Skill is building.
    /// </summary>
    SkillCreating,

    /// <summary>
    /// Linking the Alexa and Jellyfin account is pending.
    /// </summary>
    AccountLinkPending,

    /// <summary>
    /// Skill is ready.
    /// </summary>
    Ready
}

/// <summary>
/// Represents a skill user.
/// </summary>
public class UserSkill
{
    /// <summary>
    /// Gets or sets the skill id.
    /// </summary>
    public string? SkillId { get; set; }

    /// <summary>
    /// Gets or sets the skill status.
    /// </summary>
    public UserSkillStatus UserSkillStatus { get; set; }

    /// <summary>
    /// Gets or sets the invocation name.
    /// </summary>
    public string InvocationName { get; set; }
}