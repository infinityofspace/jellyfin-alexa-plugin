using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Jellyfin.Plugin.AlexaSkill.Data;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Handler;

/// <summary>
/// Handler for AMAZON.NextIntent intents.
/// </summary>
public class NextIntentHandler : BaseHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NextIntentHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">Session manager instance.</param>
    /// <param name="dbRepo">The database repository instance.</param>
    /// <param name="loggerFactory">Logger factory instance.</param>
    public NextIntentHandler(ISessionManager sessionManager, DbRepo dbRepo, ILoggerFactory loggerFactory) : base(sessionManager, dbRepo, loggerFactory)
    {
    }

    /// <inheritdoc/>
    public override bool CanHandle(Request request)
    {
        IntentRequest? intentRequest = request as IntentRequest;
        return intentRequest != null && string.Equals(intentRequest.Intent.Name, "AMAZON.NextIntent", System.StringComparison.Ordinal);
    }

    /// <summary>
    /// Resume any currently playing media or ask the user to say some media name to play.
    /// </summary>
    /// <param name="request">The skill request which should be handled.</param>
    /// <param name="context">The context of the skill intent request.</param>
    /// <param name="user">The user instance.</param>
    /// <param name="session">The session instance.</param>
    /// <returns>A play directive or a question what should be played.</returns>
    public override SkillResponse Handle(Request request, Context context, Entities.User user, SessionInfo session)
    {
        return ResponseBuilder.Ask("Welcome to Jellyfin Skill, what can I play?", new Reprompt("Please tell me, what should I play?"));
    }
}