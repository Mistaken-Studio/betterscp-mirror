// -----------------------------------------------------------------------
// <copyright file="SwapSCPCommand.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Mistaken.API;
using Mistaken.API.Commands;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;

namespace Mistaken.BetterSCP
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class SwapSCPCommand : IBetterCommand
    {
        public override string Description => "Pozwala zmienić SCP";

        public override string Command => "swapscp";

        public override string[] Aliases => new string[] { "swap" };

        public string GetUsage()
        {
            return "swapscp [SCP]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();

            if (!player.GetSessionVariable<bool>("SWAPSCP_OVERRIDE"))
                return new string[] { "<color=red>Z przyczyn technicznych komenda jest obecnie wyłączona i mają do niej dostęp tylko Vipy</color>", "Kiedy problem zostanie rozwiązany komenda znowu będzie działać jak dawiej", "<color=grey>Jak kogoś interesuje to problem jest w tym że normalnie jest cooldown raz na 3 rundy jako SCP ale poniważ serwer restartuje się co rundę to tego cooldownu nie ma, a Vipy normalnie nie mają tego cooldownu więc dla nich komenda może dalej działać</color>" };

            if (args.Length == 0)
                return new string[] { this.GetUsage() };
            if (player.Team != Team.SCP)
                return new string[] { "Nie możesz zmienić SCP nie będąc SCP" };
            if (player.Role == RoleType.Scp0492)
                return new string[] { "Nie możesz zmienić SCP jako SCP 049-2" };
            if (this.roleRequests.Any(i => i.Value.Key == player.Id))
            {
                var data = this.roleRequests.First(i => i.Value.Key == player.Id);
                var requester = RealPlayers.Get(data.Key);
                if (args[0].ToLower() == "yes")
                {
                    player.Role = requester.Role;
                    requester.Role = data.Value.Value;
                    AlreadyChanged.Add(requester.Id);
                    if (!requester.GetSessionVariable<bool>("SWAPSCP_OVERRIDE"))
                        SwapCooldown.Add(requester.UserId, RoundsCooldown);
                    this.roleRequests.Remove(data);
                    return new string[] { "Ok" };
                }
                else if (args[0].ToLower() == "no")
                {
                    requester.Broadcast("Swap SCP", 5, $"{player.Nickname} nie chce zamienić się SCP");
                    this.roleRequests.Remove(data);
                    return new string[] { "Ok" };
                }
                else
                    return new string[] { ".scpswap yes/no", $"'yes' aby zmienić SCP na {requester.Role}", $"'no' aby zostać jako {player.Role}" };
            }
            else if (Round.ElapsedTime.TotalSeconds > 30)
                return new string[] { "Za późno, możesz zmienić SCP tylko przez pierwsze 30 sekund rundy" };

            if (this.roleRequests.Any(i => i.Key == player.Id))
                return new string[] { "Już wysłałeś prośbę aby zamienić SCP" };

            if (AlreadyChanged.Contains(player.Id))
                return new string[] { "Możesz zmienić SCP tylko raz na rundę" };

            if (SwapCooldown.ContainsKey(player.UserId))
                return new string[] { $"Możesz użyć tej komendy tylko raz na {RoundsCooldown} rundy rozegrane jako SCP" };

            var scp = args[0];
            scp = scp.ToLower().Replace("scp", string.Empty).Replace("-", string.Empty);
            var role = RoleType.Scp0492;

            switch (scp)
            {
                case "173":
                    role = RoleType.Scp173;
                    break;
                case "106":
                    role = RoleType.Scp106;
                    break;
                case "93953":
                case "939":
                    role = RoleType.Scp93953;
                    break;
                case "93989":
                    role = RoleType.Scp93989;
                    break;
                case "049":
                    role = RoleType.Scp049;
                    break;
                case "079":
                    if (RealPlayers.List.Any(p => p.Team == Team.SCP && p.Id != player.Id))
                        role = RoleType.Scp079;
                    else
                        return new string[] { "Jesteś jedynym SCP, nie możesz się zamienić w SCP 079" };
                    break;
                case "096":
                    role = RoleType.Scp096;
                    break;
                default:
                    return new string[] { "Nieznany SCP", this.GetUsage() };
            }

            if (player.Role == role)
            {
                success = false;
                return new string[] { "Już jesteś tym SCP" };
            }

            if (RealPlayers.List.Any(p => p.Role == role))
            {
                var target = RealPlayers.List.First(p => p.Role == role);
                var data = new KeyValuePair<int, KeyValuePair<int, RoleType>>(player.Id, new KeyValuePair<int, RoleType>(target.Id, role));
                this.roleRequests.Add(data);
                target.Broadcast("Swap SCP", 15, $"<size=50%>{player.Nickname} chce się z tobą zamienić SCP, jeżeli się zgodzisz to zostaniesz <b>{player.Role}</b>\nWpisz \".swapscp yes\" lub \".swapscp no\" w konsoli(~) aby się zamienić lub aby tego nie robić</size>");
                Module.CallSafeDelayed(
                    15,
                    () =>
                    {
                        if (this.roleRequests.Contains(data))
                        {
                            player.Broadcast("Swap SCP", 5, "Czas minął");
                            this.roleRequests.Remove(data);
                        }
                    },
                    "SawpSCP");
                return new string[] { "Prośba zamiany wysłana" };
            }
            else
            {
                AlreadyChanged.Add(player.Id);
                if (!player.GetSessionVariable<bool>("SWAPSCP_OVERRIDE"))
                    SwapCooldown.Add(player.UserId, RoundsCooldown);
                player.Role = role;
                return new string[] { "Done" };
            }
        }

        internal static readonly Dictionary<string, uint> SwapCooldown = new Dictionary<string, uint>();
        internal static readonly List<int> AlreadyChanged = new List<int>();

        private const int RoundsCooldown = 3;

        private readonly List<KeyValuePair<int, KeyValuePair<int, RoleType>>> roleRequests = new List<KeyValuePair<int, KeyValuePair<int, RoleType>>>();
    }
}
