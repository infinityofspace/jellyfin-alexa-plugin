using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Alexa.NET.Management;
using Jellyfin.Plugin.AlexaSkill.Alexa.Manifest;
using Jellyfin.Plugin.AlexaSkill.Entities;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AlexaSkill.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    private SslCertificateType sslCertType;
    private string serverAddress;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        // set default options here
        sslCertType = SslCertificateType.Trusted;
        LwaClientId = string.Empty;
        LwaClientSecret = string.Empty;

        serverAddress = string.Empty;
        AccountLinkingClientId = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Gets or sets the ssl cert type of the public jellyfin endpoint.
    /// </summary>
    public SslCertificateType SslCertType
    {
        get => sslCertType;
        set
        {
            sslCertType = value;

            if (Plugin.Instance!.ManifestSkill == null)
            {
                Plugin.Instance.ManifestSkill = new ManifestSkill("Jellyfin.Plugin.AlexaSkill.Alexa.Manifest.manifest.json", ServerAddress, value);
            }
            else
            {
                Plugin.Instance.ManifestSkill.SetApiEndpoint(ServerAddress, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the server address.
    /// </summary>
    public string ServerAddress
    {
        get => serverAddress;
        set
        {
            serverAddress = value;

            if (Plugin.Instance!.ManifestSkill == null)
            {
                Plugin.Instance.ManifestSkill = new ManifestSkill("Jellyfin.Plugin.AlexaSkill.Alexa.Manifest.manifest.json", value, SslCertType);
            }
            else
            {
                Plugin.Instance.ManifestSkill.SetApiEndpoint(value, SslCertType);
            }
        }
    }

    /// <summary>
    /// Gets or sets the client id for LWA.
    /// </summary>
    public string LwaClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret for LWA.
    /// </summary>
    public string LwaClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the account linking client id.
    /// </summary>
    public string AccountLinkingClientId { get; set; }

    /// <summary>
    /// Gets array if skills.
    /// </summary>
    public Collection<ConfigUserSkill> Skills
    {
        get
        {
            Collection<ConfigUserSkill> skills = new Collection<ConfigUserSkill>();
            IEnumerable<User> users = Plugin.Instance!.DbRepo.GetAllUser();
            foreach (User u in users)
            {
                if (u.UserSkill != null)
                {
                    skills.Add(new ConfigUserSkill
                    {
                        SkillId = u.UserSkill.SkillId ?? "NA",
                        SkillStatus = u.UserSkill.UserSkillStatus,
                        InvocationName = u.UserSkill.InvocationName,
                        Username = Plugin.Instance!.UserManager.GetUserById(u.Id).Username,
                        UserId = u.Id.ToString()
                    });
                }
            }

            return skills;
        }
    }
}