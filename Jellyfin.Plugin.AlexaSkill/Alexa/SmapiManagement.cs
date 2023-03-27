using System.Collections.ObjectModel;
using System.Threading;
using Alexa.NET.Management;
using Alexa.NET.Management.AccountLinking;
using Alexa.NET.Management.Api;
using Alexa.NET.Management.Skills;
using Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel;
using Jellyfin.Plugin.AlexaSkill.Alexa.Manifest;
using Jellyfin.Plugin.AlexaSkill.Lwa;

namespace Jellyfin.Plugin.AlexaSkill.Alexa;

/// <summary>
/// Util methods.
/// </summary>
public class SmapiManagement : ManagementApi
{
    private DeviceToken deviceToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmapiManagement"/> class.
    /// </summary>
    /// <param name="smapiDeviceToken">The smapi device token.</param>
    public SmapiManagement(DeviceToken smapiDeviceToken) : base(smapiDeviceToken.AccessToken)
    {
        deviceToken = smapiDeviceToken;
    }

    /// <summary>
    /// Creates a new skill.
    /// </summary>
    /// <param name="manifestSkill">The manifest skill.</param>
    /// <param name="interactionModels">The interaction models.</param>
    /// <param name="endpointUri">The alexa api endpoint.</param>
    /// <param name="clientId">The client api which will be used in alexa requests to the api endpoint.</param>
    /// <returns>The id of the created skill.</returns>
    public string CreateSkill(ManifestSkill manifestSkill, Collection<SkillInteractionModel> interactionModels, string endpointUri, string clientId)
    {
        VendorResponse vendor = this.Vendors.Get().Result;

        string vendorId = vendor.Vendors[0].Id;

        SkillId skillId = this.Skills.Create(vendorId, manifestSkill).Result;

        // wait until the skill is created
        while (this.GetSkillStatus(skillId.Id).Manifest.LastModified.Status == SkillStatusState.IN_PROGRESS)
        {
            Thread.Sleep(1000);
        }

        this.UpdateAccountLinkData(skillId.Id, endpointUri, clientId);

        foreach (var interactionModel in interactionModels)
        {
            this.InteractionModel.Update(skillId.Id, SkillStage.Development, interactionModel.Locale, interactionModel);
        }

        return skillId.Id;
    }

    /// <summary>
    /// Updates a skill.
    /// </summary>
    /// <param name="skillId">The id of the skill to update.</param>
    /// <param name="manifestSkill">The new manifest skill.</param>
    /// <param name="interactionModels">The new interaction models.</param>
    public void UpdateSkill(string skillId, ManifestSkill manifestSkill, Collection<SkillInteractionModel> interactionModels)
    {
        _ = this.Skills.Update(skillId, SkillStage.Development, manifestSkill);

        // wait until the skill is created
        while (this.GetSkillStatus(skillId).Manifest.LastModified.Status == SkillStatusState.IN_PROGRESS)
        {
            Thread.Sleep(1000);
        }

        foreach (var interactionModel in interactionModels)
        {
            this.InteractionModel.Update(skillId, SkillStage.Development, interactionModel.Locale, interactionModel);
        }
    }

    /// <summary>
    /// Gets the skill.
    /// </summary>
    /// <param name="skillId">The id of the skill to get.</param>
    /// <returns>The skill.</returns>
    public ManifestSkill GetSkill(string skillId)
    {
        return new ManifestSkill(this.Skills.Get(skillId, SkillStage.Development).Result.Manifest);
    }

    /// <summary>
    /// Deletes a skill.
    /// </summary>
    /// <param name="skillId">The id of the skill to delete.</param>
    public void DeleteSkill(string skillId)
    {
        this.Skills.Delete(skillId).Wait();
    }

    /// <summary>
    /// Gets the AccountLink data.
    /// </summary>
    /// <param name="skillId">The id of the skill to get the AccountLinking data from.</param>
    /// <returns>The AccountLinking data.</returns>
    public AccountLinkData GetAccountLinkData(string skillId)
    {
        return this.AccountLinking.Get(skillId, SkillStage.Development).Result;
    }

    /// <summary>
    /// Updates the AccountLink data.
    /// </summary>
    /// <param name="skillId">The id of the skill to update the AccountLinking data from.</param>
    /// <param name="endpointUri">The endpoint uri.</param>
    /// <param name="clientId">The client id.</param>
    public void UpdateAccountLinkData(string skillId, string endpointUri, string clientId)
    {
        AccountLinkData accountLinkData = new AccountLinkData()
        {
            Type = AccountLinkType.IMPLICIT,
            AuthorizationUrl = endpointUri,
            ClientId = clientId,
        };
        this.AccountLinking.Update(skillId, accountLinkData);
    }

    /// <summary>
    /// Gets skill status.
    /// </summary>
    /// <param name="skillId">The id of the skill.</param>
    /// <returns>The skill status.</returns>
    public SkillStatus GetSkillStatus(string skillId)
    {
        return this.Skills.Status(skillId).Result;
    }
}