using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Jellyfin.Plugin.AlexaSkill.Alexa;
using Jellyfin.Plugin.AlexaSkill.Alexa.InteractionModel;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using Jellyfin.Plugin.AlexaSkill.Controller.Handler;
using Jellyfin.Plugin.AlexaSkill.Entities;
using Jellyfin.Plugin.AlexaSkill.Lwa;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Controller;

/// <summary>
/// Controller class for api requests.
/// </summary>
[ApiController]
[Route("alexaskill/lwa/")]
public class LWAController : ControllerBase
{
    /// <summary>
    /// Uri of the plugin api.
    /// </summary>
    public const string ApiBaseUri = "alexaskill/lwa/";

    private readonly ILogger<LWAController> _logger;

    private LwaAuthorizationRequestHandler lwaAuthorizationRequestHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="LWAController"/> class.
    /// </summary>
    /// <param name="loggerFactory">Instance of the <see cref="ILogger{RequestController}"/> interface.</param>
    public LWAController(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<LWAController>();

        lwaAuthorizationRequestHandler = Plugin.Instance!.LwaAuthorizationRequestHandler;
    }

    /// <summary>
    /// Get the user instructions to use LWA.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The lwa html page.</returns>
    [HttpGet]
    public ActionResult GetLwaDeviceTokenRequestPage([FromQuery(Name = "token")] string token)
    {
        var assembly = typeof(Util).Assembly;
        string page;

        if (
            Plugin.Instance!.Configuration.LwaClientId == null
            || Plugin.Instance!.Configuration.LwaClientSecret == null
        )
        {
            Stream? resource = assembly.GetManifestResourceStream("Jellyfin.Plugin.AlexaSkill.Controller.Pages.lwa_error.html");

            if (resource == null)
            {
                return new JsonResult(new { error = "Could not load html page" })
                {
                    StatusCode = 500
                };
            }

            page = new StreamReader(resource).ReadToEnd();
            page = page.Replace(
                "{{ error }}",
                "Login with Amazon (LWA) is not setup yet.<br>Please contact your Jelllyfin admin to set up LWA for the plugin.",
                StringComparison.Ordinal);
        }
        else
        {
            LwaAuthorizationRequest? lwaAuthorizationRequest =
                lwaAuthorizationRequestHandler.GetLwaAuthorizationRequest(token);
            // validate the token
            if (
                !lwaAuthorizationRequestHandler.ValidatLwaAuthorizePageToken(token)
                || lwaAuthorizationRequest == null
            )
            {
                Stream? resource = assembly.GetManifestResourceStream("Jellyfin.Plugin.AlexaSkill.Controller.Pages.lwa_error.html");

                if (resource == null)
                {
                    return new JsonResult(new { error = "Could not load html page" })
                    {
                        StatusCode = 500
                    };
                }

                page = new StreamReader(resource).ReadToEnd();
                page = page.Replace("{{ error }}", "Invalid or expired page token", StringComparison.Ordinal);
            }
            else
            {
                Stream? resource = assembly.GetManifestResourceStream("Jellyfin.Plugin.AlexaSkill.Controller.Pages.lwa_device_token_request.html");

                if (resource == null)
                {
                    return new JsonResult(new { error = "Could not load html page" })
                    {
                        StatusCode = 500
                    };
                }

                page = new StreamReader(resource).ReadToEnd();

                // check if a device token request is already initiated
                if (lwaAuthorizationRequest.DeviceAuthorizationRequest == null)
                {
                    // initiate a new lwa device token request
                    DeviceAuthorizationRequest? request = LwaClient
                        .CreateLwaDeviceAuthorizationRequest(
                            Plugin.Instance!.Configuration.LwaClientId,
                            new Lwa.Scope[]
                            {
                                Lwa.Scope.SkillsReadWrite,
                                Lwa.Scope.ModelsReadWrite
                            }).Result;

                    if (request == null)
                    {
                        return new JsonResult(new { error = "Could not create device authorization request" })
                        {
                            StatusCode = 500
                        };
                    }

                    lwaAuthorizationRequest.DeviceAuthorizationRequest = request;

                    // start a new thread to poll for the device token
                    Thread thread = new Thread(() =>
                    {
                        DeviceToken? deviceToken;
                        try
                        {
                            deviceToken = LwaClient.GetDeviceToken(request).Result;
                            if (deviceToken == null)
                            {
                                _logger.LogError("Could not get lwa device token");
                                return;
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Error while requesting lwa device token");
                            return;
                        }

                         // update the user
                        Jellyfin.Plugin.AlexaSkill.Entities.User? user =
                        Plugin.Instance!.Configuration.GetUserById(lwaAuthorizationRequest.UserId);
                        if (user == null)
                        {
                            _logger.LogError("Could not find user");
                            return;
                        }

                        user.SmapiDeviceToken = deviceToken;
                        if (user.UserSkill == null)
                        {
                            user.UserSkill = new UserSkill { InvocationName = Config.InvocationName };
                        }

                        user.UserSkill.UserSkillStatus = UserSkillStatus.SkillCreating;
                        Plugin.Instance!.SaveConfiguration();
                        lwaAuthorizationRequestHandler.RemoveLwaAuthorizeRequest(token);

                        // create the skill
                        if (user.SmapiManagement == null)
                        {
                            _logger.LogError("SmapiManagement is null for user with id {0}", user.Id);
                        }
                        else
                        {
                            _logger.LogInformation("New skill will be created for user with id {0}", user.Id);
                            Collection<SkillInteractionModel> skillInteractionModels = new Collection<SkillInteractionModel>();
                            foreach (Tuple<string, string> model in Plugin.Instance.InteractionModels)
                            {
                                skillInteractionModels.Add(new SkillInteractionModel(model.Item1, model.Item2, user.UserSkill.InvocationName));
                            }

                            PluginConfiguration configuration = Plugin.Instance.Configuration;

                            Uri endpointUri = new Uri(new Uri(configuration.ServerAddress), AlexaSkillController.ApiBaseUri);
                            string endpointUriString = new Uri(endpointUri, "account-linking").ToString();

                            string skillId = AlexaUtil.Call(user, () => user.SmapiManagement.CreateSkill(
                                Plugin.Instance.ManifestSkill!,
                                skillInteractionModels,
                                endpointUriString,
                                configuration.AccountLinkingClientId));

                            user.UserSkill.SkillId = skillId;
                            user.UserSkill.UserSkillStatus = UserSkillStatus.AccountLinkPending;
                            Plugin.Instance!.SaveConfiguration();
                        }
                    });

                    thread.Start();
                }

                page = page.Replace(
                    "{{ userCode }}",
                    lwaAuthorizationRequest.DeviceAuthorizationRequest.UserCode,
                    StringComparison.Ordinal);
                page = page.Replace(
                    "{{ verificationUri }}",
                    lwaAuthorizationRequest.DeviceAuthorizationRequest.VerificationUri,
                    StringComparison.Ordinal);
            }
        }

        return Content(page, "text/html");
    }
}
