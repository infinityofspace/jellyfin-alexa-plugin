using System;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Jellyfin.Plugin.AlexaSkill.Data;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Handler;

/// <summary>
/// Handler for AMAZON.PreviousIntent intents.
/// </summary>
public class PreviousIntentHandler : BaseHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PreviousIntentHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">Session manager instance.</param>
    /// <param name="dbRepo">The database repository instance.</param>
    /// <param name="loggerFactory">Logger factory instance.</param>
    public PreviousIntentHandler(ISessionManager sessionManager, DbRepo dbRepo, ILoggerFactory loggerFactory) : base(sessionManager, dbRepo, loggerFactory)
    {
    }

    /// <inheritdoc/>
    public override bool CanHandle(Request request)
    {
        IntentRequest? intentRequest = request as IntentRequest;
        return intentRequest != null && string.Equals(intentRequest.Intent.Name, "AMAZON.PreviousIntent", System.StringComparison.Ordinal);
    }

    /// <summary>
    /// Play the previous item in the queue.
    /// </summary>
    /// <param name="request">The skill request which should be handled.</param>
    /// <param name="context">The context of the skill intent request.</param>
    /// <param name="user">The user instance.</param>
    /// <param name="session">The session instance.</param>
    /// <returns>A play directive of the previous item in the queue or empty response if the queue is empty.</returns>
    public override SkillResponse Handle(Request request, Context context, Entities.User user, SessionInfo session)
    {
        // check if we have any media in the queue and the is currently something playing
        if (session.NowPlayingQueue.Count == 0 || session.NowPlayingItem == null)
        {
            return ResponseBuilder.Empty();
        }

        // get the previous item in the queue
        for (int i = 1; i < session.NowPlayingQueue.Count; i++)
        {
            if (session.NowPlayingQueue[i].Id == session.NowPlayingItem.Id)
            {
                string item_id = session.NowPlayingQueue[i - 1].Id.ToString();
                string audioUrl = new Uri(new Uri(Plugin.Instance!.Configuration.ServerAddress), "/Audio/" + item_id + "/universal").ToString();

                return ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, audioUrl, item_id);
            }
        }

        return ResponseBuilder.Empty();
    }
}