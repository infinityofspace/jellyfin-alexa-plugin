using System;

namespace Jellyfin.Plugin.AlexaSkill.Lwa;

/// <summary>
/// Auth scopes for lwa.
/// </summary>
public enum Scope
{
    /// <summary>
    /// Read only access to skills.
    /// </summary>
    SkillsRead = 0,

    /// <summary>
    /// Read and write access to skills.
    /// </summary>
    SkillsReadWrite = 1,

    /// <summary>
    /// Read only access to interaction  models.
    /// </summary>
    ModelsRead = 2,

    /// <summary>
    /// Read and write access to interaction models.
    /// </summary>
    ModelsReadWrite = 3
}