using System.Threading.Tasks;
using Alexa.NET.Management.Api;
using Alexa.NET.Management.Skills;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel;

/// <summary>
/// Represents the interaction model of the skill.
/// </summary>
public class SkillInteractionModel : SkillInteractionContainer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SkillInteractionModel"/> class.
    /// </summary>
    /// <param name="ressourcePath">Path to the manifest ressource.</param>
    /// <param name="locale">Locale of this interaction model.</param>
    public SkillInteractionModel(string ressourcePath, string locale)
    {
        InteractionModel = Util.DeserializeFromFile<SkillInteraction>(ressourcePath);
        Locale = locale;
        AddCustomizations();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillInteractionModel"/> class.
    /// </summary>
    /// <param name="skillInteraction">Skill interaction model of the skill.</param>
    /// <param name="locale">Locale of this interaction model.</param>
    public SkillInteractionModel(SkillInteraction skillInteraction, string locale)
    {
        InteractionModel = skillInteraction;
        Locale = locale;
        AddCustomizations();
    }

    /// <summary>
    /// Gets or sets the locale of this interaction model.
    /// </summary>
    [JsonIgnore]
    public string Locale { get; set; }

    /// <summary>
    /// Update the interaction model in the Alexa cloud.
    /// </summary>
    /// <param name="skillId">ID of the skill.</param>
    /// <param name="stage">Stage of the skill.</param>
    /// <returns>Boolean indicating if the updating was successull.</returns>
    public Task Update(string skillId, SkillStage stage = SkillStage.Development)
    {
        return Plugin.Instance!.SmapiManagement.InteractionModel.Update(skillId, stage, Locale, this);
    }

    /// <summary>
    /// Add any custom values to the interaction model.
    /// </summary>
    private void AddCustomizations()
    {
        if (Plugin.Instance != null)
        {
            PluginConfiguration configuration = Plugin.Instance.Configuration;

            InteractionModel.Language.InvocationName = configuration.InvocationName;
        }
    }
}