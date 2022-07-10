using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Jellyfin.Plugin.AlexaSkill.Configuration;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.AlexaSkill.Alexa;

/// <summary>
/// Client interacting with the SMAPI.
/// </summary>
public class SmapiAccessToken
{
    private string? accessToken = null;
    private DateTime? accessTokenExpiration = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmapiAccessToken"/> class.
    /// </summary>
    public SmapiAccessToken()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmapiAccessToken"/> class.
    /// </summary>
    /// <param name="accessToken">Smapi access token.</param>
    public SmapiAccessToken(string accessToken)
    {
        this.accessToken = accessToken;
    }

    /// <summary>
    /// Gets the access token for authenticating requests to SMAPI.
    /// </summary>
    /// <returns>Access token.</returns>
    public async Task<string> GetAccessToken()
    {
        if ((!string.IsNullOrEmpty(accessToken) && accessTokenExpiration == null)
            || (!string.IsNullOrEmpty(accessToken) && accessTokenExpiration != null && accessTokenExpiration < DateTime.UtcNow))
        {
            return accessToken;
        }

        PluginConfiguration configuration = Plugin.Instance!.Configuration;

        if (string.IsNullOrEmpty(configuration.SmapiClientId)
            || string.IsNullOrEmpty(configuration.SmapiClientSecret)
            || string.IsNullOrEmpty(configuration.SmapiRefreshToken))
        {
            throw new UnauthorizedAccessException("Missing SMAPI credentials.");
        }

        string url = "https://api.amazon.com/auth/o2/token";
        var formUrlEncodedContent = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", configuration.SmapiRefreshToken },
            { "client_id", configuration.SmapiClientId },
            { "client_secret", configuration.SmapiClientSecret }
        });

        // send request
        HttpResponseMessage response = await Plugin.HttpClient.PostAsync(url, formUrlEncodedContent).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            Dictionary<string, string> json = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            if (json.TryGetValue("access_token", out var token) && json.TryGetValue("expires_in", out var expiresIn) && int.TryParse(expiresIn, out int offset))
            {
                accessTokenExpiration = DateTime.Now.AddSeconds(offset);
                accessToken = token;
                Plugin.Instance!.SaveConfiguration();
                return token;
            }
            else
            {
                throw new JsonException("Could not get access token: " + content);
            }
        }
        else
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Invalid client id, client secret or refresh token");
            }
            else
            {
                throw new HttpRequestException("Could not get access token");
            }
        }
    }
}