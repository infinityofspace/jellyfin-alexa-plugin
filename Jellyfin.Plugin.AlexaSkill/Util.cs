using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Jellyfin.Plugin.AlexaSkill.Exceptions;
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
            T? val = JsonConvert.DeserializeObject<T>(json);
            if (val != null)
            {
                return val;
            }
            else
            {
                throw new JsonParsingException($"Could not parse json file {ressourcePath} to {typeof(T).Name}.");
            }
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
    /// Get all local interaction models json files of the skill.
    /// </summary>
    /// <returns>List of all interaction models.</returns>
    public static Collection<Tuple<string, string>> GetLocalInteractionModels()
    {
        // iter of all model json files in the ressource folder
        Collection<Tuple<string, string>> interactionModels = new Collection<Tuple<string, string>>();
        foreach (string ressourcePath in Assembly.GetExecutingAssembly().GetManifestResourceNames())
        {
            if (ressourcePath.StartsWith("Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel", StringComparison.Ordinal)
            && ressourcePath.Contains("model_", StringComparison.Ordinal)
            && ressourcePath.EndsWith(".json", StringComparison.Ordinal))
            {
                // Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel.model_de-DE.json
                string[] split = ressourcePath.Split(".");
                string locale = split[split.Length - 2].Split("_")[1];
                interactionModels.Add(new Tuple<string, string>(locale, ressourcePath));
            }
        }

        return interactionModels;
    }
}
