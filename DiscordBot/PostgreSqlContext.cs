using Microsoft.EntityFrameworkCore;

namespace DiscordBot;

/// <summary>
/// The context for the PostgreSQL database.
/// </summary>
/// <param name="options">The options for the context.</param>
public class PostgreSqlContext(DbContextOptions<PostgreSqlContext> options) : DbContext(options);