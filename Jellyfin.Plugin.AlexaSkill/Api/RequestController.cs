using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Alexa.NET.Management.AccountLinking;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Jellyfin.Plugin.AlexaSkill.Alexa;
using Jellyfin.Plugin.AlexaSkill.Alexa.Handler;
using Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.AlexaSkill.Api;

/// <summary>
/// Controller class for api requests.
/// </summary>
[ApiController]
[Route("AlexaSkill/api/")]
public class RequestController : ControllerBase
{
    /// <summary>
    /// Uri of the plugin api.
    /// </summary>
    public const string ApiBaseUri = "AlexaSkill/api/";

    private readonly IUserManager _userManager;
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<RequestController> _logger;

    private static Dictionary<string, CsrfToken> csrfTokens = new Dictionary<string, CsrfToken>();

    private BaseHandler[] handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestController"/> class.
    /// </summary>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILogger{RequestController}"/> interface.</param>
    public RequestController(
        IUserManager userManager,
        ISessionManager sessionManager,
        ILibraryManager libraryManager,
        ILoggerFactory loggerFactory)
    {
        _userManager = userManager;
        _sessionManager = sessionManager;
        _logger = loggerFactory.CreateLogger<RequestController>();

        handler = new BaseHandler[]
        {
            new LaunchRequestHandler(sessionManager, Plugin.Instance!.DbRepo, loggerFactory),
            new PauseIntentHandler(sessionManager, Plugin.Instance!.DbRepo, loggerFactory),
            new NextIntentHandler(sessionManager, Plugin.Instance!.DbRepo, loggerFactory),
            new PreviousIntentHandler(sessionManager, Plugin.Instance!.DbRepo, loggerFactory),
            new PlayLastAddedIntentHandler(sessionManager, Plugin.Instance!.DbRepo, libraryManager, userManager, loggerFactory),
            new PlayFavoritesIntentHandler(sessionManager, Plugin.Instance!.DbRepo, libraryManager, userManager, loggerFactory),
            new PlaybackFailedEventHandler(sessionManager, Plugin.Instance!.DbRepo, libraryManager, userManager, loggerFactory),
            new PlaybackFinishedEventHandler(sessionManager, Plugin.Instance!.DbRepo, libraryManager, userManager, loggerFactory),
            new PlaybackNearlyFinishedEventHandler(sessionManager, Plugin.Instance!.DbRepo, libraryManager, userManager, loggerFactory),
            new PlaybackStartedEventHandler(sessionManager, Plugin.Instance!.DbRepo, libraryManager, userManager, loggerFactory),
            new PlaybackStoppedEventHandler(sessionManager, Plugin.Instance!.DbRepo, libraryManager, userManager, loggerFactory),
            new SessionEndedRequestHandler(sessionManager, Plugin.Instance!.DbRepo, libraryManager, userManager, loggerFactory),
        };
    }

