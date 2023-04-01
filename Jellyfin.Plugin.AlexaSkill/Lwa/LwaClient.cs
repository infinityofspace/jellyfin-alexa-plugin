using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.AlexaSkill.Lwa;

/// <summary>
/// Client interacting with LWA to authorize the user for SMAPI access.
/// </summary>
public static class LwaClient
{
    /// <summary>
    /// Create a device authorization request to LWA.
    /// </summary>
    /// <param name="clientId">LWA client id.</param>
    /// <param name="scopes">List scopes of the request.</param>
    /// <returns>Access token.</returns>
    public static async Task<DeviceAuthorizationRequest?> CreateLwaDeviceAuthorizationRequest(string clientId, Scope[] scopes)
    {
        string url = "https://api.amazon.com/auth/o2/create/codepair";

        string scopeString = string.Empty;
        for (int i = 0; i < scopes.Length; i++)
        {
            if (i > 0)
            {
                scopeString += " ";
            }

            scopeString += ScopeMethods.ScopeToString(scopes[i]);
        }

        var formUrlEncodedContent = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "response_type", "device_code" },
            { "client_id", clientId },
            { "scope", scopeString }
        });

        HttpResponseMessage response = await Plugin.HttpClient.PostAsync(url, formUrlEncodedContent).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            Dictionary<string, string>? json = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            if (json != null
                && json.TryGetValue("user_code", out var userCode)
                && json.TryGetValue("device_code", out var deviceCode)
                && json.TryGetValue("verification_uri", out var verificationUri)
                && json.TryGetValue("expires_in", out var expiresInStr)
                && int.TryParse(expiresInStr, out int expiresIn)
                && json.TryGetValue("interval", out var intervalStr)
                && int.TryParse(intervalStr, out int interval))
            {
                return new DeviceAuthorizationRequest(
                    userCode,
                    deviceCode,
                    verificationUri,
                    new DateTimeOffset(DateTime.UtcNow).AddSeconds(expiresIn).ToUnixTimeSeconds(),
                    interval);
            }
            else
            {
                throw new JsonException("Could not get access token: " + content);
            }
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the device token for authenticated requests to SMAPI.
    /// </summary>
    /// <param name="deviceAuthorizationRequest">Device authorization request.</param>
    /// <returns>Devive token.</returns>
    public static async Task<DeviceToken?> GetDeviceToken(DeviceAuthorizationRequest deviceAuthorizationRequest)
    {
        string url = "https://api.amazon.com/auth/o2/token";
        var formUrlEncodedContent = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "grant_type", "device_code" },
            { "device_code", deviceAuthorizationRequest.DeviceCode },
            { "user_code", deviceAuthorizationRequest.UserCode }
        });

        // poll the api until we reach the timeout or the user granted the authorization
        while (deviceAuthorizationRequest.ExpireTimestamp > new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds())
        {
            HttpResponseMessage response = await Plugin.HttpClient.PostAsync(url, formUrlEncodedContent).ConfigureAwait(false);

            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Dictionary<string, string>? json = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            if (response.IsSuccessStatusCode)
            {
                if (json != null
                && json.TryGetValue("access_token", out var token)
                && json.TryGetValue("refresh_token", out var refreshToken)
                && json.TryGetValue("token_type", out var tokenType)
                && json.TryGetValue("expires_in", out var expiresInStr)
                && int.TryParse(expiresInStr, out int expiresIn))
                {
                    return new DeviceToken(token, refreshToken, tokenType, new DateTimeOffset(DateTime.UtcNow).AddSeconds(expiresIn).ToUnixTimeSeconds());
                }
                else
                {
                    throw new JsonException("Could not get device token: " + content);
                }
            }
            else if (json != null && json.TryGetValue("error", out var error) && error == "authorization_pending")
            {
                // wait for the interval before polling again
                Thread.Sleep(deviceAuthorizationRequest.Interval * 1000);
            }
            else
            {
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// Refreshes the access token.
    /// </summary>
    /// <param name="deviceToken">Device token.</param>
    /// <param name="clientId">LWA client id.</param>
    /// <param name="clientSecret">LWA client secret.</param>
    /// <returns>Devive token.</returns>
    public static async Task<DeviceToken?> RefreshDeviceToken(DeviceToken deviceToken, string clientId, string clientSecret)
    {
        string url = "https://api.amazon.com/auth/o2/token";
        var formUrlEncodedContent = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", deviceToken.RefreshToken },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        });

        HttpResponseMessage response = await Plugin.HttpClient.PostAsync(url, formUrlEncodedContent).ConfigureAwait(false);

        string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Dictionary<string, string>? json = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

        if (response.IsSuccessStatusCode)
        {
            if (json != null
            && json.TryGetValue("access_token", out var token)
            && json.TryGetValue("refresh_token", out var refreshToken)
            && json.TryGetValue("token_type", out var tokenType)
            && json.TryGetValue("expires_in", out var expiresInStr)
            && int.TryParse(expiresInStr, out int expiresIn))
            {
                return new DeviceToken(token, refreshToken, tokenType, new DateTimeOffset(DateTime.UtcNow).AddSeconds(expiresIn).ToUnixTimeSeconds());
            }
            else
            {
                throw new JsonException("Could not get device token: " + content);
            }
        }
        else
        {
            return null;
        }
    }
}