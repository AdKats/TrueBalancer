using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

using PRoCon.Core;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;

namespace PRoConEvents
{
    public partial class TrueBalancer
    {
        #region Guard

        public void DebugInfoGuard(string DebugMessage)
        {
            if (ynbDebugModeGuard == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n " + DebugMessage);
            }
        }

        public List<string> GetSoldierNames()
        {
            List<string> soldierNames = new List<string>();

            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                soldierNames.Add(kvp.Key);
            }

            return soldierNames;
        }

        public void OnCommandTBForceMove(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            if (capCommand.MatchedArguments.Count == 0)
            {
                this.DebugInfoGuard("^4Malformed tb-fmove command: " + strText);
                if (this.boolVirtual)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSpeaker + " - " + "Malformed command: " + strText);
                }
                else
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Malformed command: " + strText, "player", strSpeaker);
                }
                return;
            }

            string player = capCommand.MatchedArguments[0].Argument;
            this.DebugInfoGuard("^4Force Moving Player: ^b" + player);

            this.DTForceMove = DateTime.Now;

            if (this.OnCommandMove.ContainsKey(player) == false)
            {
                this.OnCommandMove.Add(player, true);
            }
            else
            {
                this.OnCommandMove[player] = true;
            }

            int team = 0;

            if (this.dicPlayerCache[player].teamID == 1)
                team = 2;
            else
                team = 1;

            if (this.boolVirtual)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + player + " " + team.ToString() + " 0 false");
            }
            else
            {
                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", player, team.ToString(), "0", "true");
            }
            this.DebugInfoGuard("^4Trying to force player to the other side, due to TB-ForceMoveCommand: ^b" + player);

        }

        public void OnCommandTBMove(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            if (capCommand.MatchedArguments.Count == 0)
            {
                this.DebugInfoGuard("^4Malformed tb-move command: " + strText);
                if (this.boolVirtual)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSpeaker + " - " + "Malformed command: " + strText);
                }
                else
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Malformed command: " + strText, "player", strSpeaker);
                }
                return;
            }
            string player = capCommand.MatchedArguments[0].Argument;
            this.DebugInfoGuard("^4Move Player when dead: ^b" + player);
            if (this.boolVirtual)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + player + " - " + "You will be moved to the other team upon death.");
            }
            else
            {
                this.ExecuteCommand("procon.protected.send", "admin.say", "You will be moved to the other team upon death.", "player", player);
            }
            if (this.OnCommandMoveDone.Contains(player))
            {
                this.OnCommandMoveDone.Remove(player);
            }

            if (this.OnCommandMove.ContainsKey(player) == false)
            {
                this.OnCommandMove.Add(player, false);
            }
            else
            {
                this.OnCommandMove[player] = false;
            }
        }



        #endregion
    }
}
