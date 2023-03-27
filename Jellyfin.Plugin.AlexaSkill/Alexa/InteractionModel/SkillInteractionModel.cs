using Alexa.NET.Management.Skills;
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
    /// <param name="invocationName">Invocation name of this interaction model.</param>
    public SkillInteractionModel(string locale, string ressourcePath, string invocationName)
    {
        InteractionModel = Util.DeserializeFromFile<SkillInteraction>(ressourcePath);
        Locale = locale;
        InvocationName = invocationName;
    }

    /// <summary>
    /// Gets or sets the locale of this interaction model.
    /// </summary>
    [JsonIgnore]
    public string Locale { get; set; }

    /// <summary>
    /// Gets or sets the invocation name of this interaction model.
    /// </summary>
    [JsonIgnore]
    public string InvocationName
    {
        get
        {
            return InteractionModel.Language.InvocationName;
        }

        set
        {
            InteractionModel.Language.InvocationName = value;
        }
    }
}