using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Jellyfin.Plugin.AlexaSkill.Data;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Handler;

/// <summary>
/// Handler for UnmarkFavoriteIntent intents.
/// </summary>
public class UnmarkFavoriteIntentHandler : BaseHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnmarkFavoriteIntentHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">Session manager instance.</param>
    /// <param name="dbRepo">The database repository instance.</param>
    /// <param name="loggerFactory">Logger factory instance.</param>
    public UnmarkFavoriteIntentHandler(ISessionManager sessionManager, DbRepo dbRepo, ILoggerFactory loggerFactory) : base(sessionManager, dbRepo, loggerFactory)
    {
    }

    /// <inheritdoc/>
    public override bool CanHandle(Request request)
    {
        IntentRequest? intentRequest = request as IntentRequest;
        return intentRequest != null && string.Equals(intentRequest.Intent.Name, "UnmarkFavoriteIntent", System.StringComparison.Ordinal);
    }

    /// <summary>
    /// Remove the currently playing media from the favorite list.
    /// </summary>
    /// <param name="request">The skill request which should be handled.</param>
    /// <param name="context">The context of the skill intent request.</param>
    /// <param name="user">The user instance.</param>
    /// <param name="session">The session instance.</param>
    /// <returns>Confirmation statement that the media was removed from the favorites list or error message when the media can not be found.</returns>
    public override SkillResponse Handle(Request request, Context context, Entities.User user, SessionInfo session)
    {
        if (session.NowPlayingItem == null)
        {
            return ResponseBuilder.Tell("Sorry I could not find the media.");
        }

        // TODO: remove media to favorites list

        return ResponseBuilder.Tell("Media removed from the favorites list.");
    }
}