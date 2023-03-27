using System;
using System.Security.Cryptography;
using Jellyfin.Plugin.AlexaSkill;
using Jellyfin.Plugin.AlexaSkill.Lwa;

namespace Jellyfin.Plugin.AlexaSkill.Controller.Handler;

/// <summary>
/// Class representing a CSRF token.
/// </summary>
public class LwaAuthorizationRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LwaAuthorizationRequest"/> class.
    /// </summary>
    /// <param name="userId">The user id.</param>
    public LwaAuthorizationRequest(Guid userId)
    {
        UserId = userId;
        PageToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(Config.LwaAuthorizePageTokenLength));
        Expiration = DateTime.UtcNow.AddMinutes(Config.LwaAuthorizePageTokenExpirationMinutes);
    }

    /// <summary>
    /// Gets the page token.
    /// </summary>
    /// <returns>The page token.</returns>
    public string PageToken { get; }

    /// <summary>
    /// Gets the expiration when the token expires.
    /// </summary>
    /// <returns>The expiration.</returns>
    public DateTime Expiration { get; }

    /// <summary>
    /// Gets the user id.
    /// </summary>
    /// <returns>The user id.</returns>
    public Guid UserId { get; }

    /// <summary>
    /// Gets or sets the device authorization request.
    /// </summary>
    /// <returns>The device authorization request.</returns>
    public DeviceAuthorizationRequest? DeviceAuthorizationRequest { get; set; } = null!;
}