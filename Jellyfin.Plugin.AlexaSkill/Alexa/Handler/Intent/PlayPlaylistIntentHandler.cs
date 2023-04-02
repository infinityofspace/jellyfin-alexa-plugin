using System.Collections.Generic;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Handler;

/// <summary>
/// Handler for PlayPlaylist intents.
/// </summary>
public class PlayPlaylistIntentHandler : BaseHandler
{
    private ILibraryManager _libraryManager;
    private IUserManager _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayPlaylistIntentHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
    public PlayPlaylistIntentHandler(
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
        return intentRequest != null && string.Equals(intentRequest.Intent.Name, "PlayPlaylistIntent", System.StringComparison.Ordinal);
    }

    /// <summary>
    /// Play a playlist by its name.
    /// </summary>
    /// <param name="request">The skill request which should be handled.</param>
    /// <param name="context">The context of the skill intent request.</param>
    /// <param name="user">The user instance.</param>
    /// <param name="session">The session instance.</param>
    /// <returns>Play directive of the playlist.</returns>
    public override SkillResponse Handle(Request request, Context context, Entities.User user, SessionInfo session)
    {
        IntentRequest intentRequest = (IntentRequest)request;

        string playlistName = intentRequest.Intent.Slots["playlist"].Value;

        Logger.LogDebug("Play playlist: {0}", playlistName);

        Jellyfin.Data.Entities.User jellyfinUser = _userManager.GetUserById(session.UserId);

        InternalItemsQuery query = new InternalItemsQuery()
        {
            User = jellyfinUser,
            SearchTerm = playlistName,
            IncludeItemTypes = new[] { BaseItemKind.Playlist },
            DtoOptions = new DtoOptions(true),
        };

        QueryResult<BaseItem> playlists = _libraryManager.GetItemsResult(query);

        if (playlists.TotalRecordCount == 0)
        {
            return ResponseBuilder.Tell("Could not find a playlist with the name " + playlistName);
        }

        BaseItem playlist = playlists.Items[0];

        // Get the playlist items
        IReadOnlyList<BaseItem> playlistItems = ((Folder)playlist).GetItemList(new InternalItemsQuery()
        {
            User = jellyfinUser,
            Recursive = true,
            MediaTypes = new[] { MediaType.Audio },
            DtoOptions = new DtoOptions(true),
        });

        if (playlistItems.Count == 0)
        {
            return ResponseBuilder.Tell("The playlist is empty.");
        }

        List<QueueItem> queueItems = new List<QueueItem>();
        for (int i = 0; i < playlistItems.Count; i++)
        {
            BaseItem item = playlistItems[i];
            queueItems.Add(new QueueItem
            {
                Id = item.Id,
                PlaylistItemId = playlist.Id.ToString(),
            });
        }

        session.NowPlayingQueue = queueItems;

        BaseItem firstItem = _libraryManager.GetItemById(queueItems[0].Id);
        session.FullNowPlayingItem = firstItem;

        string item_id = firstItem.Id.ToString();

        return ResponseBuilder.AudioPlayerPlay(PlayBehavior.ReplaceAll, GetStreamUrl(item_id, user), item_id);
    }
}