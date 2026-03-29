using System;
using System.Collections.Generic;

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

        public void DebugInfoGuard(String DebugMessage)
        {
            if (ynbDebugModeGuard == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n " + DebugMessage);
            }
        }

        public List<String> GetSoldierNames()
        {
            List<String> soldierNames = new List<String>();

            foreach (KeyValuePair<String, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                soldierNames.Add(kvp.Key);
            }

            return soldierNames;
        }

        public void OnCommandTBForceMove(String strSpeaker, String strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
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

            String player = capCommand.MatchedArguments[0].Argument;
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

            Int32 team = 0;

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

        public void OnCommandTBMove(String strSpeaker, String strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
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
            String player = capCommand.MatchedArguments[0].Argument;
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
