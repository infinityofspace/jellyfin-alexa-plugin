using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.AlexaSkill.Controller.Handler;

/// <summary>
/// Class to handle CSRF tokens.
/// </summary>
public class CsrfTokenHandler
{
    private Dictionary<string, CsrfToken> csrfTokens = new Dictionary<string, CsrfToken>();

    /// <summary>
    /// Remove all expired CSRF tokens.
    /// </summary>
    public void RemoveExpiredCsrfTokens()
    {
        // iter over all csrf tokens and remove the expired ones
        foreach (KeyValuePair<string, CsrfToken> csrfToken in csrfTokens)
        {
            if (DateTime.Compare(DateTime.UtcNow, csrfToken.Value.Expiration) < 0)
            {
                csrfTokens.Remove(csrfToken.Key);
            }
        }
    }

    /// <summary>
    /// Validate a CSRF token and remove it if it is expired.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>True if the token is valid, false otherwise.</returns>
    public bool ValidateCsrfToken(string token)
    {
        CsrfToken? csfrToken;
        if (!csrfTokens.TryGetValue(token, out csfrToken))
        {
            return false;
        }

        // validate expiration
        if (DateTime.Compare(DateTime.UtcNow, csfrToken.Expiration) < 0)
        {
            return true;
        }
        else
        {
            csrfTokens.Remove(token);

            return false;
        }
    }

    /// <summary>
    /// Get a new CSRF token.
    /// </summary>
    /// <returns>The new CSRF token.</returns>
    public CsrfToken GetNewCsrfToken()
    {
        CsrfToken csfrToken;

        while (true)
        {
            try
            {
                csfrToken = new CsrfToken();
                csrfTokens.Add(csfrToken.Token, csfrToken);
                break;
            }
            catch (System.ArgumentException)
            {
                continue;
            }
        }

        return csfrToken;
    }
}