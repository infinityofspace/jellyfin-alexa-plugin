using System;
using System.Threading;
using System.Threading.Tasks;
using Alexa.NET.Management.AccountLinking;
using Alexa.NET.Management.Skills;
using Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel;
using Jellyfin.Plugin.AlexaSkill.Api;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa;

/// <summary>
/// Util methods.
/// </summary>
public static class AlexaUtil
{
    /// <summary>
    /// Create a new skill and update the interaction models and add the skill id to the config.
    /// </summary>
    /// <returns>Task.</returns>
    public static async Task CreateSkill()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = loggerFactory.CreateLogger<Plugin>();

        logger.LogDebug("Creating a new skill...");

        PluginConfiguration configuration = Plugin.Instance!.Configuration;

        // create the skill in the Alxa cloud
        SkillId skillIdObj = await Plugin.Instance.Skill.CreateSkill(configuration.VendorId).ConfigureAwait(false);

        configuration.SkillId = skillIdObj.Id;
        string skillId = skillIdObj.Id;
        logger.LogDebug("New skill created with ID: {0}", skillId);

        // wait until the skill is build
        SkillStatus status;
        while (true)
        {
            status = await Plugin.Instance.SmapiManagement.Skills.Status(skillId).ConfigureAwait(false);
            if (status.Manifest.LastModified.Status != SkillStatusState.IN_PROGRESS)
            {
                break;
            }

            Thread.Sleep(1000);
        }

        Plugin.Instance.SaveConfiguration();

        // now update the interaction models
        foreach (SkillInteractionModel skillInteractionModel in Plugin.Instance.SkillInteractionModels)
        {
            await skillInteractionModel.Update(skillId).ConfigureAwait(false);
        }

        // update the account linking
        Uri endpointUri = new Uri(new Uri(Plugin.Instance.Configuration.ServerAddress), RequestController.ApiBaseUri);
        string endpointUriString = new Uri(endpointUri, "account-linking").ToString();

        AccountLinkData accountLinkData = new AccountLinkData()
        {
            Type = AccountLinkType.IMPLICIT,
            AuthorizationUrl = endpointUriString,
            ClientId = Plugin.Instance.Configuration.AccountLinkingClientId,
        };

        await Plugin.Instance.SmapiManagement.AccountLinking.Update(skillId, accountLinkData).ConfigureAwait(false);
    }
}