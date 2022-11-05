using System;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Jellyfin.Plugin.AlexaSkill.Data;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Alexa.Handler;

/// <summary>
/// Base handler class to handle skill requests.
/// </summary>
public abstract class BaseHandler
{
    private DbRepo _dbRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseHandler"/> class.
    /// </summary>
    /// <param name="sessionManager">The session manager instance.</param>
    /// <param name="dbRepo">The database repository instance.</param>
    /// <param name="loggerFactory">The logger factory instance.</param>
    protected BaseHandler(ISessionManager sessionManager, DbRepo dbRepo, ILoggerFactory loggerFactory)
    {
        SessionManager = sessionManager;
        _dbRepo = dbRepo;
        Logger = loggerFactory.CreateLogger<BaseHandler>();
    }

    /// <summary>
    /// Gets or sets the session manager instance.
    /// </summary>
    protected ISessionManager SessionManager { get; set; }

    /// <summary>
    /// Gets or sets logger instance.
    /// </summary>
    protected ILogger Logger { get; set; }

    /// <summary>
    /// Handle a skill reuqest by calling the class Handle method and return a skill response.
    /// </summary>
    /// <param name="request">The skill request to handle.</param>
    /// <param name="context">The lambda context.</param>
    /// <returns>The skill response to the request.</returns>
    public SkillResponse HandleRequest(Request request, Context context)
    {
        string token = context.System.User.AccessToken;

        Entities.User? user = _dbRepo.GetUserByToken(token);
        if (user == null)
        {
            Logger.LogError("User not found");

            return ResponseBuilder.Tell("User not found. Please relink your account.");
        }

        SessionInfo session = SessionManager.GetSessionByAuthenticationToken(token, context.System.Device.DeviceID, Plugin.Instance!.Configuration.ServerAddress).Result;

        return Handle(request, context, user, session);
    }

    /// <summary>
    /// Determines whether this instance can handle the skill request.
    /// </summary>
    /// <param name="request">The Request type what this handler can process.</param>
    /// <returns>True if this handle can handle the given request type, false otherwise.</returns>
    public abstract bool CanHandle(Request request);

    /// <summary>
    /// Handle a skill reuqest and return a skill response.
    /// </summary>
    /// <param name="request">The skill request to handle.</param>
    /// <param name="context">The lambda context.</param>
    /// <param name="user">The user instance.</param>
    /// <param name="session">The session instance.</param>
    /// <returns>The skill response to the request.</returns>
    public abstract SkillResponse Handle(Request request, Context context, Entities.User user, SessionInfo session);

    /// <summary>
    /// Get a stream url for the given item.
    /// </summary>
    /// <param name="itemId">Id of the item to stream.</param>
    /// <param name="user">The user for which the item should be played.</param>
    /// <returns>Streamable url of the requested item.</returns>
    public string GetStreamUrl(string itemId, Entities.User user)
    {
        // TODO: add possible transcoding
        return new Uri(new Uri(Plugin.Instance!.Configuration.ServerAddress), "/Items/" + itemId + "/Download?api_key=" + user.Token).ToString();
    }
}