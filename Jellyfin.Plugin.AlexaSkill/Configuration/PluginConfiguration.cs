using System;
using Alexa.NET.Management;
using Alexa.NET.Management.Api;
using Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel;
using Jellyfin.Plugin.AlexaSkill.Alexa.Manifest;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AlexaSkill.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    private SslCertificateType sslCertType;
    private string serverAddress;
    private string invocationName;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        // set default options here
        sslCertType = SslCertificateType.Trusted;
        invocationName = "jellyfin player";
        SmapiClientId = string.Empty;
        SmapiClientSecret = string.Empty;
        SmapiRefreshToken = string.Empty;
        VendorId = string.Empty;
        SkillId = string.Empty;

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
            Plugin.Instance!.Skill.SetApiEndpoint(serverAddress, value);
            sslCertType = value;
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
            Plugin.Instance!.Skill.SetApiEndpoint(value, sslCertType);
            serverAddress = value;
        }
    }

    /// <summary>
    /// Gets or sets the custom invocation name of the skill.
    /// </summary>
    public string InvocationName
    {
        get => invocationName;
        set
        {
            foreach (SkillInteractionModel model in Plugin.Instance!.SkillInteractionModels)
            {
                model.InteractionModel.Language.InvocationName = value;
            }

            invocationName = value;
        }
    }

    /// <summary>
    /// Gets or sets the client id for SMAPI.
    /// </summary>
    public string SmapiClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret for SMAPI.
    /// </summary>
    public string SmapiClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the refresh token for SMAPI.
    /// </summary>
    public string SmapiRefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the vendor id of the user.
    /// </summary>
    public string VendorId { get; set; }

    /// <summary>
    /// Gets or sets the id of the Alexa skill.
    /// </summary>
    public string SkillId { get; set; }

    /// <summary>
    /// Gets the current version of the local skill.
    /// </summary>
    public string SkillVersion { get => "v" + Util.GetVersion(); }

    /// <summary>
    /// Gets the current version of the skill in the Alexa cloud.
    /// </summary>
    public string SkillVersionCloud
    {
        get
        {
            if (string.IsNullOrEmpty(SkillId))
            {
                return string.Empty;
            }

            return new ManifestSkill(Plugin.Instance!.SmapiManagement.Skills.Get(SkillId, SkillStage.Development).Result.Manifest).GetVersionTag();
        }
    }

    /// <summary>
    /// Gets or sets the account linking client id.
    /// </summary>
    public string AccountLinkingClientId { get; set; }
}
