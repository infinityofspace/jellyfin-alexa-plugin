using System;
using System.Collections.Generic;
using System.Web;
using Jellyfin.Plugin.AlexaSkill.Controller.Handler;
using Jellyfin.Plugin.AlexaSkill.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.AlexaSkill.Controller;

/// <summary>
/// Controller class for api requests.
/// </summary>
[ApiController]
[Route("alexaskill/api/")]
public class ConfigurationController : ControllerBase
{
    /// <summary>
    /// Uri of the plugin api.
    /// </summary>
    public const string ApiBaseUri = "alexaskill/api/";

    private readonly IUserManager _userManager;
    private readonly ILogger<ConfigurationController> _logger;
    private LwaAuthorizationRequestHandler lwaAuthorizationRequestHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationController"/> class.
    /// </summary>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILogger{RequestController}"/> interface.</param>
    public ConfigurationController(
        IUserManager userManager,
        ISessionManager sessionManager,
        ILibraryManager libraryManager,
        ILoggerFactory loggerFactory)
    {
        _userManager = userManager;
        _logger = loggerFactory.CreateLogger<ConfigurationController>();

        lwaAuthorizationRequestHandler = Plugin.Instance!.LwaAuthorizationRequestHandler;
    }

    /// <summary>
    /// Update the specified user skill.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="json">The json request body.</param>
    /// <returns>A <see cref="ActionResult"/>.</returns>
    [HttpPatch("user-skills/{userId}")]
    [Authorize(Policy = "RequiresElevation")]
    public ActionResult UpdateUserSkill([FromRoute] string userId, [FromBody] dynamic json)
    {
        Dictionary<string, string> req = JsonConvert.DeserializeObject<Dictionary<string, string>>(json.ToString());
        if (req.TryGetValue("invocationName", out var invocationName)
            && invocationName.Length > 0
            && invocationName.Split(" ").Length >= 2)
        {
            Jellyfin.Plugin.AlexaSkill.Entities.User? pluginUser = Plugin.Instance!.DbRepo.GetUser(new Guid(userId));
            if (pluginUser == null)
            {
                return new JsonResult(new { error = "Could not find user" }, StatusCode(404));
            }

            if (pluginUser.UserSkill == null)
            {
                return new JsonResult(new { error = "User has no skill" }, StatusCode(404));
            }

            pluginUser.UserSkill.InvocationName = invocationName;
            Plugin.Instance!.DbRepo.UpdateUser(pluginUser);

            return new OkResult();
        }
        else
        {
            return new JsonResult(new { error = "Invalid invocation name" }, StatusCode(400));
        }
    }

    /// <summary>
    /// Create a new user skill.
    /// </summary>
    /// <param name="json">The json request body.</param>
    /// <returns>A <see cref="ActionResult"/>.</returns>
    [HttpPost("user-skills")]
    [Authorize(Policy = "RequiresElevation")]
    public ActionResult CreateNewUserSkill([FromBody] dynamic json)
    {
        Dictionary<string, string> req = JsonConvert.DeserializeObject<Dictionary<string, string>>(json.ToString());
        if (req.TryGetValue("username", out var username) && username.Length > 0)
        {
            Jellyfin.Data.Entities.User user = _userManager.GetUserByName(username);
            if (user == null)
            {
                return new JsonResult(new { error = "Could not find user" }) { StatusCode = 404 };
            }

            UserSkill userSkill = new UserSkill
            {
                InvocationName = Config.InvocationName,
                UserSkillStatus = UserSkillStatus.LwaAuthPending
            };

            Jellyfin.Plugin.AlexaSkill.Entities.User? pluginUser =
                Plugin.Instance!.DbRepo.CreateUser(user.Id, string.Empty, userSkill);
            if (pluginUser == null)
            {
                return new JsonResult(new { error = "Could not create user with skill" })
                {
                    StatusCode = 500
                };
            }

            return new OkResult();
        }
        else
        {
            return new JsonResult(new { error = "Invalid username" }) { StatusCode = 400 };
        }
    }

    /// <summary>
    /// Delete the specified user skill.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>A <see cref="ActionResult"/>.</returns>
    [HttpDelete("user-skills/{userId}")]
    [Authorize(Policy = "RequiresElevation")]
    public ActionResult DeleteUserSkill([FromRoute] string userId)
    {
        Jellyfin.Plugin.AlexaSkill.Entities.User? pluginUser = Plugin.Instance!.DbRepo.GetUser(new Guid(userId));
        if (pluginUser == null)
        {
            return new JsonResult(new { error = "Could not find user" }, StatusCode(404));
        }

        Plugin.Instance!.DbRepo.DeleteUser(pluginUser.Id);

        return new OkResult();
    }

    /// <summary>
    /// Get a new lwa authorization url.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>A <see cref="ActionResult"/>.</returns>
    [HttpPut("user-skills/{userId}/authorization")]
    [Authorize(Policy = "RequiresElevation")]
    public ActionResult GetUserSkillAuthorisation([FromRoute] string userId)
    {
        Guid userIdGuid = new Guid(userId);
        Jellyfin.Plugin.AlexaSkill.Entities.User? pluginUser = Plugin.Instance!.DbRepo.GetUser(userIdGuid);
        if (pluginUser == null)
        {
            return new JsonResult(new { error = "Could not find user" }) { StatusCode = 404 };
        }
        else if (pluginUser.UserSkill == null)
        {
            return new JsonResult(new { error = "User has no skill" }) { StatusCode = 404 };
        }

        return new JsonResult(
            new
            {
                verificationUrl = LWAController.ApiBaseUri
                    + "?token="
                    + HttpUtility.UrlEncode(lwaAuthorizationRequestHandler.GetNewLwaAuthorizationRequest(userIdGuid))
            })
        {
            StatusCode = 200
        };
    }

    /// <summary>
    /// Delete database.
    /// </summary>
    /// <returns>A <see cref="ActionResult"/>.</returns>
    [HttpDelete("database")]
    [Authorize(Policy = "RequiresElevation")]
    public ActionResult DeleteDatabase()
    {
        Plugin.Instance!.DbRepo.DeleteDatabase();

        return new OkResult();
    }
}
