using System;
using System.IO;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Jellyfin.Plugin.AlexaSkill.Alexa.Handler;
using Jellyfin.Plugin.AlexaSkill.Controller.Handler;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.AlexaSkill.Controller;

/// <summary>
/// Controller class for api requests.
/// </summary>
[ApiController]
[Route("alexaskill/api/")]
public class AlexaSkillController : ControllerBase
{
    /// <summary>
    /// Uri of the plugin api.
    /// </summary>
    public const string ApiBaseUri = "alexaskill/api/";

    private readonly IUserManager _userManager;
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<AlexaSkillController> _logger;

    private BaseHandler[] handler;

    private CsrfTokenHandler csrfTokenHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlexaSkillController"/> class.
    /// </summary>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILogger{RequestController}"/> interface.</param>
    public AlexaSkillController(
        IUserManager userManager,
        ISessionManager sessionManager,
        ILibraryManager libraryManager,
        ILoggerFactory loggerFactory)
    {
        _userManager = userManager;
        _sessionManager = sessionManager;
        _logger = loggerFactory.CreateLogger<AlexaSkillController>();

        csrfTokenHandler = Plugin.Instance!.CsrfTokenHandler;

        handler = new BaseHandler[]
        {
            new LaunchRequestHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, loggerFactory),

            new PlayIntentHandler(sessionManager, Plugin.Instance!.Configuration, loggerFactory),
            new PauseIntentHandler(sessionManager, Plugin.Instance!.Configuration, loggerFactory),
            new NextIntentHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, loggerFactory),
            new PreviousIntentHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, loggerFactory),
            new ResumeIntentHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, loggerFactory),

            new PlayLastAddedIntentHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, userManager, loggerFactory),
            new PlayPlaylistIntentHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, userManager, loggerFactory),
            new PlayFavoritesIntentHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, userManager, loggerFactory),
            new PlayArtistSongsIntentHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, userManager, loggerFactory),
            new PlayAlbumIntentHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, userManager, loggerFactory),

            new MediaInfoIntentHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, userManager, loggerFactory),

            new PlaybackFailedEventHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, userManager, loggerFactory),
            new PlaybackFinishedEventHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, userManager, loggerFactory),
            new PlaybackNearlyFinishedEventHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, userManager, loggerFactory),
            new PlaybackStartedEventHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, userManager, loggerFactory),
            new PlaybackStoppedEventHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, userManager, loggerFactory),
            new SessionEndedRequestHandler(sessionManager, Plugin.Instance!.Configuration, libraryManager, userManager, loggerFactory),

            new ExceptionHandler(sessionManager, Plugin.Instance!.Configuration, loggerFactory),
            new FallbackIntentHandler(sessionManager, Plugin.Instance!.Configuration, loggerFactory)
        };
    }

    /// <summary>
    /// Get the account linking html page.
    /// </summary>
    /// <param name="clientId">The client id of the skill account linking request.</param>
    /// <param name="redirectUri">The redirect uri of the skill account linking request.</param>
    /// <param name="state">The state of the skill account linking request.</param>
    /// <param name="error">The error of the skill account linking request.</param>
    /// <returns>The account linking html page.</returns>
    [HttpGet("account-linking")]
    public ActionResult GetAccountLinking(
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery(Name = "state")] string state,
        [FromQuery(Name = "error")] string? error)
    {
        bool valid_redirect_uri = false;
        foreach (string url in Config.ValidRedirectUrls)
        {
            if (redirectUri.StartsWith(url, StringComparison.Ordinal))
            {
                valid_redirect_uri = true;
                break;
            }
        }

        if (!valid_redirect_uri)
        {
            _logger.LogError("Invalid redirect uri: {0}", redirectUri);

            return new BadRequestResult();
        }

        if (!clientId.Equals(Plugin.Instance!.Configuration.AccountLinkingClientId, StringComparison.Ordinal))
        {
            _logger.LogError("Invalid client id: {0}", clientId);

            return new BadRequestResult();
        }

        var assembly = typeof(Util).Assembly;
        Stream? resource = assembly.GetManifestResourceStream("Jellyfin.Plugin.AlexaSkill.Controller.Pages.account_linking.html");

        if (resource == null)
        {
            return StatusCode(500);
        }

        string page = new StreamReader(resource).ReadToEnd();

        page = page.Replace("{{ csrf_token }}", csrfTokenHandler.GetNewCsrfToken().Token, StringComparison.Ordinal);

        if (error != null)
        {
            page = page.Replace("{{ error }}", error, StringComparison.Ordinal);
        }
        else
        {
            page = page.Replace("{{ error }}", string.Empty, StringComparison.Ordinal);
        }

        return Content(page, "text/html");
    }

    /// <summary>
    /// Post the Jellyfin username and passwort for the account linking html page.
    /// </summary>
    /// <param name="csrfToken">The CSRF token to verify the account linking request.</param>
    /// <param name="username">The username of the Jellyfin account.</param>
    /// <param name="password">The password of the Jellyfin account.</param>
    /// <param name="clientId">The client id of the skill account linking request.</param>
    /// <param name="redirectUri">The redirect uri of the skill account linking request.</param>
    /// <param name="state">The state of the skill account linking request.</param>
    /// <returns>The html page.</returns>
    [HttpPost("account-linking")]
    public async Task<ActionResult> PostAccountLinking(
        [FromForm(Name = "csrf_token")] string csrfToken,
        [FromForm(Name = "username")] string username,
        [FromForm(Name = "password")] string password,
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery(Name = "state")] string state)
    {
        if (!csrfTokenHandler.ValidateCsrfToken(csrfToken))
        {
            return Unauthorized();
        }

        bool valid_redirect_uri = false;
        foreach (string url in Config.ValidRedirectUrls)
        {
            if (redirectUri.StartsWith(url, StringComparison.Ordinal))
            {
                valid_redirect_uri = true;
                break;
            }
        }

        if (!valid_redirect_uri)
        {
            _logger.LogError("Invalid redirect uri: {0}", redirectUri);

            return new BadRequestResult();
        }

        if (!clientId.Equals(Plugin.Instance!.Configuration.AccountLinkingClientId, StringComparison.Ordinal))
        {
            _logger.LogError("Invalid client id: {0}", clientId);

            return new BadRequestResult();
        }

        AuthenticationRequest authenticationRequest = new AuthenticationRequest();
        authenticationRequest.Username = username;
        authenticationRequest.Password = password;
        authenticationRequest.AppVersion = Util.GetVersion();
        authenticationRequest.App = "Alexa Skill";
        authenticationRequest.DeviceId = "AlexaDevice";
        authenticationRequest.DeviceName = "Alexa enabled device";

        AuthenticationResult authenticationResult;
        try
        {
            authenticationResult = await _sessionManager.AuthenticateNewSession(authenticationRequest).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is MediaBrowser.Controller.Authentication.AuthenticationException)
        {
            _logger.LogError(ex, "Failed to authenticate user");

            return Redirect("account-linking?error=invalid credentials&client_id=" + clientId + "&redirect_uri=" + redirectUri + "&state=" + state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong during authenticate the user");

            return Redirect("account-linking?error=unknown error&client_id=" + clientId + "&redirect_uri=" + redirectUri + "&state=" + state);
        }

        Entities.User? user = Plugin.Instance.Configuration.GetUserById(authenticationResult.User.Id);
        if (user == null)
        {
            return Redirect("account-linking?error=this user have no user skill&client_id=" + clientId + "&redirect_uri=" + redirectUri + "&state=" + state);
        }

        user.JellyfinToken = authenticationResult.AccessToken;
        Plugin.Instance!.SaveConfiguration();

        string urlParams = $"access_token={user.Id.ToString()}&state={state}&token_type=token";

        return RedirectPermanent(redirectUri + "#" + urlParams);
    }

    /// <summary>
    /// Handle a Alexa skill request.
    /// </summary>
    /// <param name="json">Alexa skill request.</param>
    /// <returns>A <see cref="ActionResult"/>.</returns>
    [HttpPost("alexa-request")]
    [Consumes("application/json")]
    public ActionResult HandleIntentRequest([FromBody] dynamic json)
    {
        SkillRequest req = JsonConvert.DeserializeObject<SkillRequest>(json.ToString());

        Guid userId = new Guid(req.Context.System.User.AccessToken);
        Entities.User? user = Plugin.Instance!.Configuration.GetUserById(userId);
        if (user == null)
        {
            _logger.LogError("User not found or invalid access token: {0}", userId);

            return Unauthorized();
        }

        if (req.Request == null)
        {
            return new NoContentResult();
        }

        _logger.LogInformation("Alexa request of type: {0}", req.Request.GetType().ToString());

        foreach (BaseHandler h in handler)
        {
            if (h.CanHandle(req.Request))
            {
                SkillResponse skillResponse = h.HandleRequest(req.Request, req.Context);

                ContentResult res = new ContentResult();
                res.Content = JsonConvert.SerializeObject(skillResponse);
                return res;
            }
        }

        if (req.Request is IntentRequest)
        {
            _logger.LogWarning("Unhandled skill intent request: {0}", ((IntentRequest)req.Request).Intent.Name);

            ContentResult res = new ContentResult();
            res.Content = JsonConvert.SerializeObject(ResponseBuilder.Tell("This intent is not implemented yet."));
            return res;
        }
        else
        {
            _logger.LogWarning("Unhandled skill request: {0}", req.Request.Type);
        }

        return new BadRequestResult();
    }
}