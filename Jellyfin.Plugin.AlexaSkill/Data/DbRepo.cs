using System;
using System.Collections.Generic;
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
    /// <param name="userSkill">The user skill linked to the jellyfin account.</param>
    /// <returns>Instance of the <see cref="User"/> class.</returns>
    public User CreateUser(Guid userId, string token, UserSkill userSkill)
    {
        _logger.LogDebug("Creating user with id {0}", userId);

        var user = new User
        {
            Id = userId,
            JellyfinToken = token,
            UserSkill = userSkill
        };
        _liteDb.GetCollection<User>("users").Insert(user);

        return user;
    }

    /// <summary>
    /// Update a user in the database.
    /// </summary>
    /// <param name="user">The user to update.</param>
    public void UpdateUser(User user)
    {
        _logger.LogDebug("Updating user with id {0}", user.Id);

        _liteDb.GetCollection<User>("users").Update(user);
    }

    /// <summary>
    /// Get all user from the database.
    /// </summary>
    /// <returns>Enumerable of all <see cref="User"/> class instances.</returns>
    public IEnumerable<User> GetAllUser()
    {
        _logger.LogDebug("Get all user");

        return _liteDb.GetCollection<User>("users").FindAll();
    }

    /// <summary>
    /// Get a user from the database.
    /// </summary>
    /// <param name="userId">Id of the user to get.</param>
    /// <returns>The <see cref="User"/> class instance or null if not found.</returns>
    public User? GetUser(Guid userId)
    {
        _logger.LogDebug("Get user with id {0}", userId);

        return _liteDb.GetCollection<User>("users").FindOne(x => x.Id == userId);
    }

    /// <summary>
    /// Get a user by its skill auth token.
    /// </summary>
    /// <param name="token">Jellyfin API auth token.</param>
    /// <returns>The <see cref="User"/> class instance or null if not found.</returns>
    public User? GetUserByToken(string token)
    {
        _logger.LogDebug("Get user with access token {0}", token);

        return _liteDb.GetCollection<User>("users").FindOne(x => x.JellyfinToken == token);
    }

    /// <summary>
    /// Get a user by its skill auth token.
    /// </summary>
    /// <param name="skillId">Alexa Skill id.</param>
    /// <returns>The <see cref="User"/> class instance or null if not found.</returns>
    public User? GetUserBySkilId(string skillId)
    {
        _logger.LogDebug("Get user with skill id {0}", skillId);

        return _liteDb.GetCollection<User>("users").FindOne(x => x.UserSkill.SkillId == skillId);
    }

    /// <summary>
    /// Delete a user by its id.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    public void DeleteUser(Guid userId)
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