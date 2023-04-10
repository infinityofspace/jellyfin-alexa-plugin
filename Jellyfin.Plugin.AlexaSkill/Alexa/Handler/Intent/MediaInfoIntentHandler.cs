using System;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Handler;

/// <summary>
/// Handler for MediaInfo intents.
/// </summary>
public class MediaInfoIntentHandler : BaseHandler
{
    private ILibraryManager _libraryManager;
    private IUserManager _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInfoIntentHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
    public MediaInfoIntentHandler(
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
        IntentRequest? intentRequest = request as IntentRequest;
        return intentRequest != null && string.Equals(intentRequest.Intent.Name, "MediaInfoIntent", System.StringComparison.Ordinal);
    }

    /// <summary>
    /// Inform the user about the currently playing media.
    /// </summary>
    /// <param name="request">The skill request which should be handled.</param>
    /// <param name="context">The context of the skill intent request.</param>
    /// <param name="user">The user instance.</param>
    /// <param name="session">The session instance.</param>
    /// <returns>Info about the currently playing media.</returns>
    public override SkillResponse Handle(Request request, Context context, Entities.User user, SessionInfo session)
    {
        if (session.FullNowPlayingItem == null)
        {
            return ResponseBuilder.Tell("There is currently no media playing.");
        }

        string responseString;
        if (session.NowPlayingItem.Artists.Count == 0)
        {
            responseString = FormattableString.Invariant($"Currently playing {session.NowPlayingItem.Name}");
        }
        else
        {
            string artistsString = session.NowPlayingItem.Artists[0];
            for (int i = 1; i < session.NowPlayingItem.Artists.Count; i++)
            {
                string artist = session.NowPlayingItem.Artists[i];
                artistsString += FormattableString.Invariant($", {artist}");
            }

            responseString = FormattableString.Invariant($"Currently playing {session.NowPlayingItem.Name} from {artistsString}");
        }

        return ResponseBuilder.Tell(responseString);
    }
}