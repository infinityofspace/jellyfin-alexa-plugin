using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Alexa.NET.Management.AccountLinking;
using Alexa.NET.Management.Skills;
using Jellyfin.Plugin.AlexaSkill.Alexa;
using Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel;
using Jellyfin.Plugin.AlexaSkill.Alexa.Manifest;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using Jellyfin.Plugin.AlexaSkill.Controller;
using Jellyfin.Plugin.AlexaSkill.Entities;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.EntryPoints;

/// <summary>
/// Setup the skill and update or create the skill in the Alexa cloud if it is outdated.
/// </summary>
public class SkillStartup : IServerEntryPoint
{
    private readonly ILogger<SkillStartup> _logger;
    private readonly ISessionManager _sessionManager;

    // Pointer to an external unmanaged resource.
    private IntPtr handle;
    // Other managed resource this class uses.
    private Component component = new Component();
    // Track whether Dispose has been called.
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillStartup"/> class.
    /// </summary>
    /// <param name="sessionManager">Session manager.</param>
    /// <param name="loggerFactory">Logger.</param>
    public SkillStartup(ISessionManager sessionManager, ILoggerFactory loggerFactory)
    {
        _sessionManager = sessionManager;
        _logger = loggerFactory.CreateLogger<SkillStartup>();
    }

        /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose.
    /// </summary>
    /// <param name="disposing">dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!this.disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                component.Dispose();
            }

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // If disposing is false,
            // only the following code is executed.
            // CloseHandle(handle);
            handle = IntPtr.Zero;

            // Note disposing has been done.
            disposed = true;
        }
    }

    /// <inheritdoc />
    public async Task RunAsync()
    {
        _logger.LogInformation("Skill version (local): v{0}", Util.GetVersion());

        PluginConfiguration configuration = Plugin.Instance!.Configuration;

        if (string.IsNullOrEmpty(configuration.ServerAddress))
        {
            _logger.LogWarning("No server address set up. No new skill can be created or old skills can be updated. Also, old skill might not work if the last active server address is no longer valid. Please configure the server address in the plugin settings.");
            return;
        }

        ManifestSkill manifestSkill = new ManifestSkill("Jellyfin.Plugin.AlexaSkill.Alexa.Manifest.manifest.json", configuration.ServerAddress, configuration.SslCertType);
        Plugin.Instance.ManifestSkill = manifestSkill;

        Uri endpointUri = new Uri(new Uri(configuration.ServerAddress), AlexaSkillController.ApiBaseUri);
        string endpointUriString = new Uri(endpointUri, "account-linking").ToString();

        await Task.Run(() =>
        {
            foreach (User user in configuration.Users)
            {
                if (user.UserSkill != null)
                {
                    Collection<SkillInteractionModel> skillInteractionModels = new Collection<SkillInteractionModel>();
                    foreach (Tuple<string, string> model in Plugin.Instance.InteractionModels)
                    {
                        skillInteractionModels.Add(new SkillInteractionModel(model.Item1, model.Item2, user.UserSkill.InvocationName));
                    }

                    // check if the skill is created in the cloud
                    if (user.UserSkill.SkillId != null && user.SmapiManagement != null)
                    {
                        ManifestSkill cloudManifestSkill = AlexaUtil.Call(user, () => user.SmapiManagement.GetSkill(user.UserSkill.SkillId));
                        _logger.LogInformation("Skill version (cloud) for user with id {0}: {1}", user.Id, cloudManifestSkill.GetVersionTag());

                        AccountLinkData accountLinkingData = AlexaUtil.Call(user, () => user.SmapiManagement.GetAccountLinkData(user.UserSkill.SkillId));

                        SkillStatus status = AlexaUtil.Call(user, () => user.SmapiManagement.GetSkillStatus(user.UserSkill.SkillId));

                        // check if the skill is diverged from the local model
                        if (cloudManifestSkill.GetVersionTag() != Util.GetVersion()
                            || status.Manifest.LastModified.Status == SkillStatusState.FAILED)
                        {
                            // update the skill in the cloud
                            _logger.LogInformation("Skill for user with id {0} is outdated. Updating...", user.Id);
                            AlexaUtil.Call<object?>(user, () =>
                            {
                                user.SmapiManagement.UpdateSkill(user.UserSkill.SkillId, manifestSkill, skillInteractionModels);
                                return null;
                            });
                        }

                        if (!accountLinkingData.AuthorizationUrl.Equals(endpointUriString, StringComparison.Ordinal)
                            || !Plugin.Instance.Configuration.AccountLinkingClientId.Equals(accountLinkingData.ClientId, StringComparison.Ordinal))
                        {
                            _logger.LogInformation("Account linking data for user with id {0} is outdated. Updating...", user.Id);
                            AlexaUtil.Call<object?>(user, () =>
                            {
                                user.SmapiManagement.UpdateAccountLinkData(
                                    user.UserSkill.SkillId,
                                    configuration.ServerAddress,
                                    configuration.AccountLinkingClientId);
                                return null;
                            });
                        }
                    }
                    else if (user.SmapiManagement != null)
                    {
                        user.UserSkill.UserSkillStatus = UserSkillStatus.SkillCreating;

                        // create the skill in the cloud
                        _logger.LogInformation("Skill for user with id {0} is not created in the cloud. Creating...", user.Id);
                        string skillId = AlexaUtil.Call(user, () => user.SmapiManagement.CreateSkill(
                            manifestSkill,
                            skillInteractionModels,
                            configuration.ServerAddress,
                            configuration.AccountLinkingClientId));

                        user.UserSkill.SkillId = skillId;
                        user.UserSkill.UserSkillStatus = UserSkillStatus.AccountLinkPending;
                        Plugin.Instance.SaveConfiguration();
                    }
                }
            }
        }).ConfigureAwait(false);
    }
}
