using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Jellyfin.Plugin.AlexaSkill.Data;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Handler;

/// <summary>
/// Handler for AMAZON.PauseIntent, AMAZON.StopIntent and AMAZON.CancelIntent intents.
/// </summary>
public class PauseIntentHandler : BaseHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PauseIntentHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">Session manager instance.</param>
    /// <param name="dbRepo">The database repository instance.</param>
    /// <param name="loggerFactory">Logger factory instance.</param>
    public PauseIntentHandler(ISessionManager sessionManager, DbRepo dbRepo, ILoggerFactory loggerFactory) : base(sessionManager, dbRepo, loggerFactory)
    {
    }

    /// <inheritdoc/>
    public override bool CanHandle(Request request)
    {
        IntentRequest? intentRequest = request as IntentRequest;

        return intentRequest != null && (string.Equals(intentRequest.Intent.Name, "AMAZON.PauseIntent", System.StringComparison.Ordinal) ||
            string.Equals(intentRequest.Intent.Name, "AMAZON.StopIntent", System.StringComparison.Ordinal) ||
            string.Equals(intentRequest.Intent.Name, "AMAZON.CancelIntent", System.StringComparison.Ordinal));
    }

    /// <summary>
    /// Pause any currently playing media.
    /// </summary>
    /// <param name="request">The skill request which should be handled.</param>
    /// <param name="context">The context of the skill intent request.</param>
    /// <param name="user">The user instance.</param>
    /// <param name="session">The session instance.</param>
    /// <returns>Emptry skill response.</returns>
    public override SkillResponse Handle(Request request, Context context, Entities.User user, SessionInfo session)
    {
        return ResponseBuilder.AudioPlayerStop();
    }
}