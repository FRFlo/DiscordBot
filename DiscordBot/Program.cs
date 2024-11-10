using DiscordBot.Commands;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
/// Main entry point for the bot.
/// </summary>
internal class Program : IDesignTimeDbContextFactory<PostgreSqlContext>
{
	/// <summary>
	/// Main entry point for the bot.
	/// </summary>
	public static async Task Main()
	{
		string? discordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
		if (string.IsNullOrWhiteSpace(discordToken))
		{
			Console.WriteLine("Error: No discord token found. Please provide a token via the DISCORD_TOKEN environment variable.");
			Environment.Exit(1);
		}

		string? databaseConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
		if (string.IsNullOrWhiteSpace(databaseConnectionString))
		{
			Console.WriteLine("Error: No database connection string found. Please provide a connection string via the DATABASE_CONNECTION_STRING environment variable.");
			Environment.Exit(1);
		}

		ServiceCollection services = new();

		services.AddDbContext<PostgreSqlContext>(options => { options.UseNpgsql(databaseConnectionString, o => o.UseNodaTime()); });

		try
		{
			await using ServiceProvider serviceProvider = services.BuildServiceProvider();
			using IServiceScope scope = serviceProvider.CreateScope();
			await scope.ServiceProvider.GetRequiredService<PostgreSqlContext>().Database.MigrateAsync();
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error: Unable to migrate database: {e.Message}");
			if (e.InnerException is not null)
			{
				Console.WriteLine($"Inner exception: {e.InnerException.Message}");
			}

			Environment.Exit(1);
		}

		DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(discordToken, SlashCommandProcessor.RequiredIntents | DiscordIntents.GuildVoiceStates, services);

		builder.UseCommands((_, extension) => { extension.AddCommands([typeof(PingCommand)]); }, new CommandsConfiguration
		{
			RegisterDefaultCommandProcessors = true,
			UseDefaultCommandErrorHandler = true
		});

		DiscordClient client = builder.Build();

		await client.ConnectAsync();
		await Task.Delay(-1);
	}

	/// <summary>
	/// Creates a new instance of the <see cref="PostgreSqlContext"/> class.
	/// </summary>
	/// <param name="args">Arguments passed to the program.</param>
	/// <returns>A new instance of the <see cref="PostgreSqlContext"/> class.</returns>
	public PostgreSqlContext CreateDbContext(string[] args)
	{
		string? databaseConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
		if (string.IsNullOrWhiteSpace(databaseConnectionString))
		{
			Console.WriteLine("Error: No database connection string found. Please provide a connection string via the DATABASE_CONNECTION_STRING environment variable.");
			Environment.Exit(1);
		}

		DbContextOptionsBuilder<PostgreSqlContext> optionsBuilder = new();
		optionsBuilder.UseNpgsql(databaseConnectionString, o => o.UseNodaTime());

		return new PostgreSqlContext(optionsBuilder.Options);
	}
}