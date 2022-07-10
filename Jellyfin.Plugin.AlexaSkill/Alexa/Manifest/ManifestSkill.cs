using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alexa.NET.Management;
using Alexa.NET.Management.Api;
using Alexa.NET.Management.Manifest;
using Alexa.NET.Management.Skills;
using Jellyfin.Plugin.AlexaSkill.Api;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Manifest;

/// <summary>
/// Represents a Alexa skill.
/// </summary>
public class ManifestSkill : Skill
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestSkill"/> class.
    /// </summary>
    /// <param name="ressourcePath">Path to the manifest ressource.</param>
    public ManifestSkill(string ressourcePath)
    {
        Manifest = Util.DeserializeFromFile<Skill>(ressourcePath).Manifest;
        AddVersionTag();

        if (Plugin.Instance != null && !string.IsNullOrEmpty(Plugin.Instance!.Configuration.ServerAddress))
        {
            SetApiEndpoint(Plugin.Instance!.Configuration.ServerAddress, Plugin.Instance.Configuration.SslCertType);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestSkill"/> class.
    /// </summary>
    /// <param name="manifest">Manifest of the skill.</param>
    public ManifestSkill(SkillManifest manifest)
    {
        Manifest = manifest;
        AddVersionTag();

        if (Plugin.Instance != null && !string.IsNullOrEmpty(Plugin.Instance.Configuration.ServerAddress))
        {
            SetApiEndpoint(Plugin.Instance.Configuration.ServerAddress, Plugin.Instance.Configuration.SslCertType);
        }
    }

    /// <summary>
    /// Update the skill manifest in the Alexa cloud and return a Uri to check the build status of the skill.
    /// </summary>
    /// <param name="skillId">ID of the skill.</param>
    /// <param name="stage">Stage of the skill.</param>
    /// <returns>Skill id object of the updated skill.</returns>
    public Task<SkillId> Update(string skillId, SkillStage stage = SkillStage.Development)
    {
        return Plugin.Instance!.SmapiManagement!.Skills.Update(skillId, stage, this);
    }

    /// <summary>
    /// Create a new skill in the Alexa cloud.
    /// </summary>
    /// <param name="vendorId">The vendor ID for which the skill will be created.</param>
    /// <returns>The ID of the newly created skill or null if something went wrong.</returns>
    public Task<SkillId> CreateSkill(string vendorId)
    {
        return Plugin.Instance!.SmapiManagement!.Skills.Create(vendorId, this);
    }

    /// <summary>
    /// Add the current version of this skill to the name of the skill.
    /// </summary>
    private void AddVersionTag()
    {
        string version = Util.GetVersion();

        foreach (KeyValuePair<string, Locale> l in Manifest.PublishingInformation.Locales)
        {
            if (l.Value != null)
            {
                l.Value.Name += " v" + version;
            }
        }
    }

    /// <summary>
    /// Add the current version of this skill to the name of the skill.
    /// </summary>
    /// <returns>The version tag of the skill.</returns>
    public string GetVersionTag()
    {
        Locale? baseLocale = Manifest.PublishingInformation.Locales.GetValueOrDefault<string, Locale?>("en-US", null);
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

    /// <summary>
    /// Set the api endpoint of the skill.
    /// </summary>
    /// <param name="uri">Uri of the api server.</param>
    /// <param name="certificateType">Certificate type of the endpoint.</param>
    public void SetApiEndpoint(string uri, SslCertificateType certificateType)
    {
        if (string.IsNullOrEmpty(uri))
        {
            // remove the endpoint if it exists
            for (int i = 0; i < Manifest.Apis.Count; i++)
            {
                if (Manifest.Apis[i] is CustomApi)
                {
                    Manifest.Apis.RemoveAt(i);
                    return;
                }
            }

            return;
        }

        Uri endpointUri = new Uri(new Uri(uri), RequestController.ApiBaseUri);
        string endpointUriString = new Uri(endpointUri, "alexa-request").ToString();

        foreach (IApi api in Manifest.Apis)
        {
            if (api is CustomApi)
            {
                CustomApi customApi = (CustomApi)api;
                customApi.Endpoint = new Endpoint();

                customApi.Endpoint.Uri = endpointUriString;
                customApi.Endpoint.SslCertificateType = certificateType;

                return;
            }
        }

        CustomApi newCustomApi = new CustomApi();
        newCustomApi.Endpoint = new Endpoint();
        newCustomApi.Endpoint.Uri = endpointUriString;
        newCustomApi.Endpoint.SslCertificateType = certificateType;

        Manifest.Apis.Add(newCustomApi);
    }
}