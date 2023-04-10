using System;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Handler;

/// <summary>
/// Handler for PlaybackNearlyFinished events.
/// </summary>
#pragma warning disable CA1711
public class PlaybackNearlyFinishedEventHandler : BaseHandler
#pragma warning restore CA1711
{
    private ILibraryManager _libraryManager;
    private IUserManager _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackNearlyFinishedEventHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
    public PlaybackNearlyFinishedEventHandler(
        ISessionManager sessionManager,
        PluginConfiguration config,
        ILibraryManager libraryManager,
        IUserManager userManager,
        ILoggerFactory loggerFactory) : base(sessionManager, config, loggerFactory)
    {
        _libraryManager = libraryManager;
        _userManager = userManager;
    }

    /// <inheritdoc/>
    public override bool CanHandle(Request request)
    {
        AudioPlayerRequest? audioPlayerRequest = request as AudioPlayerRequest;
        return audioPlayerRequest != null && audioPlayerRequest.AudioRequestType == AudioRequestType.PlaybackNearlyFinished;
    }

    /// <summary>
    /// Respond with next item in the queue, otherwise sginal end of playback queue.
    /// </summary>
    /// <param name="request">The skill request which should be handled.</param>
    /// <param name="context">The context of the skill intent request.</param>
    /// <param name="user">The user instance.</param>
    /// <param name="session">The session instance.</param>
    /// <returns>Next item in the queue or end of playback queue.</returns>
    public override SkillResponse Handle(Request request, Context context, Entities.User user, SessionInfo session)
    {
        AudioPlayerRequest req = (AudioPlayerRequest)request;

        Guid? nextItemId = null;
        for (int i = 0; i < session.NowPlayingQueue.Count; i++)
        {
            if (session.NowPlayingQueue[i].Id == session.FullNowPlayingItem?.Id)
            {
                if (i + 1 < session.NowPlayingQueue.Count)
                {
                    nextItemId = session.NowPlayingQueue[i + 1].Id;
                }

                break;
            }
        }

        if (nextItemId == null)
        {
            return ResponseBuilder.AudioPlayerStop();
        }

        string itemId = nextItemId.ToString();
        return ResponseBuilder.AudioPlayerPlay(PlayBehavior.Enqueue, GetStreamUrl(itemId, user), itemId, req.Token, 0);
    }
}