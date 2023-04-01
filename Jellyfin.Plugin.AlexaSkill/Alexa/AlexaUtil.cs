using System;
using System.Net;
using Jellyfin.Plugin.AlexaSkill.Entities;
using Jellyfin.Plugin.AlexaSkill.Lwa;

namespace Jellyfin.Plugin.AlexaSkill.Alexa;

/// <summary>
/// Util methods.
/// </summary>
public static class AlexaUtil
{
    /// <summary>
    /// Runs a function and retries with an new access token if the first call fails. Updates the new token in the database.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="user">The user to run the function for.</param>
    /// <param name="func">The function to run.</param>
    /// <returns>The result of the function.</returns>
    public static T Call<T>(User user, Func<T> func)
    {
        try
        {
            return func();
        }
        catch (Exception ex) when (ex.InnerException is Refit.ApiException)
        {
            if (((Refit.ApiException) ex.InnerException).StatusCode == HttpStatusCode.Unauthorized) {
                if (user.SmapiDeviceToken == null)
                {
                    throw ex;
                }

                // Refresh the token and try again
                DeviceToken? token = LwaClient.RefreshDeviceToken(
                    user.SmapiDeviceToken,
                    Plugin.Instance!.Configuration.LwaClientId,
                    Plugin.Instance!.Configuration.LwaClientSecret).Result;
                if (token == null)
                {
                    throw new UnauthorizedAccessException("Failed to refresh token");
                }

                user.SmapiDeviceToken = token;

                Plugin.Instance.SaveConfiguration();

                return func();
            }
            else
            {
                throw ex;
            }
        }
    }
}