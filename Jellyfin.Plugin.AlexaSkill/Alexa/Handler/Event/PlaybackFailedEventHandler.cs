using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Jellyfin.Plugin.AlexaSkill.Data;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Handler;

/// <summary>
/// Handler for PlaybackFinished events.
/// </summary>
#pragma warning disable CA1711
public class PlaybackFailedEventHandler : BaseHandler
#pragma warning restore CA1711
{
    private ILibraryManager _libraryManager;
    private IUserManager _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackFailedEventHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
    /// <param name="dbRepo">Instance of the <see cref="DbRepo"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
    public PlaybackFailedEventHandler(
        ISessionManager sessionManager,
        DbRepo dbRepo,
        ILibraryManager libraryManager,
        IUserManager userManager,
        ILoggerFactory loggerFactory) : base(sessionManager, dbRepo, loggerFactory)
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
        return ResponseBuilder.Tell("Something went wrong while playing your media. Please try again.");
    }
}