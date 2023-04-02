namespace Jellyfin.Plugin.AlexaSkill.Lwa;

/// <summary>
/// Device token.
/// </summary>
public class DeviceToken
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceToken"/> class.
    /// </summary>
    public DeviceToken()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceToken"/> class.
    /// </summary>
    /// <param name="accessToken">The access token for the user.</param>
    /// <param name="refreshToken">The refresh token that can be used to request a new access token.</param>
    /// <param name="tokenType">The type of the token.</param>
    /// <param name="expireTimestamp">The epoch timetstamp when the access token expires.</param>
    public DeviceToken(
        string accessToken,
        string refreshToken,
        string tokenType,
        long expireTimestamp)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenType = tokenType;
        ExpireTimestamp = expireTimestamp;
    }

    /// <summary>
    /// Gets or sets the access token for the user.
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the refresh token that can be used to request a new access token.
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the type of the token.
    /// </summary>
    public string TokenType { get; set; }

    /// <summary>
    /// Gets or sets the epoch timetstamp when the access token expires.
    /// </summary>
    public long ExpireTimestamp { get; set; }
}