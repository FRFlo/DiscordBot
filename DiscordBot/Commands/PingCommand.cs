using System.ComponentModel;
using System.Diagnostics;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
// ReSharper disable UnusedMember.Global

namespace DiscordBot.Commands;

/// <summary>
/// A command group for showing the latency of the bot.
/// </summary>
/// <param name="postgreSqlContext"></param>
[Command("ping")]
[Description("Shows the latency of the bot.")]
public class PingCommand(PostgreSqlContext postgreSqlContext)
{
	/// <summary>
	/// Shows the latency of the bot to the discord gateway.
	/// </summary>
	/// <param name="context">The context of the command.</param>
	[Command("gateway")]
	[Description("Shows the latency of the bot to the discord gateway.")]
	public static async ValueTask GatewayExecuteAsync(SlashCommandContext context)
	{
		await context.DeferResponseAsync(true);
		await context.RespondAsync($"La latence du WebSocket est de {context.Client.GetConnectionLatency(context.Guild?.Id ?? 0).Milliseconds}ms.");
	}

	/// <summary>
	/// Shows the latency of the bot to the database.
	/// </summary>
	/// <param name="context">The context of the command.</param>
	[Command("database")]
	[Description("Shows the latency to the database.")]
	public async ValueTask PostgresExecuteAsync(SlashCommandContext context)
	{
		await context.DeferResponseAsync(true);

		Stopwatch stopwatch = Stopwatch.StartNew();
		await postgreSqlContext.Database.CanConnectAsync();
		stopwatch.Stop();

		await context.RespondAsync($"La latence de la base de données est de {stopwatch.ElapsedMilliseconds}ms.");
	}
}