using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Alexa.NET.Management.Manifest;
using Alexa.NET.Management.Skills;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.AlexaSkill;

/// <summary>
/// Util class with some usefull methods.
/// </summary>
public static class Util
{
    /// <summary>
    /// Deserialize an object from a local json file and create a new object of class.
    /// </summary>
    /// <typeparam name="T">The type of the new object.</typeparam>
    /// <param name="ressourcePath">The path to the json file ressource.</param>
    /// <returns>A new object of the specified class.</returns>
    public static T DeserializeFromFile<T>(string ressourcePath)
    {
        if (string.IsNullOrEmpty(ressourcePath))
        {
            throw new System.ArgumentException($"'{nameof(ressourcePath)}' cannot be null or empty.", nameof(ressourcePath));
        }

        var assembly = typeof(Util).Assembly;
        Stream? resource = assembly.GetManifestResourceStream(ressourcePath);

        if (resource != null)
        {
            string json = new StreamReader(resource).ReadToEnd();
            return JsonConvert.DeserializeObject<T>(json);
        }
        else
        {
            throw new FileNotFoundException("ressource path can not be lodaded or does not exists");
        }
    }

    /// <summary>
    /// Get the current version of the skill plugin.
    /// </summary>
    /// <returns>Version of the skill plugin.</returns>
    public static string GetVersion()
    {
        return Assembly.GetExecutingAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
    }

    /// <summary>
    /// Add the current version of this skill to the name of the skill.
    /// </summary>
    /// <param name="skill">The skill where the version tag should be added to.</param>
    public static void AddVersionTag(Skill skill)
    {
        string version = GetVersion();

        foreach (KeyValuePair<string, Locale> l in skill.Manifest.PublishingInformation.Locales)
        {
            l.Value.Name += " v" + version;
        }
    }

    /// <summary>
    /// Add the current version of this skill to the name of the skill.
    /// </summary>
    /// <param name="skill">The skill where the version tag should be added to.</param>
    /// <returns>The version tag of the skill.</returns>
    public static string GetVersionTag(Skill skill)
    {
        Locale? baseLocale = skill.Manifest.PublishingInformation.Locales.GetValueOrDefault<string, Locale?>("en-US", null);
        if (baseLocale != null)
        {
            string[] split = baseLocale.Name.Split(" ");
            return split[split.Length - 1];
        }
        else
        {
            return "unknown";
        }
    }
}