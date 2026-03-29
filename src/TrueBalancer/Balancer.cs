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
        #region TrueBalancer Functions

        public void DebugInfo(string DebugMessage)
        {
            if (ynbDebugMode == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n " + DebugMessage);
            }
        }

        public void CompareTeams()
        {
            //int WaitforOPSec = 0;

            //this.TSWaitforOP = DateTime.Now - this.DTLevelStart;
            //WaitforOPSec = this.TSWaitforOP.Hours * 3600 + this.TSWaitforOP.Minutes * 60 + this.TSWaitforOP.Seconds;
            //if (WaitforOPSec >= 10)
            //{
            //this.DebugInfo("WaitforOPSec: " + WaitforOPSec.ToString());

            this.TeamA = 0;
            this.TeamB = 0;

            this.rankA = 0;
            this.rankB = 0;
            this.skillA = 0;
            this.skillB = 0;
            this.spmA = 0;
            this.spmB = 0;
            this.spmcombatA = 0;
            this.spmcombatB = 0;
            this.kdrA = 0;
            this.kdrB = 0;
            this.TBvalueA = 0;
            this.TBvalueB = 0;

            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                if (!dicPlayerCache[kvp.Key].IsCommander && !dicPlayerCache[kvp.Key].IsSpectator)
                {
                    if (this.dicPlayerCache[kvp.Key].teamID == 1)
                    {
                        this.TeamA++;
                        this.rankA = this.rankA + dicPlayerCache[kvp.Key].rank;
                        this.skillA = this.skillA + dicPlayerCache[kvp.Key].skill;
                        this.spmA = this.spmA + dicPlayerCache[kvp.Key].spm;
                        this.spmcombatA = this.spmcombatA + dicPlayerCache[kvp.Key].spmcombat;
                        this.kdrA = this.kdrA + dicPlayerCache[kvp.Key].kdr;
                        this.TBvalueA = this.TBvalueA + dicPlayerCache[kvp.Key].TBvalue;
                    }
                    if (this.dicPlayerCache[kvp.Key].teamID == 2)
                    {
                        this.TeamB++;
                        this.rankB = this.rankB + dicPlayerCache[kvp.Key].rank;
                        this.skillB = this.skillB + dicPlayerCache[kvp.Key].skill;
                        this.spmB = this.spmB + dicPlayerCache[kvp.Key].spm;
                        this.spmcombatB = this.spmcombatB + dicPlayerCache[kvp.Key].spmcombat;
                        this.kdrB = this.kdrB + dicPlayerCache[kvp.Key].kdr;
                        this.TBvalueB = this.TBvalueB + dicPlayerCache[kvp.Key].TBvalue;
                    }
                }
            }

            this.rankA = Math.Round(this.rankA / this.TeamA, 2);
            this.rankB = Math.Round(this.rankB / this.TeamB, 2);

            this.skillA = Math.Round(this.skillA / this.TeamA, 2);
            this.skillB = Math.Round(this.skillB / this.TeamB, 2);

            this.spmA = Math.Round(this.spmA / this.TeamA, 2);
            this.spmB = Math.Round(this.spmB / this.TeamB, 2);

            this.spmcombatA = Math.Round(this.spmcombatA / this.TeamA, 2);
            this.spmcombatB = Math.Round(this.spmcombatB / this.TeamB, 2);

            this.kdrA = Math.Round(this.kdrA / this.TeamA, 2);
            this.kdrB = Math.Round(this.kdrB / this.TeamB, 2);

            this.TBvalueA = Math.Round(this.TBvalueA / this.TeamA, 1);
            this.TBvalueB = Math.Round(this.TBvalueB / this.TeamB, 1);

            this.DebugInfo("Team A: ^6Size: ^b" + this.TeamA + "^n,^7 TBValue: ^b" + this.TBvalueA.ToString("F1") + "^n,^0 Rank: ^b" + this.rankA.ToString("F2") + "^n,^1 Skill: ^b" + this.skillA.ToString("F2") + "^n,^2 SPM: ^b" + this.spmA.ToString("F2") + "^n,^3 SPMcombat: ^b" + this.spmcombatA.ToString("F2") + "^n,^4 KDR: ^b" + this.kdrA.ToString("F2"));
            this.DebugInfo("Team B: ^6Size: ^b" + this.TeamB + "^n,^7 TBValue: ^b" + this.TBvalueB.ToString("F1") + "^n,^0 Rank: ^b" + this.rankB.ToString("F2") + "^n,^1 Skill: ^b" + this.skillB.ToString("F2") + "^n,^2 SPM: ^b" + this.spmB.ToString("F2") + "^n,^3 SPMcombat: ^b" + this.spmcombatB.ToString("F2") + "^n,^4 KDR: ^b" + this.kdrB.ToString("F2"));

            if (this.TeamA > this.TeamB)
            {
                this.intToTeam = 2;
                this.intFromTeam = 1;
                this.intPlayerDif = this.TeamA - this.TeamB;

            }
            else if (this.TeamB > this.TeamA)
            {
                this.intToTeam = 1;
                this.intFromTeam = 2;
                this.intPlayerDif = this.TeamB - this.TeamA;
            }
            else
            {
                this.intPlayerDif = 0;
            }
            this.DebugInfo("^4PlayerDiff: ^b" + this.intPlayerDif);

            if (this.intPlayerDif < 2)
            {
                this.DebugInfo("^2Teams are balanced.");
                this.boolfirstwarningWL = false;
                this.boolneedbalance = false;
                this.booltimer = false;
                this.intcountWarnings = 0;
                this.boolbalanced = true;

                if (this.boolstartBalance == true)
                {
                    this.boolstartBalance = false;
                    this.boolafterBalance = true;
                }
                this.intToTeam = 0;
                this.intFromTeam = 0;
                this.strdeadplayer = "";
                this.boolwaitfordeath = false;
                this.boolwaitdead = false;
            }





            if (!this.boolscrambleActive)
            {

                if (this.intPlayerDif > this.intAllowDif)
                {
                    this.boolneedbalance = true;
                    this.DebugInfo("^4^bNeedBalance = true");
                    this.ExecuteCommand("procon.protected.send", "serverInfo");
                }


                if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Rush") || this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CaptureTheFlag0") || this.strcurrentGametype.Contains("Elimination") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                {
                    if (this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore)
                    {
                        //1. WARNUNG strWarning
                        if (this.intcountWarnings == 0 && this.boolneedbalance)
                        {
                            this.intcountWarnings = 1;
                            string strTEMP = this.strWarning.Replace("%Warning%", Convert.ToString(this.intcountWarnings));
                            strTEMP = strTEMP.Replace("%maxWarnings%", Convert.ToString(this.intWarnings));
                            if (ynbShowWarnings == enumBoolYesNo.Yes)
                            {
                                if (this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strTEMP);
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                                }
                            }

                            this.DebugInfo(strTEMP);
                        }

                        if (this.boolneedbalance && this.booltimer == false)
                        {
                            this.booltimer = true;

                            if (this.boolstartBalance)
                            {
                                this.DebugInfo("^4Balancing next player.");
                                BalancingTimer();
                            }
                            else
                            {
                                this.intTimerWait = this.intInterval;
                                this.DebugInfo("Starting Timer now");
                                this.ExecuteCommand("procon.protected.tasks.add", "WaitBalancingTimer", this.intTimerWait.ToString(), "1", "1", "procon.protected.plugins.call", "TrueBalancer", "BalancingTimer");
                            }
                        }
                    }
                    else
                    {
                        this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                    }
                }
                else if (this.strcurrentGametype.Contains("Domination"))
                {
                    if (this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore)
                    {
                        //1. WARNUNG strWarning
                        if (this.intcountWarnings == 0 && this.boolneedbalance)
                        {
                            this.intcountWarnings = 1;
                            string strTEMP = this.strWarning.Replace("%Warning%", Convert.ToString(this.intcountWarnings));
                            strTEMP = strTEMP.Replace("%maxWarnings%", Convert.ToString(this.intWarnings));
                            if (ynbShowWarnings == enumBoolYesNo.Yes)
                            {
                                if (this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strTEMP);
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                                }
                            }

                            this.DebugInfo(strTEMP);
                        }

                        if (this.boolneedbalance && this.booltimer == false)
                        {
                            this.booltimer = true;

                            if (this.boolstartBalance)
                            {
                                this.DebugInfo("^4Balancing next player.");
                                BalancingTimer();
                            }
                            else
                            {
                                this.intTimerWait = this.intInterval;
                                this.DebugInfo("Starting Timer now");
                                this.ExecuteCommand("procon.protected.tasks.add", "WaitBalancingTimer", this.intTimerWait.ToString(), "1", "1", "procon.protected.plugins.call", "TrueBalancer", "BalancingTimer");
                            }
                        }
                    }
                    else
                    {
                        this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                    }
                }
                else if (this.strcurrentGametype.Contains("Obliteration"))
                {
                    if (this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore)
                    {
                        //1. WARNUNG strWarning
                        if (this.intcountWarnings == 0 && this.boolneedbalance)
                        {
                            this.intcountWarnings = 1;
                            string strTEMP = this.strWarning.Replace("%Warning%", Convert.ToString(this.intcountWarnings));
                            strTEMP = strTEMP.Replace("%maxWarnings%", Convert.ToString(this.intWarnings));
                            if (ynbShowWarnings == enumBoolYesNo.Yes)
                            {
                                if (this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strTEMP);
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                                }
                            }

                            this.DebugInfo(strTEMP);
                        }

                        if (this.boolneedbalance && this.booltimer == false)
                        {
                            this.booltimer = true;

                            if (this.boolstartBalance)
                            {
                                this.DebugInfo("^4Balancing next player.");
                                BalancingTimer();
                            }
                            else
                            {
                                this.intTimerWait = this.intInterval;
                                this.DebugInfo("Starting Timer now");
                                this.ExecuteCommand("procon.protected.tasks.add", "WaitBalancingTimer", this.intTimerWait.ToString(), "1", "1", "procon.protected.plugins.call", "TrueBalancer", "BalancingTimer");
                            }
                        }
                    }
                    else
                    {
                        this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                    }
                }
                else if (this.strcurrentGametype.Contains("TeamDeathMatch"))
                {
                    if (this.intScoreTeamA < this.intminScore && this.intScoreTeamB < this.intminScore)
                    {
                        //1. WARNUNG strWarning
                        if (this.intcountWarnings == 0 && this.boolneedbalance)
                        {
                            this.intcountWarnings = 1;
                            string strTEMP = this.strWarning.Replace("%Warning%", Convert.ToString(this.intcountWarnings));
                            strTEMP = strTEMP.Replace("%maxWarnings%", Convert.ToString(this.intWarnings));
                            if (ynbShowWarnings == enumBoolYesNo.Yes)
                            {
                                if (this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strTEMP);
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                                }
                            }

                            this.DebugInfo(strTEMP);
                        }


                        if (this.boolneedbalance && this.booltimer == false)
                        {
                            this.booltimer = true;

                            if (this.boolstartBalance)
                            {
                                this.DebugInfo("^4Balancing next player.");
                                BalancingTimer();
                            }
                            else
                            {
                                this.intTimerWait = this.intInterval;
                                this.DebugInfo("Starting Timer now");
                                this.ExecuteCommand("procon.protected.tasks.add", "WaitBalancingTimer", this.intTimerWait.ToString(), "1", "1", "procon.protected.plugins.call", "TrueBalancer", "BalancingTimer");
                            }
                        }
                    }
                    else
                    {
                        this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                    }
                }


            }
            else
            {
                DebugInfoSkill("^3Scrambler Active Comparing teams!");
            }

            //}
            //else
            //{
            //    this.DebugInfo("Waiting for new round! WaitforOPSec: " + WaitforOPSec.ToString());
            //}
        }

        public void BalancingTimer()
        {

            this.DebugInfo("------Timer-----");

            if (this.intPlayerDif < 2)
            {
                this.DebugInfo("^2Teams are balanced.(timer)");
                this.boolfirstwarningWL = false;
                this.boolneedbalance = false;
                this.intcountWarnings = 0;
                this.boolwaitfordeath = false;
                this.strdeadplayer = "";
                this.boolbalanced = true;

                if (this.boolstartBalance == true)
                {
                    this.boolstartBalance = false;
                    this.boolafterBalance = true;
                }
                this.intToTeam = 0;
                this.intFromTeam = 0;
                this.boolwaitdead = false;
            }

            if (this.boolneedbalance)
            {
                this.intcountWarnings++;
                if (this.intcountWarnings <= this.intWarnings)
                {
                    // Warnungen ausgeben. strWarning
                    string strTEMP = this.strWarning.Replace("%Warning%", Convert.ToString(this.intcountWarnings));
                    strTEMP = strTEMP.Replace("%maxWarnings%", Convert.ToString(this.intWarnings));
                    if (ynbShowWarnings == enumBoolYesNo.Yes)
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strTEMP);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                        }
                    }
                    this.DebugInfo(strTEMP);

                }
                if (this.intcountWarnings > this.intWarnings)
                {
                    this.boolstartBalance = true;
                    this.boolwaitfordeath = true;
                    //AUSFÜHREN von SERVERINFO
                    //this.ExecuteCommand("procon.protected.send", "serverInfo");
                    if (/*this.strdeadplayer != "" && this.boolwaitfordeath && */this.boolwaitdead == false)
                    {
                        if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Rush") || this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CaptureTheFlag0") || this.strcurrentGametype.Contains("Elimination") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                        {
                            if (this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore)
                            {
                                if (this.boolbalanced)
                                {
                                    this.DebugInfo("^b^4" + this.strcurrentGametype + "^n: Starting Balance!");
                                    startBalancing();
                                }
                                else
                                {
                                    this.DebugInfo("^5Waiting for player to be balanced.");
                                }
                            }
                            else
                            {
                                this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                            }
                        }
                        if (this.strcurrentGametype.Contains("Domination"))
                        {
                            if (this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore)
                            {
                                if (this.boolbalanced)
                                {
                                    this.DebugInfo("^b^4" + this.strcurrentGametype + "^n: Starting Balance!");
                                    startBalancing();
                                }
                                else
                                {
                                    this.DebugInfo("^5Waiting for player to be balanced.");
                                }
                            }
                            else
                            {
                                this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Obliteration"))
                        {
                            if (this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore)
                            {
                                if (this.boolbalanced)
                                {
                                    this.DebugInfo("^b^4" + this.strcurrentGametype + "^n: Starting Balance!");
                                    startBalancing();
                                }
                                else
                                {
                                    this.DebugInfo("^5Waiting for player to be balanced.");
                                }
                            }
                            else
                            {
                                this.DebugInfo("^3Not starting Balance. Tickets till Endround: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                            }
                        }
                        else if (this.strcurrentGametype.Contains("TeamDeathMatch"))
                        {
                            if (this.intScoreTeamA < this.intminScore && this.intScoreTeamB < this.intminScore)
                            {
                                this.DebugInfo("^b^4" + this.strcurrentGametype + "^n: Starting Balance!");
                                startBalancing();
                            }
                            else
                            {
                                this.DebugInfo("^3Not starting Balance. Tickets: TeamA: ^b" + this.intScoreTeamA + "^n, TeamB: ^b" + this.intScoreTeamB);
                            }
                        }
                        else
                        {
                            this.DebugInfo("^8^b" + this.strcurrentGametype + " not supported!");
                        }
                    }

                    //Start Balance
                    // if ( this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore){
                    // startBalancing();
                    // } else {
                    // this.DebugInfo("Not starting Balance. Tickets till Endround: TeamA: " + this.intScoreTeamA + ", TeamB: " + this.intScoreTeamB);
                    // }
                }
            }

            this.booltimer = false;
            // if (this.boolLevelStart)
            // CompareTeams();
            this.DebugInfo("Timer End");
        }

        public void startBalancing()
        {

            this.DebugInfo("startBalancing");
            this.boolstartBalance = true;
            this.boolwaitdead = true;

            //DateTime maxValue = new DateTime();
            this.strMovedPlayer = "";


            Dictionary<string, CPlayerJoinInf> dicPlayerSorted = new Dictionary<string, CPlayerJoinInf>();
            Dictionary<string, CPlayerJoinInf> dicPlayerSortedTEMP = new Dictionary<string, CPlayerJoinInf>();

            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                if (this.dicPlayerCache[kvp.Key].teamID == this.intFromTeam && this.dicPlayerCache[kvp.Key].playerWL == 0 && !dicPlayerCache[kvp.Key].IsCommander && !dicPlayerCache[kvp.Key].IsSpectator)
                {
                    dicPlayerSortedTEMP.Add(kvp.Key, kvp.Value);
                }
            }

            /*      
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp1 in dicPlayerSortedTEMP)
            {
                DateTime maxValueJoined = new DateTime();
                int minpoints = 100000000;
                KeyValuePair<string, CPlayerJoinInf> kvplastjoiner = new KeyValuePair<string, CPlayerJoinInf>();
                
                foreach (KeyValuePair<string, CPlayerJoinInf> kvp2 in dicPlayerSortedTEMP)
                {
                    if (dicPlayerSortedTEMP[kvp2.Key].score <= minpoints && dicPlayerSorted.ContainsKey(kvp2.Key) == false)
                    {
                        minpoints = dicPlayerSortedTEMP[kvp2.Key].score;
                        kvplastjoiner = kvp2;
                    }
                }
                dicPlayerSorted.Add(kvplastjoiner.Key, kvplastjoiner.Value);    
            }
            */

            bool Sortiert = true;
            do
            {
                Sortiert = true;
                DateTime maxValueJoined = new DateTime();
                int minscore = 100000000;
                KeyValuePair<string, CPlayerJoinInf> kvplastjoiner = new KeyValuePair<string, CPlayerJoinInf>();

                foreach (KeyValuePair<string, CPlayerJoinInf> kvp2 in dicPlayerSortedTEMP)
                {

                    if (dicPlayerSorted.ContainsKey(kvp2.Key) == false && dicPlayerSortedTEMP[kvp2.Key].score < minscore)
                    {
                        minscore = dicPlayerSortedTEMP[kvp2.Key].score;
                        maxValueJoined = dicPlayerSortedTEMP[kvp2.Key].Playerjoined;
                        kvplastjoiner = kvp2;
                        Sortiert = false;
                    }
                    else if (dicPlayerSorted.ContainsKey(kvp2.Key) == false && dicPlayerSortedTEMP[kvp2.Key].score == minscore && maxValueJoined < dicPlayerSortedTEMP[kvp2.Key].Playerjoined)
                    {
                        maxValueJoined = dicPlayerSortedTEMP[kvp2.Key].Playerjoined;
                        kvplastjoiner = kvp2;
                        Sortiert = false;
                    }
                }
                if (Sortiert == false)
                {
                    dicPlayerSorted.Add(kvplastjoiner.Key, kvplastjoiner.Value);
                }
                if (Sortiert)
                    this.DebugInfo("sorted");

            } while (!Sortiert);


            string strsorted = "";
            string printSoldier = "";
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in dicPlayerSorted)
            {
                printSoldier = kvp.Key.Replace("{", "(");
                printSoldier = printSoldier.Replace("}", ")");

                strsorted = strsorted + "^4" + Convert.ToString(dicPlayerSorted[kvp.Key].score) +
                "^9 / ^5" + dicPlayerSorted[kvp.Key].Playerjoined.ToString("HH:mm:ss") + "^9 - ^0^b" + printSoldier + "^n^3 -=- ^9";
            }

            this.DebugInfo(strsorted);


            List<string> ToBeMovedList = new List<string>(dicPlayerSorted.Keys);
            int itemCount = ToBeMovedList.Count;

            int dblcutoff = 5;
            if (this.intFromTeam == 1)
            {
                if (this.TeamA / 2 > 5)
                    dblcutoff = this.TeamA / 2;
            }
            else if (this.intFromTeam == 2)
            {
                if (this.TeamB / 2 > 5)
                    dblcutoff = this.TeamB / 2;
            }
            this.DebugInfo("Cutoff: ^b" + dblcutoff.ToString());
            for (int k = itemCount - 1; k >= dblcutoff; k--)
            {
                //this.DebugInfo("removed");
                ToBeMovedList.RemoveAt(k);
                //itemCount = ToBeMovedList.Count;
            }

            string completelist = "";
            itemCount = ToBeMovedList.Count;
            for (int k = 0; k < itemCount; k++)
            {
                completelist = completelist + "^0^b" + ToBeMovedList[k] + "^9^n: ^4" + this.dicPlayerCache[ToBeMovedList[k]].score + "^9 -=- ";
            }
            this.DebugInfo(completelist);

            bool willbebalanced = false;

            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                if (this.dicPlayerCache[kvp.Key].tobebalanced && this.dicPlayerCache[kvp.Key].teamID == this.intFromTeam)
                {
                    this.strMovedPlayer = kvp.Key;
                    willbebalanced = true;
                    break;
                }
            }

            if (!willbebalanced && ToBeMovedList.Contains(this.strdeadplayer))
            {
                this.strMovedPlayer = this.strdeadplayer;
                willbebalanced = true;
            }

            if (!willbebalanced)
            {
                this.DebugInfo("^3No Player dead and/or marked to be moved.");
                this.strdeadplayer = "";
            }
            else
            {
                string strTEMP = this.strLastWarning.Replace("%MovedPlayer%", this.strMovedPlayer);
                if (ynbShowBallancing == enumBoolYesNo.Yes)
                {
                    if (this.boolVirtual)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strTEMP);
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                    }
                }
                this.boolbalanced = false;
                if (this.boolVirtual)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + this.strMovedPlayer + " " + this.intToTeam.ToString() + " 0 false");
                }
                else
                {
                    this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.strMovedPlayer, this.intToTeam.ToString(), "0", "false");
                    LogMove("AUTOBALANCING [Player: " + this.strMovedPlayer + "]");
                }
                this.DebugInfo("playermoved");
                this.ExecuteCommand("procon.protected.chat.write", "AUTOBALANCING [Player: " + this.strMovedPlayer + "]");
                this.BalancedPlayers.Add(this.strMovedPlayer);

                this.dicPlayerCache[this.strMovedPlayer].teamID = intToTeam;
                this.dicPlayerCache[this.strMovedPlayer].playerWL = 2;
                this.dicPlayerCache[this.strMovedPlayer].tobebalanced = false;
                dicPlayerCache[this.strMovedPlayer].Playerjoined = DateTime.Now;
                string strTEMP2 = this.strBeenMoved.Replace("%MovedPlayer%", this.strMovedPlayer);

                if (ynbShowPlayermessage == enumBoolYesNo.Yes)
                {
                    if (this.boolVirtual)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + this.strMovedPlayer + " - " + strTEMP2);
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP2, "player", this.strMovedPlayer);
                    }
                }

                this.DebugInfo("^b^0" + this.strMovedPlayer + "^n:^4 " + dicPlayerCache[this.strMovedPlayer].score + "^9 - ^4" + dicPlayerCache[this.strMovedPlayer].Playerjoined.ToString("HH:mm:ss") + "^9 -=- ^4" +
                this.intFromTeam.ToString() + "." + dicPlayerCache[this.strMovedPlayer].playerSquad + " ---> " + dicPlayerCache[this.strMovedPlayer].teamID.ToString());
                this.DebugInfo(strTEMP);
                this.DebugInfo(strTEMP2);
                this.strdeadplayer = "";
            }

            this.DebugInfo("startBalancing end");
            this.boolwaitdead = false;
        }

        #endregion
    }
}
