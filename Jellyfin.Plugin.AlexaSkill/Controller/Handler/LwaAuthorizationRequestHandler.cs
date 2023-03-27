using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.AlexaSkill.Controller.Handler;

/// <summary>
/// Class to handle LWA authorization requests.
/// </summary>
public class LwaAuthorizationRequestHandler
{
    private Dictionary<string, LwaAuthorizationRequest> lwaAuthorizationRequests = new Dictionary<string, LwaAuthorizationRequest>();

    /// <summary>
    /// Remove the specified LWA authorization request.
    /// </summary>
    /// <param name="pageToken">The page token of the LWA request.</param>
    public void RemoveLwaAuthorizeRequest(string pageToken)
    {
        lwaAuthorizationRequests.Remove(pageToken);
    }

    /// <summary>
    /// Validate the page token of a LWA authorization request.
    /// </summary>
    /// <param name="pageToken">The page token of the LWA request.</param>
    /// <returns>True if the token is valid, false otherwise.</returns>
    public bool ValidatLwaAuthorizePageToken(string pageToken)
    {
        LwaAuthorizationRequest? lwaAuthorizationRequest;
        if (!lwaAuthorizationRequests.TryGetValue(pageToken, out lwaAuthorizationRequest))
        {
            return false;
        }

        // validate expiration
        if (DateTime.Compare(DateTime.UtcNow, lwaAuthorizationRequest.Expiration) < 0)
        {
            return true;
        }
        else
        {
            lwaAuthorizationRequests.Remove(pageToken);

            return false;
        }
    }

    /// <summary>
    /// Get the LWA authorization request for the specified page token.
    /// </summary>
    /// <param name="pageToken">The page token of the LWA request.</param>
    /// <returns>The LWA authorization request.</returns>
    public LwaAuthorizationRequest? GetLwaAuthorizationRequest(string pageToken)
    {
        LwaAuthorizationRequest? lwaAuthorizationRequest;
        if (lwaAuthorizationRequests.TryGetValue(pageToken, out lwaAuthorizationRequest))
        {
            return lwaAuthorizationRequest;
        }

        return null;
    }

    /// <summary>
    /// Get a new LWA authorization request.
    /// </summary>
    /// <param name="pageToken">The page token of the LWA request.</param>
    /// <returns>The new LWA authorization request.</returns>
    public string GetNewLwaAuthorizationRequest(Guid pageToken)
    {
        LwaAuthorizationRequest lwaAuthorizationRequest;

        while (true)
        {
            try
            {
                lwaAuthorizationRequest = new LwaAuthorizationRequest(pageToken);
                lwaAuthorizationRequests.Add(lwaAuthorizationRequest.PageToken, lwaAuthorizationRequest);
                break;
            }
            catch (System.ArgumentException)
            {
                continue;
            }
        }

        return lwaAuthorizationRequest.PageToken;
    }
}