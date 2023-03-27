using System;

namespace Jellyfin.Plugin.AlexaSkill.Lwa;

/// <summary>
/// Auth scopes for lwa.
/// </summary>
public static class ScopeMethods
{
    /// <summary>
    /// Converts a scope to a string.
    /// </summary>
    /// <param name="scope">The scope to convert.</param>
    /// <returns>The string representation of the scope.</returns>
    public static string ScopeToString(Scope scope)
    {
        switch (scope)
        {
            case Scope.SkillsRead:
                return "alexa::ask:skills:read";
            case Scope.SkillsReadWrite:
                return "alexa::ask:skills:readwrite";
            case Scope.ModelsRead:
                return "alexa::ask:models:read";
            case Scope.ModelsReadWrite:
                return "alexa::ask:models:readwrite";
            default:
                throw new ArgumentException("Scope value can not be found");
        }
    }
}
