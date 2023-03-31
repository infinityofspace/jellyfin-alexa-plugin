using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using Jellyfin.Plugin.AlexaSkill.Alexa.Manifest;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using Jellyfin.Plugin.AlexaSkill.Controller.Handler;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill;

/// <summary>
/// The main plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        ILoggerFactory loggerFactory,
        IUserManager userManager) : base(applicationPaths, xmlSerializer)
    {
        Instance = this;

        UserManager = userManager;

        ILogger<Plugin> logger = loggerFactory.CreateLogger<Plugin>();
    }

    /// <inheritdoc />
    public override string Name => "AlexaSkill";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("c5df7de0-8777-4b3c-a70d-5c3dae359c9e");

    /// <summary>
    /// Gets the http client.
    /// </summary>
    public static HttpClient HttpClient { get; } = new HttpClient();

    /// <summary>
    /// Gets or sets the skill manifest.
    /// </summary>
    public ManifestSkill? ManifestSkill { get; set; }

    /// <summary>
    /// Gets the dictionary of device ids to session tokens.
    /// </summary>
    public IUserManager UserManager { get; private set; }

    /// <summary>
    /// Gets the dictionary of device ids to session tokens.
    /// </summary>
    public Dictionary<string, string> SessionTokens { get; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the LWA authorization request handler.
    /// </summary>
    public LwaAuthorizationRequestHandler LwaAuthorizationRequestHandler { get; } =
        new LwaAuthorizationRequestHandler();

    /// <summary>
    /// Gets the Interaction models for each supported locale.
    /// </summary>
    public Collection<Tuple<string, string>> InteractionModels { get; } = Util.GetLocalInteractionModels();

    /// <summary>
    /// Gets the CSRF token handler.
    /// </summary>
    public CsrfTokenHandler CsrfTokenHandler { get; } = new CsrfTokenHandler();

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
