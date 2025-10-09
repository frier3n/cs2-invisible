using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Text.Json;

namespace cssinvisible
{
    public class InvisibleCommand : BasePlugin
    {
        public override string ModuleName => "Toggle Invisibility";
        public override string ModuleVersion => "1.0.3";
        public override string ModuleAuthor => "r991";
        public override string ModuleDescription => "Inspired by Dima Invisible Players Videos";

        private static readonly ConcurrentDictionary<ulong, bool> invisiblePlayers = new();

        // Config and lang
        private static PluginConfig Config = new();
        private static PluginLang Lang = new();

        private string ConfigDir => Path.Combine(ModuleDirectory, "config");
        private string ConfigFile => Path.Combine(ConfigDir, "config.json");
        private string LangFile => Path.Combine(ConfigDir, "lang.json");

        public override void Load(bool hotReload)
        {
            Directory.CreateDirectory(ConfigDir);
            LoadConfig();
            LoadLang();
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    Config = JsonSerializer.Deserialize<PluginConfig>(File.ReadAllText(ConfigFile)) ?? new PluginConfig();
                }
                else
                {
                    File.WriteAllText(ConfigFile, JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
            catch
            {
                Logger.LogError("[cssInvisible] Error loading config.json — using default.");
                Config = new PluginConfig();
            }
        }

        private void LoadLang()
        {
            try
            {
                if (File.Exists(LangFile))
                {
                    Lang = JsonSerializer.Deserialize<PluginLang>(File.ReadAllText(LangFile)) ?? new PluginLang();
                }
                else
                {
                    File.WriteAllText(LangFile, JsonSerializer.Serialize(Lang, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
            catch
            {
                Logger.LogError("[cssInvisible] Error loading lang.json — using default.");
                Lang = new PluginLang();
            }
        }

        // COMMAND !in

        [ConsoleCommand("css_in", "Makes players invisible (admins use only)")]
        [CommandHelper(minArgs: 1, usage: "!in <playername|@me|@ct|@t>", whoCanExecute: CommandUsage.CLIENT_ONLY)]
        public void OnInvisibleCommand(CCSPlayerController? caller, CommandInfo info)
        {
            if (caller == null || !caller.IsValid)
                return;

            if (!AdminManager.PlayerHasPermissions(caller, Config.RequiredFlag))
            {
                caller.PrintToChat(Prefix($" {ChatColors.Red}{Lang.NoPermission}"));
                return;
            }

            string arg = info.GetArg(1).ToLower();

            if (arg == "@me")
            {
                ToggleInvisibility(caller);
                caller.PrintToChat(Prefix($" {ChatColors.Orange}{Lang.YouToggled}"));
                return;
            }

            if (arg == "@ct" || arg == "@t")
            {
                byte team = (byte)(arg == "@ct" ? 3 : 2);
                var players = Utilities.GetPlayers().Where(p => p.IsValid && p.TeamNum == team);

                foreach (var player in players)
                    ToggleInvisibility(player);

                caller.PrintToChat(Prefix($" {ChatColors.White}{Lang.TeamToggled.Replace("{team}", arg.ToUpper())}"));
                return;
            }

            var target = Utilities.GetPlayers()
                .FirstOrDefault(p => p.IsValid && p.PlayerName.ToLower().Contains(arg));

            if (target == null)
            {
                caller.PrintToChat(Prefix($" {ChatColors.Red}{Lang.PlayerNotFound}"));
                return;
            }

            ToggleInvisibility(target);
            caller.PrintToChat(Prefix($" {ChatColors.Green}{Lang.TargetToggled.Replace("{player}", target.PlayerName)}"));
        }

        // INVISIBILITY

        private static void ToggleInvisibility(CCSPlayerController player)
        {
            if (!player.IsValid || player.PlayerPawn.Value == null)
                return;

            bool isInvisible = invisiblePlayers.ContainsKey(player.SteamID);

            if (isInvisible)
            {
                SetVisibility(player, true);
                invisiblePlayers.TryRemove(player.SteamID, out _);
                player.PrintToChat(Prefix($" {ChatColors.Yellow}{Lang.NowVisible}"));
            }
            else
            {
                SetVisibility(player, false);
                invisiblePlayers[player.SteamID] = true;
                player.PrintToChat(Prefix($" {ChatColors.Green}{Lang.NowInvisible}"));
            }
        }

        private static void SetVisibility(CCSPlayerController player, bool visible)
        {
            var pawn = player.PlayerPawn.Value;
            if (pawn == null) return;

            var color = visible
                ? Color.FromArgb(255, 255, 255, 255)
                : Color.FromArgb(0, 255, 255, 255);

            pawn.Render = color;
            pawn.ShadowStrength = visible ? 1.0f : 0.0f;
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

            if (pawn.WeaponServices != null)
            {
                foreach (var handle in pawn.WeaponServices.MyWeapons)
                {
                    if (handle?.Value == null || !handle.Value.IsValid)
                        continue;

                    var weapon = handle.Value;
                    weapon.Render = color;
                    weapon.ShadowStrength = visible ? 1.0f : 0.0f;
                    Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");
                }
            }
        }


        [GameEventHandler]
        public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid)
                return HookResult.Continue;

            bool shouldBeInvisible = invisiblePlayers.ContainsKey(player.SteamID);

            Server.NextFrame(() =>
            {
                if (player.IsValid && shouldBeInvisible)
                    SetVisibility(player, false);
            });

            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            if (!Config.Persistent)
                return HookResult.Continue;

            foreach (var player in Utilities.GetPlayers())
            {
                if (player.IsValid && invisiblePlayers.ContainsKey(player.SteamID))
                    SetVisibility(player, false);
            }

            return HookResult.Continue;
        }

        // PREFIX

        private static string Prefix(string message)
        {
            return $" {ChatColors.Blue}{Config.ChatPrefix} {message}";
        }
    }

    // CONFIG / LANG

    public class PluginConfig
    {
        public bool Persistent { get; set; } = true;
        public string RequiredFlag { get; set; } = "@css/root";
        public string ChatPrefix { get; set; } = "[Invisible]";
    }

    public class PluginLang
    {
        public string NoPermission { get; set; } = "Você não tem permissão para usar este comando.";
        public string YouToggled { get; set; } = "Sua invisibilidade foi alternada!";
        public string TeamToggled { get; set; } = "Jogadores {team} tiveram sua visibilidade alternada!";
        public string PlayerNotFound { get; set; } = "Jogador não encontrado.";
        public string TargetToggled { get; set; } = "{player} teve sua visibilidade alternada.";
        public string NowInvisible { get; set; } = "Você agora está invisível!";
        public string NowVisible { get; set; } = "Você agora está visível novamente!";
    }
}