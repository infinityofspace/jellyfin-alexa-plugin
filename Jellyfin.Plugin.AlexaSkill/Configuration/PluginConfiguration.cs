using System;
using System.Collections.Generic;
using Alexa.NET.Management;
using Jellyfin.Plugin.AlexaSkill.Alexa.Manifest;
using Jellyfin.Plugin.AlexaSkill.Entities;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AlexaSkill.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    private SslCertificateType sslCertType;
    private string serverAddress;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        // set default options here
        sslCertType = SslCertificateType.Wildcard;
        LwaClientId = string.Empty;
        LwaClientSecret = string.Empty;

        serverAddress = string.Empty;
        AccountLinkingClientId = Guid.NewGuid().ToString();

        Users = new List<User>();
    }

    /// <summary>
    /// Gets or sets the ssl cert type of the public jellyfin endpoint.
    /// </summary>
    public SslCertificateType SslCertType
    {
        get => sslCertType;
        set
        {
            sslCertType = value;

            if (Plugin.Instance!.ManifestSkill == null)
            {
                Plugin.Instance.ManifestSkill = new ManifestSkill("Jellyfin.Plugin.AlexaSkill.Alexa.Manifest.manifest.json", ServerAddress, value);
            }
            else
            {
                Plugin.Instance.ManifestSkill.SetApiEndpoint(ServerAddress, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the server address.
    /// </summary>
    public string ServerAddress
    {
        get => serverAddress;
        set
        {
            serverAddress = value;

            if (Plugin.Instance!.ManifestSkill == null)
            {
                Plugin.Instance.ManifestSkill = new ManifestSkill("Jellyfin.Plugin.AlexaSkill.Alexa.Manifest.manifest.json", value, SslCertType);
            }
            else
            {
                Plugin.Instance.ManifestSkill.SetApiEndpoint(value, SslCertType);
            }
        }
    }

    /// <summary>
    /// Gets or sets the client id for LWA.
    /// </summary>
    public string LwaClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret for LWA.
    /// </summary>
    public string LwaClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the account linking client id.
    /// </summary>
    public string AccountLinkingClientId { get; set; }

    /// <summary>
    /// Gets or sets the list of users.
    /// </summary>
    public List<User> Users { get; set; }

    /// <summary>
    /// Add a user to the list of users.
    /// </summary>
    /// <param name="user">The user to add.</param>
    public void AddUser(User user)
    {
        // check if the user is already inside the list
        foreach (User u in Users)
        {
            if (user.Id == u.Id)
            {
                throw new ArgumentException("User already inside list");
            }
        }

        Users.Add(user);
    }

    /// <summary>
    /// Get the user by its guid.
    /// </summary>
    /// <param name="guid">The guid of the user.</param>
    /// <returns>Instance of the <see cref="User"/> class or null if the user was not found.</returns>
    public User? GetUserById(Guid guid)
    {
        foreach (User u in Users)
        {
            if (guid == u.Id)
            {
                return u;
            }
        }

        return null;
    }

    /// <summary>
    /// Delete the user with the given guid.
    /// </summary>
    /// <param name="guid">The guid of the user.</param>
    /// <returns>True if the user was deleted, false otherwise.</returns>
    public bool DeleteUser(Guid guid)
    {
        foreach (User u in Users)
        {
            if (guid == u.Id)
            {
                return Users.Remove(u);
            }
        }

        return false;
    }
}