using System;
using System.Collections.Generic;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Jellyfin.Plugin.AlexaSkill.Data;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Handler;

/// <summary>
/// Handler for PlayFavorites intents.
/// </summary>
public class PlayFavoritesIntentHandler : BaseHandler
{
    private ILibraryManager _libraryManager;
    private IUserManager _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayFavoritesIntentHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
    /// <param name="dbRepo">Instance of the <see cref="DbRepo"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
    public PlayFavoritesIntentHandler(
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
        IntentRequest? intentRequest = request as IntentRequest;
        return intentRequest != null && string.Equals(intentRequest.Intent.Name, "PlayFavoritesIntent", System.StringComparison.Ordinal);
    }

    /// <summary>
    /// Resume any currently playing media or ask the user to say some media name to play.
    /// </summary>
    /// <param name="request">The skill request which should be handled.</param>
    /// <param name="context">The context of the skill intent request.</param>
    /// <param name="user">The user instance.</param>
    /// <param name="session">The session instance.</param>
    /// <returns>Play directive of the last added items.</returns>
    public override SkillResponse Handle(Request request, Context context, Entities.User user, SessionInfo session)
    {
        InternalItemsQuery query = new InternalItemsQuery()
        {
            User = _userManager.GetUserById(session.UserId),
            IsFavorite = true,
            SourceTypes = new[] { SourceType.Library },
            DtoOptions = new MediaBrowser.Controller.Dto.DtoOptions(true)
        };

        List<BaseItem> favoriteItems = _libraryManager.GetItemList(query);

        if (favoriteItems.Count == 0)
        {
            return ResponseBuilder.Tell("No favorite items found.");
        }

        List<QueueItem> queueItems = new List<QueueItem>();

        for (int i = 0; i < favoriteItems.Count; i++)
        {
            BaseItem item = favoriteItems[i];
            queueItems.Add(new QueueItem
            {
                Id = item.Id,
                PlaylistItemId = null,
            });
        }

        session.NowPlayingQueue = queueItems;
        session.FullNowPlayingItem = favoriteItems[0];

        string item_id = favoriteItems[0].Id.ToString();
        string audioUrl = new Uri(new Uri(Plugin.Instance!.Configuration.ServerAddress), "/Audio/" + item_id + "/universal").ToString();

        return ResponseBuilder.AudioPlayerPlay(PlayBehavior.ReplaceAll, audioUrl, item_id);
    }
}