namespace Jellyfin.Plugin.AlexaSkill;

/// <summary>
/// Global config values for the plugin.
/// </summary>
public static class Config
{
    /// <summary>
    /// The name of the Alexa skill.
    /// </summary>
    public const string SkillName = "Jellyfin";

    /// <summary>
    /// The default invocation name of the Alexa skill.
    /// </summary>
    public const string InvocationName = "jellyfin player";

    /// <summary>
    /// Length of the CSRF token.
    /// </summary>
    public const int CsrfTokenLength = 1024;

    /// <summary>
    /// Expiration time of the CSRF token in minutes.
    /// </summary>
    public const int CsrfTokenExpirationMinutes = 10;

    /// <summary>
    /// Length of the LWA authorization page token.
    /// </summary>
    public const int LwaAuthorizePageTokenLength = 6;

    /// <summary>
    /// Expiration time of the CSRF token in minutes.
    /// </summary>
    public const int LwaAuthorizePageTokenExpirationMinutes = 10;

    /// <summary>
    /// Name of the database file.
    /// </summary>
    public const string DbFilePath = "alexa-skill-plugin.db";

    /// <summary>
    /// Valid redirect urls for the Alexa skill during account linking process.
    /// </summary>
    public static readonly string[] ValidRedirectUrls = new string[]
    {
        "https://alexa.amazon.co.jp/spa/skill/account-linking-status.html?vendorId=",
        "https://layla.amazon.com/spa/skill/account-linking-status.html?vendorId=",
        "https://pitangui.amazon.com/spa/skill/account-linking-status.html?vendorId=",
    };
}