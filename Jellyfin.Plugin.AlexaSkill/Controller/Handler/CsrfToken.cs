using System;
using System.Security.Cryptography;

namespace Jellyfin.Plugin.AlexaSkill.Controller.Handler;

/// <summary>
/// Class representing a CSRF token.
/// </summary>
public class CsrfToken
{
    /// <summary>
    /// Gets the token.
    /// </summary>
    /// <returns>The token.</returns>
    public string Token { get; } = Convert.ToBase64String(RandomNumberGenerator.GetBytes(Config.CsrfTokenLength));

    /// <summary>
    /// Gets the expiration when the token expires.
    /// </summary>
    /// <returns>The expiration.</returns>
    public DateTime Expiration { get; } = DateTime.UtcNow.AddMinutes(Config.CsrfTokenExpirationMinutes);
}