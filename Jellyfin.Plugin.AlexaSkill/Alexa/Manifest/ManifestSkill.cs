using System;
using System.Collections.Generic;
using Alexa.NET.Management;
using Alexa.NET.Management.Api;
using Alexa.NET.Management.Internals;
using Alexa.NET.Management.Manifest;
using Alexa.NET.Management.Skills;
using Jellyfin.Plugin.AlexaSkill.Alexa.Interface;
using Jellyfin.Plugin.AlexaSkill.Controller;

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
    /// <param name="serverAddress">Server address.</param>
    /// <param name="sslCertType">SSL certificate type.</param>
    public ManifestSkill(string ressourcePath, string serverAddress, SslCertificateType sslCertType)
    {
        CustomApiInterfaceConverter.InterfaceLookup = new Dictionary<string, Func<CustomApiInterface>>
        {
            { "ALEXA_EXTENSION", () => new ExtensionInterface() },
            { "AUDIO_PLAYER", () => new AudioPlayerInterface() },
            { "VIDEO_APP", () => new VideoAppInterface() }
        };

        Manifest = Util.DeserializeFromFile<Skill>(ressourcePath).Manifest;
        AddVersionTag();

        SetApiEndpoint(serverAddress, sslCertType);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestSkill"/> class.
    /// </summary>
    /// <param name="manifest">Manifest of the skill.</param>
    /// <param name="serverAddress">Server address.</param>
    /// <param name="sslCertType">SSL certificate type.</param>
    public ManifestSkill(SkillManifest manifest, string serverAddress, SslCertificateType sslCertType)
    {
        CustomApiInterfaceConverter.InterfaceLookup = new Dictionary<string, Func<CustomApiInterface>>
        {
            { "ALEXA_EXTENSION", () => new ExtensionInterface() },
            { "AUDIO_PLAYER", () => new AudioPlayerInterface() },
            { "VIDEO_APP", () => new VideoAppInterface() }
        };

        Manifest = manifest;
        AddVersionTag();

        SetApiEndpoint(serverAddress, sslCertType);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestSkill"/> class.
    /// </summary>
    /// <param name="manifest">Manifest of the skill.</param>
    public ManifestSkill(SkillManifest manifest)
    {
        CustomApiInterfaceConverter.InterfaceLookup = new Dictionary<string, Func<CustomApiInterface>>
        {
            { "ALEXA_EXTENSION", () => new ExtensionInterface() },
            { "AUDIO_PLAYER", () => new AudioPlayerInterface() },
            { "VIDEO_APP", () => new VideoAppInterface() }
        };

        Manifest = manifest;
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
            return split[split.Length - 1].Replace("v", string.Empty, StringComparison.OrdinalIgnoreCase);
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
                    ((CustomApi)Manifest.Apis[i]).Endpoint = null;
                    return;
                }
            }

            return;
        }

        Uri endpointUri = new Uri(new Uri(uri), AlexaSkillController.ApiBaseUri);
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