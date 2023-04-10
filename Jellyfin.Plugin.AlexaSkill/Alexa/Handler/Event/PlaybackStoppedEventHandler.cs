using System;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Handler;

/// <summary>
/// Handler for PlaybackStopped events.
/// </summary>
#pragma warning disable CA1711
public class PlaybackStoppedEventHandler : BaseHandler
#pragma warning restore CA1711
{
    private ILibraryManager _libraryManager;
    private IUserManager _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackStoppedEventHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
    public PlaybackStoppedEventHandler(
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
        return audioPlayerRequest != null && audioPlayerRequest.AudioRequestType == AudioRequestType.PlaybackStopped;
    }

    /// <inheritdoc/>
    public override SkillResponse Handle(Request request, Context context, Entities.User user, SessionInfo session)
    {
        AudioPlayerRequest req = (AudioPlayerRequest)request;

        PlaybackProgressInfo playbackProgressInfo = new PlaybackProgressInfo
        {
            SessionId = session.Id,
            ItemId = new Guid(req.Token),
            IsPaused = true,
            PositionTicks = req.OffsetInMilliseconds * 10000
        };
        SessionManager.OnPlaybackProgress(playbackProgressInfo).ConfigureAwait(false);

        return ResponseBuilder.Empty();
    }
}