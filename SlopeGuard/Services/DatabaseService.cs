using SQLite;
using SlopeGuard.Models;

namespace SlopeGuard.Services;

public static class DatabaseService
{
    static SQLiteAsyncConnection? _db;

    public static async Task InitAsync()
    {
        if (_db is not null)
            return;

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "sessions.db");
        _db = new SQLiteAsyncConnection(dbPath);
        await _db.CreateTableAsync<SkiSession>();
    }

    public static async Task<List<SkiSession>> GetSessionsAsync()
    {
        if (_db is null)
            throw new InvalidOperationException("Database not initialized. Call InitAsync() first.");

        return await _db.Table<SkiSession>().OrderByDescending(s => s.Date).ToListAsync();
    }

    public static async Task InsertSessionAsync(SkiSession session)
    {
        if (_db is null)
            throw new InvalidOperationException("Database not initialized. Call InitAsync() first.");

        await _db.InsertAsync(session);
    }

    public static async Task DeleteSessionAsync(int id)
    {
        if (_db is null)
            throw new InvalidOperationException("Database not initialized.");

        await _db.DeleteAsync<SkiSession>(id);
    }

}
