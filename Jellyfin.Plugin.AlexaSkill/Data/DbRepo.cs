using System;
using Jellyfin.Plugin.AlexaSkill.Entities;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AlexaSkill.Data;

/// <summary>
/// Database for persistent skill data.
/// </summary>
public class DbRepo : IDisposable
{
    private readonly LiteDatabase _liteDb;
    private readonly ILogger _logger;

    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbRepo"/> class.
    /// </summary>
    /// <param name="dbFilePath">Path to the LiteDB file.</param>
    /// <param name="loggerFactory">Logger factory instance.</param>
    public DbRepo(string dbFilePath, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<DbRepo>();

        _liteDb = new LiteDatabase($"filename={dbFilePath}");
    }

    /// <summary>
    /// Creates a new user in the database.
    /// </summary>
    /// <param name="userId">The user id of the user to create.</param>
    /// <param name="token">The auth token to interact with the Jellyfin server api.</param>
    /// <returns>Instance of the <see cref="User"/> class.</returns>
    public User CreateUser(string userId, string token)
    {
        _logger.LogDebug("Creating user with id {0}", userId);

        var user = new User
        {
            Id = userId,
            Token = token
        };
        _liteDb.GetCollection<User>("users").Insert(user);

        return user;
    }

    /// <summary>
    /// Get a user from the database.
    /// </summary>
    /// <param name="userId">Id of the user to get.</param>
    /// <returns>The <see cref="User"/> class instance or null if not found.</returns>
    public User? GetUser(string userId)
    {
        _logger.LogDebug("Get user with id {0}", userId);

        return _liteDb.GetCollection<User>("users").FindOne(x => x.Id == userId);
    }

    /// <summary>
    /// Get a user by its skill auth token.
    /// </summary>
    /// <param name="accessToken">Skill auth token.</param>
    /// <returns>The <see cref="User"/> class instance or null if not found.</returns>
    public User? GetUserByToken(string accessToken)
    {
        _logger.LogDebug("Get user with access token {0}", accessToken);

        return _liteDb.GetCollection<User>("users").FindOne(x => x.Token == accessToken);
    }

    /// <summary>
    /// Delete a user by its id.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    public void DeleteUser(string userId)
    {
        User user = _liteDb.GetCollection<User>("users").FindOne(x => x.Id == userId);
        _liteDb.GetCollection<User>("users").Delete(user.Id);
    }

    /// <summary>
    /// Delete the user collection.
    /// </summary>
    public void DeleteAllUsers()
    {
        _liteDb.DropCollection("users");
    }

    /// <summary>
    /// Delete all collections.
    /// </summary>
    public void DeleteDatabase()
    {
        this.DeleteAllUsers();
    }

    /// <summary>
    /// Dispose of the database.
    /// </summary>
    /// <param name="disposing">True if disposing, false if finalizing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            _liteDb?.Dispose();

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}