    /// <summary>
    /// Get the account linking html page.
    /// </summary>
    /// <param name="clientId">The client id of the skill account linking request.</param>
    /// <param name="redirectUri">The redirect uri of the skill account linking request.</param>
    /// <param name="state">The state of the skill account linking request.</param>
    /// <returns>The account linking html page.</returns>
    [HttpGet("account-linking")]
    public ActionResult GetAccountLinking(
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery(Name = "state")] string state)
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
        Stream? resource = assembly.GetManifestResourceStream("Jellyfin.Plugin.AlexaSkill.Api.Pages.account_linking.html");

        if (resource == null)
        {
            return StatusCode(500);
        }

        string page = new StreamReader(resource).ReadToEnd();

        page = page.Replace("{{ csrf_token }}", GetNewCsrfToken(), StringComparison.Ordinal);

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
        if (!csrfTokens.ContainsKey(csrfToken))
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

        Entities.User user = Plugin.Instance!.DbRepo.CreateUser(username, authenticationResult.AccessToken);

        string accessToken = user.Token;

        string urlParams = $"access_token={accessToken}&state={state}&token_type=token";

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
        _logger.LogCritical("Alexa skill request");

        SkillRequest req = JsonConvert.DeserializeObject<SkillRequest>(json.ToString());

        string accessToken = req.Context.System.User.AccessToken;
        Entities.User? user = Plugin.Instance!.DbRepo.GetUserByToken(accessToken);
        if (user == null)
        {
            _logger.LogError("Invalid access token: {0}", accessToken);

            return Unauthorized();
        }

        if (req.Request == null)
        {
            return new NoContentResult();
        }

        _logger.LogInformation("{0}", req.Request.GetType().ToString());

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

        _logger.LogWarning("Unhandled skill request: {0}", req.Request.Type.ToString());

        return new NoContentResult();
    }

    /// <summary>
    /// Rebuild the skill in the Alexa cloud.
    /// </summary>
    /// <returns>A <see cref="ActionResult"/>.</returns>
    [HttpPatch("skill-rebuild")]
    [Authorize(Policy = "RequiresElevation")]
    public async Task<ActionResult> RebuildSkill()
    {
        PluginConfiguration configuration = Plugin.Instance!.Configuration;

        string skillId = configuration.SkillId;

        // check if we already have an active skill in the Alexa cloud
        if (string.IsNullOrEmpty(skillId))
        {
            // check if we can create a new skill

            // check if all credentials and tokens are available
            if (string.IsNullOrEmpty(configuration.SmapiClientId)
                || string.IsNullOrEmpty(configuration.SmapiClientSecret)
                || string.IsNullOrEmpty(configuration.SmapiRefreshToken)
                || string.IsNullOrEmpty(configuration.VendorId)
                || string.IsNullOrEmpty(configuration.ServerAddress))
            {
                _logger.LogInformation("Missing skill config value to create a new skill");
                // we don't have a skill active skill in the Alexa cloud and we have some required skill config values
                return new BadRequestResult();
            }
            else
            {
                try
                {
                    await AlexaUtil.CreateSkill().ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is UnauthorizedAccessException || ex is JsonException)
                {
                    _logger.LogError("Failed to create the skill", ex.Message);
                    return StatusCode(500);
                }

                return new OkResult();
            }
        }
        else
        {
            _logger.LogInformation("Update the existing skill with ID: {0}", skillId);
            // update the existing skill in the alexa cloud
            try
            {
                await Plugin.Instance.Skill.Update(skillId).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is UnauthorizedAccessException || ex is JsonException)
            {
                _logger.LogError("Failed to update existing skill manifest: {0}", ex.Message);
                return StatusCode(500);
            }

            try
            {
                // now update the interaction models
                foreach (SkillInteractionModel skillInteractionModel in Plugin.Instance.SkillInteractionModels)
                {
                    await skillInteractionModel.Update(skillId).ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is UnauthorizedAccessException || ex is JsonException)
            {
                _logger.LogError("Failed to update existing skill interaction: {0}", ex.Message);
                return StatusCode(500);
            }

            try
            {
                Uri endpointUri = new Uri(new Uri(Plugin.Instance.Configuration.ServerAddress), RequestController.ApiBaseUri);
                string endpointUriString = new Uri(endpointUri, "account-linking").ToString();

                AccountLinkData accountLinkData = new AccountLinkData()
                {
                    Type = AccountLinkType.IMPLICIT,
                    AuthorizationUrl = endpointUriString,
                    ClientId = Plugin.Instance.Configuration.AccountLinkingClientId,
                };

                await Plugin.Instance.SmapiManagement.AccountLinking.Update(skillId, accountLinkData).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is UnauthorizedAccessException || ex is JsonException)
            {
                _logger.LogError("Failed to update account liniking: {0}", ex.Message);
                return StatusCode(500);
            }

            return new OkResult();
        }
    }

    /// <summary>
    /// Delete the whole skill database.
    /// </summary>
    /// <returns>A <see cref="ActionResult"/>.</returns>
    [HttpDelete("database")]
    [Authorize(Policy = "RequiresElevation")]
    public ActionResult DeleteDatabase()
    {
        Plugin.Instance!.DbRepo.DeleteDatabase();

        return new OkResult();
    }

    private void RemoveExpiredCsrfTokens()
    {
        // iter over all csrf tokens and remove the expired ones
        foreach (KeyValuePair<string, CsrfToken> csrfToken in csrfTokens)
        {
            if (csrfToken.Value.Expiration > DateTime.Now)
            {
                csrfTokens.Remove(csrfToken.Key);
            }
        }
    }

    private bool ValidateCsrfToken(string token)
    {
        CsrfToken? csfrToken;
        if (csrfTokens.TryGetValue(token, out csfrToken))
        {
            return false;
        }

        // validate expiration
        if (csfrToken!.Expiration < DateTime.UtcNow)
        {
            return true;
        }
        else
        {
            csrfTokens.Remove(token);

            RemoveExpiredCsrfTokens();

            return false;
        }
    }

    private string GetNewCsrfToken()
    {
        CsrfToken csfrToken;

        while (true)
        {
            try
            {
                csfrToken = new CsrfToken();
                csrfTokens.Add(csfrToken.Token, csfrToken);
                break;
            }
            catch (System.ArgumentException)
            {
                continue;
            }
        }

        return csfrToken.Token;
    }

    private class CsrfToken
    {
        public string Token { get; } = Convert.ToBase64String(RandomNumberGenerator.GetBytes(Config.CsrfTokenLength));

        public DateTime Expiration { get; } = DateTime.UtcNow.AddMinutes(Config.CsrfTokenExpirationMinutes);
    }
}
