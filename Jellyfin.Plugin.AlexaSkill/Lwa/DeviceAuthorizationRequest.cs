namespace Jellyfin.Plugin.AlexaSkill.Lwa;

/// <summary>
/// Device authorization request.
/// </summary>
public class DeviceAuthorizationRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceAuthorizationRequest"/> class.
    /// </summary>
    /// <param name="userCode">The code to display to the user.</param>
    /// <param name="deviceCode">The unique device code.</param>
    /// <param name="verificationUri">The URL to where the user enters the user code.</param>
    /// <param name="expireTimestamp">The epoch timetstamp when the device code expires.</param>
    /// <param name="interval">The length of time in seconds you should wait between each Device Token Request.</param>
    public DeviceAuthorizationRequest(
        string userCode,
        string deviceCode,
        string verificationUri,
        long expireTimestamp,
        int interval)
    {
        UserCode = userCode;
        DeviceCode = deviceCode;
        VerificationUri = verificationUri;
        ExpireTimestamp = expireTimestamp;
        Interval = interval;
    }

    /// <summary>
    /// Gets the code to display to the user.
    /// </summary>
    public string UserCode { get; private set; }

    /// <summary>
    /// Gets the unique device code.
    /// </summary>
    public string DeviceCode { get; private set; }

    /// <summary>
    /// Gets the URL to where the user enters the user code.
    /// </summary>
    public string VerificationUri { get; private set; }

    /// <summary>
    /// Gets the epoch timetstamp when the device code expires.
    /// </summary>
    public long ExpireTimestamp { get; private set; }

    /// <summary>
    /// Gets the length of time in seconds you should wait between each Device Token Request.
    /// </summary>
    public int Interval { get; private set; }
}