using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Alexa.NET.Management.AccountLinking;
using Alexa.NET.Management.Api;
using Alexa.NET.Management.Skills;
using Jellyfin.Plugin.AlexaSkill.Alexa;
using Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel;
using Jellyfin.Plugin.AlexaSkill.Alexa.Manifest;
using Jellyfin.Plugin.AlexaSkill.Api;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill;

/// <summary>
/// Update the skill in the Alexa cloud if it is outdated.
/// </summary>
public class SkillUpdate : IServerEntryPoint
{
    private readonly ILogger<SkillUpdate> _logger;
    private readonly ISessionManager _sessionManager;

    // Pointer to an external unmanaged resource.
    private IntPtr handle;
    // Other managed resource this class uses.
    private Component component = new Component();
    // Track whether Dispose has been called.
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillUpdate"/> class.
    /// </summary>
    /// <param name="sessionManager">Session manager.</param>
    /// <param name="loggerFactory">Logger.</param>
    public SkillUpdate(ISessionManager sessionManager, ILoggerFactory loggerFactory)
    {
        _sessionManager = sessionManager;
        _logger = loggerFactory.CreateLogger<SkillUpdate>();
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

        // check if all credentials and tokens are available
        if (!string.IsNullOrEmpty(configuration.SmapiClientId)
                && !string.IsNullOrEmpty(configuration.SmapiClientSecret)
                && !string.IsNullOrEmpty(configuration.SmapiRefreshToken)
                && !string.IsNullOrEmpty(configuration.VendorId)
                && !string.IsNullOrEmpty(configuration.ServerAddress))
        {
            string skillId = configuration.SkillId;

            // check if we already have a skill in the Alexa cloud
            if (string.IsNullOrEmpty(skillId))
            {
                _logger.LogInformation("New skill will be created");

                // otherwise create a new skill in the Alexa cloud
                try
                {
                    await AlexaUtil.CreateSkill().ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is UnauthorizedAccessException || ex is JsonException)
                {
                    _logger.LogError("Failed to create the skill {0} {1}", ex.Message, ex.StackTrace);
                }
                catch (Exception ex) when (ex is Refit.ApiException)
                {
                    _logger.LogCritical("SMAPI exception: {0}", ((Refit.ApiException)ex).Content);
                }
            }
            else
            {
                _logger.LogInformation("Existing skill found with id {0}", skillId);

                ManifestSkill skill;
                SkillStatus status;
                try
                {
                    // try to get the latest skill model status from the Alexa cloud
                    skill = new ManifestSkill(manifest: (await Plugin.Instance.SmapiManagement.Skills.Get(skillId, SkillStage.Development).ConfigureAwait(false)).Manifest);

                    _logger.LogInformation("Skill version (cloud): {0}", skill.GetVersionTag());

                    // check if the model status is failed and try to rebuild
                    status = await Plugin.Instance.SmapiManagement.Skills.Status(skillId).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is UnauthorizedAccessException || ex is JsonException)
                {
                    _logger.LogError("Failed to get existing skilll {0} {1}", ex.Message, ex.StackTrace);
                    return;
                }
                catch (Exception ex) when (ex is Refit.ApiException)
                {
                    _logger.LogCritical("SMAPI exception: {0}", ((Refit.ApiException)ex).Content);
                    return;
                }

                AccountLinkData accountLinkingData = await Plugin.Instance.SmapiManagement.AccountLinking.Get(skillId, SkillStage.Development).ConfigureAwait(false);

                Uri endpointUri = new Uri(new Uri(Plugin.Instance.Configuration.ServerAddress), RequestController.ApiBaseUri);
                string endpointUriString = new Uri(endpointUri, "account-linking").ToString();

                // check if the version in the cloud is outdated
                if (!string.Equals(Plugin.Instance.Skill.GetVersionTag(), skill.GetVersionTag(), StringComparison.OrdinalIgnoreCase)
                        || status.Manifest.LastModified.Status == SkillStatusState.FAILED
                        || !accountLinkingData.AuthorizationUrl.Equals(endpointUriString, StringComparison.Ordinal)
                        || !Plugin.Instance.Configuration.AccountLinkingClientId.Equals(accountLinkingData.ClientId, StringComparison.Ordinal))
                {
                    _logger.LogInformation("Skill version (cloud) outdated or last build status failed, rebuilding...");

                    try
                    {
                        // update the skill in the cloud
                        await Plugin.Instance.Skill.Update(Plugin.Instance.Configuration.SkillId).ConfigureAwait(false);

                        // wait until the skill is build
                        while (true)
                        {
                            status = await Plugin.Instance.SmapiManagement.Skills.Status(skillId).ConfigureAwait(false);
                            if (status.Manifest.LastModified.Status != SkillStatusState.IN_PROGRESS)
                            {
                                break;
                            }

                            Thread.Sleep(1000);
                        }

                        // update the skill interaction models in the Alexa cloud if there is an active skill
                        if (!string.IsNullOrEmpty(skillId))
                        {
                            foreach (SkillInteractionModel skillInteractionModel in Plugin.Instance.SkillInteractionModels)
                            {
                                await skillInteractionModel.Update(skillId).ConfigureAwait(false);
                            }
                        }

                        // update the account linking
                        AccountLinkData accountLinkData = new AccountLinkData()
                        {
                            Type = AccountLinkType.IMPLICIT,
                            AuthorizationUrl = endpointUriString,
                            ClientId = Plugin.Instance.Configuration.AccountLinkingClientId,
                        };

                        await Plugin.Instance.SmapiManagement.AccountLinking.Update(skillId, accountLinkData).ConfigureAwait(false);
                    }
                    catch (Exception ex) when (ex is HttpRequestException || ex is UnauthorizedAccessException || ex is JsonException)
                    {
                        _logger.LogError("Failed to update existing skilll {0} {1}", ex.Message, ex.StackTrace);
                        return;
                    }
                    catch (Exception ex) when (ex is Refit.ApiException)
                    {
                        _logger.LogCritical("SMAPI exception: {0}", ((Refit.ApiException)ex).Content);
                        return;
                    }
                }
            }
        }
    }
}