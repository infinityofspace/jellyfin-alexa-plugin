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
/// Handler for LaunchRequest intents.
/// </summary>
public class LaunchRequestHandler : BaseHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LaunchRequestHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">Session manager instance.</param>
    /// <param name="dbRepo">The database repository instance.</param>
    /// <param name="loggerFactory">Logger factory instance.</param>
    public LaunchRequestHandler(ISessionManager sessionManager, DbRepo dbRepo, ILoggerFactory loggerFactory) : base(sessionManager, dbRepo, loggerFactory)
    {
    }

    /// <inheritdoc/>
    public override bool CanHandle(Request request)
    {
        return request is LaunchRequest;
    }

    /// <summary>
    /// Resume any currently playing media or ask the user to say some media name to play.
    /// </summary>
    /// <param name="request">The skill intent request which should be handled.</param>
    /// <param name="context">The context of the skill intent request.</param>
    /// <param name="user">The user instance.</param>
    /// <param name="session">The session instance.</param>
    /// <returns>A play directive or a question what should be played.</returns>
    public override SkillResponse Handle(Request request, Context context, Entities.User user, SessionInfo session)
    {
        LaunchRequest launchRequest = (LaunchRequest)request;

        // check if we have any media in the queue
        if (session.NowPlayingQueue.Count == 0)
        {
            return ResponseBuilder.Ask("Welcome to Jellyfin Skill, what can I play?", new Reprompt("Please tell me, what should I play?"));
        }

        // check if something is currently playing which we can resume
        if (session.NowPlayingItem != null)
        {
            string item_id = session.NowPlayingItem.Id.ToString();
            string audioUrl = new Uri(new Uri(Plugin.Instance!.Configuration.ServerAddress), "/Audio/" + item_id + "/universal").ToString();

            return ResponseBuilder.AudioPlayerPlay(PlayBehavior.ReplaceAll, audioUrl, item_id);
        }
        else
        {
            // resume the first item in the queue
            string item_id = session.NowPlayingQueue[0].Id.ToString();
            string audioUrl = new Uri(new Uri(Plugin.Instance!.Configuration.ServerAddress), "/Audio/" + item_id + "/universal").ToString();

            return ResponseBuilder.AudioPlayerPlay(PlayBehavior.ReplaceAll, audioUrl, item_id);
        }
    }
}