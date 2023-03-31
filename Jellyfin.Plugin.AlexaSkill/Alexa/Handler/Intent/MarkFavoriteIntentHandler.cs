using System.Threading;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Handler;

/// <summary>
/// Handler for MarkFavoriteIntent intents.
/// </summary>
public class MarkFavoriteIntentHandler : BaseHandler
{
    private IUserDataRepository _userDataRepository;
    private IUserManager _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkFavoriteIntentHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <param name="userDataRepository">Instance of the <see cref="IUserDataRepository"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
    public MarkFavoriteIntentHandler(
        ISessionManager sessionManager,
        PluginConfiguration config,
        IUserDataRepository userDataRepository,
        IUserManager userManager,
        ILoggerFactory loggerFactory) : base(sessionManager, config, loggerFactory)
    {
        _userDataRepository = userDataRepository;
        _userManager = userManager;
    }

    /// <inheritdoc/>
    public override bool CanHandle(Request request)
    {
        IntentRequest? intentRequest = request as IntentRequest;
        return intentRequest != null && string.Equals(intentRequest.Intent.Name, "MarkFavoriteIntent", System.StringComparison.Ordinal);
    }

    /// <summary>
    /// Add the currently playing media to the favorite list.
    /// </summary>
    /// <param name="request">The skill request which should be handled.</param>
    /// <param name="context">The context of the skill intent request.</param>
    /// <param name="user">The user instance.</param>
    /// <param name="session">The session instance.</param>
    /// <returns>Confirmation statement that the media was added to the favorites list or error message when the media can not be found.</returns>
    public override SkillResponse Handle(Request request, Context context, Entities.User user, SessionInfo session)
    {
        BaseItemDto item = session.NowPlayingItem;
        if (item == null)
        {
            return ResponseBuilder.Tell("Sorry I could not find the media.");
        }

        var jellyfinUser = _userManager.GetUserById(user.Id);

        var data = _userDataRepository.GetUserData(jellyfinUser.InternalId, item.Id.ToString());

        data.IsFavorite = true;

        _userDataRepository.SaveUserData(jellyfinUser.InternalId, item.Id.ToString(), data, CancellationToken.None);

        return ResponseBuilder.Tell("Media added to favorites list.");
    }
}