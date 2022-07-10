using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using Alexa.NET.Management;
using Jellyfin.Plugin.AlexaSkill.Alexa;
using Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel;
using Jellyfin.Plugin.AlexaSkill.Alexa.Manifest;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using Jellyfin.Plugin.AlexaSkill.Data;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill;

/// <summary>
/// The main plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private SmapiAccessToken smapiAccessToken = new SmapiAccessToken();

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILoggerFactory loggerFactory)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;

        DbRepo = new DbRepo($"{applicationPaths.DataPath}/{Config.DbFilePath}", loggerFactory);

        SmapiManagement = new ManagementApi(smapiAccessToken.GetAccessToken);

        ILogger<Plugin> logger = loggerFactory.CreateLogger<Plugin>();
    }

    /// <inheritdoc />
    public override string Name => "AlexaSkill";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("c5df7de0-8777-4b3c-a70d-5c3dae359c9e");

    /// <summary>
    /// Gets the database for persistent skill data.
    /// </summary>
    public DbRepo DbRepo { get; private set; }

    /// <summary>
    /// Gets the http client.
    /// </summary>
    public static HttpClient HttpClient { get; } = new HttpClient();

    /// <summary>
    /// Gets the current smapi managment instance.
    /// </summary>
    public ManagementApi SmapiManagement { get; private set; }

    /// <summary>
    /// Gets the skill manifest.
    /// </summary>
    public ManifestSkill Skill { get; } = new ManifestSkill("Jellyfin.Plugin.AlexaSkill.Alexa.Manifest.manifest.json");

    /// <summary>
    /// Gets the Interaction models for each supported locale.
    /// </summary>
    public Collection<SkillInteractionModel> SkillInteractionModels { get; } = new Collection<SkillInteractionModel>()
    {
        new SkillInteractionModel("Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel.model_en_US.json", "en-US"),
        new SkillInteractionModel("Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel.model_de_DE.json", "de-DE")
    };

    /// <summary>
    /// Gets the dictionary of device ids to session tokens.
    /// </summary>
    public Dictionary<string, string> SessionTokens { get; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = this.Name,
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.config.html"
            }
        };
    }
}
