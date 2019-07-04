using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Commands;
using Smod2.Config;
using Smod2.EventHandlers;
using Smod2.Events;

namespace NoUFriendlyFire
{
	[PluginDetails(
		author = "Phoenix",
		name = "NoUFriendlyFire",
		id = "phoenix.noufriendlyfire",
		description = "For servers with friendly fire enabled, it will damage the attacker rather than the team member damaged by FF",
		configPrefix = "nuff",
		version = "1.0",
		SmodMajor = 3,
		SmodMinor = 5,
		SmodRevision = 0)]

	public class NoUFriendlyFire : Plugin
	{
		[ConfigOption]
		public readonly bool Disable = false;
		[ConfigOption]
		public readonly bool consoleLogging = false;
		[ConfigOption]
		public readonly string[] reverseRanks = { "owner", "admin" };
		[ConfigOption]
		public bool reverse = false;


		public override void OnDisable()
		{
			this.Info(this.Details.name + " disabled");
		}
		public override void OnEnable()
		{
			this.Info(this.Details.name + " enabled");
		}
		public override void Register()
		{
			this.AddEventHandlers(new PlayerHurtHandler(this));
			this.AddCommand("reverse", new Reverse(this));
		}
	}

	internal class Reverse : ICommandHandler
	{
		private NoUFriendlyFire plugin;
		private string State;

		public Reverse(NoUFriendlyFire plugin) => this.plugin = plugin;

		public string GetCommandDescription()
		{
			return "Reverses all friendly fire damage";
		}

		public string GetUsage()
		{
			return "reverse";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			bool valid = sender is Server;
			Player admin = null;
			if (!valid)
			{
				admin = sender as Player;
				if (admin != null)
				{
					valid = plugin.reverseRanks.Contains(admin.GetRankName());
				}
			}

			if (valid)
			{
				plugin.reverse = !plugin.reverse;
				if (plugin.reverse) State = "enabled";
				else if (!plugin.reverse) State = "disabled";
				return new[] { $"Friendly fire damage reversal has been {State}!" };
			}
			else if (!valid)
			{
				return new[] { "You are not whitelisted to use this command" };
			}

			return new[] { GetUsage() };
		}
	}

	internal class PlayerHurtHandler : IEventHandlerPlayerHurt
	{
		private NoUFriendlyFire plugin;

		public PlayerHurtHandler(NoUFriendlyFire plugin) => this.plugin = plugin;

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (plugin.Disable || !plugin.reverse || ev.Attacker.Name == "Server" || ev.Player.SteamId == ev.Attacker.SteamId || ev.Attacker.TeamRole.Team != ev.Player.TeamRole.Team) return;

			float OriginalDamage = ev.Damage;
			ev.Damage = 0;

			ev.Attacker.Damage((int)OriginalDamage, ev.DamageType);
			if (plugin.consoleLogging) plugin.Info($"Dealing {OriginalDamage.ToString()} to {ev.Attacker.Name} for dealing friendly damage");
		}
	}
}
