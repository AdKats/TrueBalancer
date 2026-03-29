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
        #region SkillScrambler

        public void DebugInfoSkill(string DebugMessage)
        {
            if (ynbDebugModeSkill == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n " + DebugMessage);
                //TextWriter tw = new StreamWriter("TrueBalancer.txt",true);
                //tw.WriteLine(DateTime.Now.ToString() + ": " + DebugMessage);
                //tw.Close();
            }
        }

        public void StartScrambler()
        {
            //this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
            // NEU - Die Squadsortierung und das scramblen hier nach unten verschieben, um sicherzustellen, dass auch nur die gescrambled werden, die auch auf dem Server sind.
            DebugInfoSkill("^4Starting Scrambler now!");

            int CompareSlots = 0;

            if (this.intMaxSlots >= 48)
                CompareSlots = intMaxSlots - 1;
            else
                CompareSlots = intMaxSlots;


            if (this.dicPlayerCache.Count < CompareSlots)
            {

                this.dicSquadScore.Clear();
                this.DTScramblestarted = DateTime.Now;
                this.boolscrambleActive = true;
                this.boolTeamsScrambled = false;
                this.boolFirstOP = false;
                this.intScrambleCount = 0;
                this.strErrorMsg = "";



                if (this.strScrambleMode == "Keep all Squads")
                {
                    KeepAllSquads();
                }
                else if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    KeepClanMates();
                }
                else if (this.strScrambleMode == "Keep no squads")
                {
                    KeepNoSquads();
                }



                //move them all out of squads
                foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
                {
                    if (this.boolVirtual)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + kvp.Key + " " + this.dicPlayerCache[kvp.Key].teamID.ToString() + " 0 true");
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", kvp.Key, this.dicPlayerCache[kvp.Key].teamID.ToString(), "0", "true");
                    }
                }
                ScrambleNow();


                //this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "1", "1", "1",  "procon.protected.plugins.call", "TrueBalancer", "ScrambleNow");
            }
            else
            {
                this.DebugInfoSkill("^3^bWas not able to scramble, because the server is full.");
            }

        }

        public void ScrambleNow()
        {

            this.boolscrambleNow = true;
            this.intScrambleCount++;
            bool boolScrambledall = true;
            this.DebugInfoSkill("^4Scrambling now!");

            TimeSpan ScrambleTime = new TimeSpan(0);
            ScrambleTime = DateTime.Now - this.DTScramblestarted;
            if (ScrambleTime.TotalSeconds > 20 || this.boolTeamsScrambled)
            {
                this.DebugInfoSkill("^3^bWas not able to scramble teams in 15 seconds! Teams partly scrambled. TeamsScrambled? " + this.boolTeamsScrambled.ToString());
                if (this.boolLevelStart)
                {
                    if (this.ynbScrambleMessage == enumBoolYesNo.Yes)
                    {
                        if (this.strScrambleDoneMsg != "")
                        {
                            if (this.boolVirtual)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strScrambleDoneMsg);
                            }
                            else
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.say", strScrambleDoneMsg, "all");
                            }
                            if (this.ynbYellScrambleManuall == enumBoolYesNo.Yes && !this.boolVirtual)
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.yell", strScrambleDoneMsg, "30");
                            }
                        }
                    }
                }

                this.boolTeamsScrambled = true;
                this.intScrambledPlayers = 0;
                this.boolscrambleNow = false;
                this.boolscrambleActive = false;
                this.intScrambleCount = 0;
            }

            if (this.boolscrambleActive)
            {

                if (this.boolfirstscrambler || this.boolscramblefailed)
                {
                    this.boolfirstscrambler = false;
                    this.boolscramblefailed = false;

                    if (this.strErrorMsg == "SetSquadFailed")
                    {
                        string strfailedSquad = "";
                        int SSFTeamID = 0;
                        int intnewSquad = 0;

                        foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                        {
                            if (this.m_isPluginEnabled == false) break;
                            if (this.dicPlayerCache.ContainsKey(this.dicPlayerScore[kvp.Key].playerName) && !this.dicPlayerScore[kvp.Key].scrambled)
                            {
                                this.strFinalSquad = this.dicPlayerScore[kvp.Key].teamID + "." + this.dicPlayerScore[kvp.Key].playerSquad;
                                if (!this.dicSquadList.ContainsKey(this.strFinalSquad))
                                {
                                    this.dicSquadList.Add(this.strFinalSquad, this.dicPlayerScore[kvp.Key].playerSquad);
                                }
                                if (strfailedSquad == "")
                                {
                                    SSFTeamID = this.dicPlayerScore[kvp.Key].teamID;
                                    strfailedSquad = this.strFinalSquad;
                                    /*for (int j = 1; j < 17; j++) {
                                        if (j<17 && !this.dicSquadList.ContainsValue(j)){
                                            intnewSquad = j;
                                            this.dicPlayerScore[kvp.Key].playerSquad = j;
                                            this.dicSquadList.Add(this.dicPlayerScore[kvp.Key].teamID.ToString() + "." + j.ToString(), j);
                                            break;
                                        }else if (j==17){
                                            intnewSquad = 0;
                                            this.dicPlayerScore[kvp.Key].playerSquad = 0;
                                            break;
                                        }
                                    }*/
                                }

                                if (strfailedSquad != this.strFinalSquad)
                                {
                                    this.DebugInfoSkill("^1" + this.strFinalSquad + ":" + "(SSF[noChange])Scramble Player: " + this.dicPlayerScore[kvp.Key].playerName);
                                    boolScrambledall = false;
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + this.dicPlayerScore[kvp.Key].playerName + " " + this.dicPlayerScore[kvp.Key].teamID.ToString() + " " + this.dicPlayerScore[kvp.Key].playerSquad.ToString() + " true");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.dicPlayerScore[kvp.Key].playerName, this.dicPlayerScore[kvp.Key].teamID.ToString(), this.dicPlayerScore[kvp.Key].playerSquad.ToString(), "true");
                                    }
                                }

                                /*if (strfailedSquad == this.strFinalSquad && intnewSquad != -1){
                                    this.dicPlayerScore[kvp.Key].playerSquad = intnewSquad;
                                    this.DebugInfoSkill("^1Before: ^b" + this.strFinalSquad + "^n, Change to: ^b" + intnewSquad.ToString() + "^n^8(SSF[Change])^1Scramble Player: " + this.dicPlayerScore[kvp.Key].playerName);
                                    boolScrambledall = false;
                                    this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.dicPlayerScore[kvp.Key].playerName, this.dicPlayerScore[kvp.Key].teamID.ToString(), this.dicPlayerScore[kvp.Key].playerSquad.ToString(), "true");
                                    
                                }else{
                                    this.DebugInfoSkill("^1" + this.strFinalSquad + ":" +  "(SSF[noChange])Scramble Player: " + this.dicPlayerScore[kvp.Key].playerName);
                                    boolScrambledall = false;
                                    this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.dicPlayerScore[kvp.Key].playerName, this.dicPlayerScore[kvp.Key].teamID.ToString(), this.dicPlayerScore[kvp.Key].playerSquad.ToString(), "true");
                                    
                                }*/
                            }
                        }

                        for (int j = 1; j <= 20; j++)
                        {
                            if (!this.dicSquadList.ContainsKey(SSFTeamID + "." + j))
                            {
                                intnewSquad = j;
                                this.dicSquadList.Add(SSFTeamID + "." + j, j);
                                break;
                            }

                        }

                        int playersinsquad = 0;

                        foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                        {
                            if (this.m_isPluginEnabled == false) break;
                            this.strFinalSquad = this.dicPlayerScore[kvp.Key].teamID + "." + this.dicPlayerScore[kvp.Key].playerSquad;
                            if (this.dicPlayerCache.ContainsKey(this.dicPlayerScore[kvp.Key].playerName) && this.strFinalSquad == strfailedSquad)
                            {
                                if (playersinsquad < 4)
                                {
                                    playersinsquad++;
                                    this.dicPlayerScore[kvp.Key].playerSquad = intnewSquad;
                                    this.DebugInfoSkill("^1Before: ^b" + this.strFinalSquad + "^n, Change to: ^b" + intnewSquad.ToString() + "^n^8(SSF[Change])^1Scramble Player: " + this.dicPlayerScore[kvp.Key].playerName);
                                    boolScrambledall = false;
                                    this.dicPlayerScore[kvp.Key].scrambled = false;
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + this.dicPlayerScore[kvp.Key].playerName + " " + this.dicPlayerScore[kvp.Key].teamID.ToString() + " " + this.dicPlayerScore[kvp.Key].playerSquad.ToString() + " true");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.dicPlayerScore[kvp.Key].playerName, this.dicPlayerScore[kvp.Key].teamID.ToString(), this.dicPlayerScore[kvp.Key].playerSquad.ToString(), "true");
                                    }

                                }
                                else
                                {
                                    this.dicPlayerScore[kvp.Key].playerSquad = 0;
                                    this.DebugInfoSkill("^1Before: ^b" + this.strFinalSquad + "^n, Change to: ^b 0 ^n^8(SSF[4+ players in Squad])^1Scramble Player: " + this.dicPlayerScore[kvp.Key].playerName);
                                    boolScrambledall = false;
                                    this.dicPlayerScore[kvp.Key].scrambled = false;
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + this.dicPlayerScore[kvp.Key].playerName + " " + this.dicPlayerScore[kvp.Key].teamID.ToString() + " " + this.dicPlayerScore[kvp.Key].playerSquad.ToString() + " true");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.dicPlayerScore[kvp.Key].playerName, this.dicPlayerScore[kvp.Key].teamID.ToString(), this.dicPlayerScore[kvp.Key].playerSquad.ToString(), "true");
                                    }
                                }
                            }
                        }



                    }
                    else
                    {
                        foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                        {
                            if (this.m_isPluginEnabled == false) break;
                            if (this.dicPlayerCache.ContainsKey(this.dicPlayerScore[kvp.Key].playerName) && !this.dicPlayerScore[kvp.Key].scrambled)
                            {
                                this.strFinalSquad = this.dicPlayerScore[kvp.Key].teamID + "." + this.dicPlayerScore[kvp.Key].playerSquad;
                                if (!this.dicSquadList.ContainsKey(this.strFinalSquad))
                                {
                                    this.dicSquadList.Add(this.strFinalSquad, this.dicPlayerScore[kvp.Key].playerSquad);
                                }
                                //this.DebugInfoSkill("^4" + this.strFinalSquad + ":" +  "Scramble Player: " + this.dicPlayerScore[kvp.Key].playerName);
                                boolScrambledall = false;
                                if (this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + this.dicPlayerScore[kvp.Key].playerName + " " + this.dicPlayerScore[kvp.Key].teamID.ToString() + " " + this.dicPlayerScore[kvp.Key].playerSquad.ToString() + " true");
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.movePlayer", this.dicPlayerScore[kvp.Key].playerName, this.dicPlayerScore[kvp.Key].teamID.ToString(), this.dicPlayerScore[kvp.Key].playerSquad.ToString(), "true");
                                }

                            }
                        }
                    }

                    this.strErrorMsg = "";
                }
                else
                {
                    this.DebugInfoSkill("^1Waiting for players to be moved");
                    boolScrambledall = false;
                }
            }

            if (!boolScrambledall)
            {
                this.boolRunOnList = true;
                this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
            }
            else
            {
                this.boolRunOnList = false;
                this.DebugInfoSkill("^4^bTeams are scrambled now! boolScrambledall");
                this.boolscrambleNow = false;
                this.boolscrambleActive = false;
                this.intScrambleCount = 0;
                this.boolTeamsScrambled = true;
                this.intScrambledPlayers = 0;
                if (this.boolLevelStart)
                {
                    if (this.ynbScrambleMessage == enumBoolYesNo.Yes)
                    {
                        if (this.strScrambleDoneMsg != "")
                        {
                            if (this.boolVirtual)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + strScrambleDoneMsg);
                            }
                            else
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.say", strScrambleDoneMsg, "all");
                            }
                            if (this.ynbYellScrambleManuall == enumBoolYesNo.Yes && !this.boolVirtual)
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.yell", strScrambleDoneMsg, "30");
                            }
                        }
                    }
                }
            }
        }

        public void OnCommandScrambleNow(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            bool blIsAdmin = false;
            CPrivileges cpAccount = this.GetAccountPrivileges(strSpeaker);
            if (cpAccount != null && cpAccount.PrivilegesFlags > 0) { blIsAdmin = true; }

            if (blIsAdmin)
            {
                this.DebugInfoSkill("^1Admin requested a scramble now!");

                this.TSLevelStartWait = DateTime.Now - this.DTLevelStart;
                if (this.boolLevelStart && this.boolFirstOP)
                {

                    if (this.ynbEnableScrambleNow == enumBoolYesNo.Yes)
                    {
                        if (!this.boolscrambleActive)
                        {

                            if (this.ynbScrambleMessage == enumBoolYesNo.Yes)
                            {
                                if (this.strScrambleNowMsg != "")
                                {
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleNowMsg);
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleNowMsg, "all");
                                    }
                                    if (this.ynbYellScrambleManuall == enumBoolYesNo.Yes && !this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleNowMsg, "30");
                                    }
                                }
                            }

                            int i = 1;
                            this.dicPlayerScore.Clear();
                            this.dicSquadScore.Clear();
                            this.bestSquadTeamID = 0;


                            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
                            {
                                double value = this.dicPlayerCache[kvp.Key].playerValue;
                                string tag = this.dicPlayerCache[kvp.Key].tag;
                                CPlayerScoreInf newEntry = new CPlayerScoreInf(kvp.Key, this.dicPlayerCache[kvp.Key].teamID, this.dicPlayerCache[kvp.Key].playerSquad, value, false, false, tag);
                                this.dicPlayerScore.Add(i, newEntry);
                                i++;

                            }


                            bool Sortiert;
                            do
                            {
                                Sortiert = true;
                                for (int j = 1; j < this.dicPlayerScore.Count; j++)
                                {
                                    if (this.dicPlayerScore[j].playerValue < this.dicPlayerScore[j + 1].playerValue)
                                    {
                                        CPlayerScoreInf temp = new CPlayerScoreInf(this.dicPlayerScore[j].playerName, this.dicPlayerScore[j].teamID, this.dicPlayerScore[j].playerSquad, this.dicPlayerScore[j].playerValue, false, false, this.dicPlayerScore[j].tag);
                                        this.dicPlayerScore[j] = this.dicPlayerScore[j + 1];
                                        this.dicPlayerScore[j + 1] = temp;
                                        Sortiert = false;
                                    }
                                }
                            } while (!Sortiert);

                            this.boolFirstOP = false;
                            this.boolwaitfordeath = false;
                            this.boolscrambleActive = true;
                            this.boolTeamsScrambled = false;
                            this.intScrambledPlayers = 0;
                            this.teamswitcher.Clear();


                            this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "3", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                            //StartScrambler();

                        }
                        else
                        {
                            this.DebugInfoSkill("^3A scrambing has allready been requested.");
                            if (this.boolVirtual)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSpeaker + " - " + "A scrambing has allready been requested.");
                            }
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.say", "A scrambing has allready been requested.", "player", strSpeaker);
                            }
                        }

                    }
                    else
                    {
                        this.DebugInfoSkill("^3This command is deactivated at the moment. Activate in in PRoCon.");
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSpeaker + " - " + "This command is not activated for your server in PRoCon.");
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", "This command is not activated for your server in PRoCon.", "player", strSpeaker);
                        }
                    }

                }
            }
        }

        public void OnCommandScrambleRound(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            bool blIsAdmin = false;
            CPrivileges cpAccount = this.GetAccountPrivileges(strSpeaker);
            if (cpAccount != null && cpAccount.PrivilegesFlags > 0) { blIsAdmin = true; }
            if (blIsAdmin)
            {
                this.DebugInfoSkill("^1Admin requested a scramble next round!");

                if (this.ynbEnableScrambleRound == enumBoolYesNo.Yes)
                {
                    this.boolscramblebyadminroundend = true;
                    if (this.boolLevelStart)
                    {
                        if (this.ynbScrambleMessage == enumBoolYesNo.Yes)
                        {
                            if (this.strScrambleRoundMsg != "")
                            {
                                if (this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleRoundMsg);
                                }
                                else
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleRoundMsg, "all");
                                }
                                if (this.ynbYellScrambleManuall == enumBoolYesNo.Yes && !this.boolVirtual)
                                {
                                    this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleRoundMsg, "30");
                                }
                            }
                        }
                    }
                }
                else
                {
                    this.DebugInfoSkill("^3This command is deactivated at the moment. Activate it in PRoCon.");
                    if (this.boolVirtual)
                    {
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSpeaker + " - " + "This command is not activated for your server in PRoCon.");
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "This command is not activated for your server in PRoCon.", "player", strSpeaker);
                    }
                }
            }
        }

        public void OnScrambleViaPRoCon()
        {
            this.ynbScrambleRoundViaPRoConConf = enumBoolYesNo.No;
            this.ynbScrambleRoundViaPRoCon = enumBoolYesNo.No;

            if (this.ynbEnableScrambleRound == enumBoolYesNo.Yes)
            {
                if (this.boolLevelStart)
                {
                    this.DebugInfoSkill("^1Admin requested a scramble for next round!");
                    this.boolscramblebyadminroundend = true;
                    if (this.ynbScrambleMessage == enumBoolYesNo.Yes)
                    {
                        if (this.strScrambleRoundMsg != "")
                        {
                            if (this.boolVirtual)
                            {
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleRoundMsg);
                            }
                            else
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleRoundMsg, "all");
                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n Teams will be scrambled at RoundEnd");
                            }
                            if (this.ynbYellScrambleManuall == enumBoolYesNo.Yes && !this.boolVirtual)
                            {
                                this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleRoundMsg, "30");
                            }
                        }
                    }
                }
                else
                {
                    this.DebugInfoSkill("^3You can only request for a scramble, if a round is running.");
                }
            }
            else
            {
                this.DebugInfoSkill("You need to activate and setup !scrambleround at '2.5 Skill-Scrambler: Manual Commands'");
            }
        }

        public void LogMove(string MoveMsg)
        {
            if (showMoves == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n " + MoveMsg);
            }
        }

        public void KeepAllSquads()
        {

            //KEEP ALL SQUADS START
            string DebugScoreList = "";
            string strTeam1 = "";
            string strTeam2 = "";
            bool squadexists = false;
            int squadIDnew = 0;

            int intTeamA = 0;
            int intTeamB = 0;

            List<int> toremoveKeys = new List<int>();
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].teamID == 1)
                {
                    strTeam1 = strTeam1 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
                else if (this.dicPlayerScore[kvp.Key].teamID == 2)
                {
                    strTeam2 = strTeam2 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }

                if (this.dicPlayerCache.ContainsKey(this.dicPlayerScore[kvp.Key].playerName))
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == 1)
                        intTeamA++;
                    if (this.dicPlayerScore[kvp.Key].teamID == 2)
                        intTeamB++;
                }
                else
                {
                    toremoveKeys.Add(kvp.Key);
                }
            }
            foreach (int removeKey in toremoveKeys)
            {
                this.dicPlayerScore.Remove(removeKey);
            }


            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {

                if (this.dicPlayerScore[kvp.Key].playerSquad != 0)
                {
                    squadexists = false;
                    foreach (KeyValuePair<int, CSquadScoreInf> kvpsquad in this.dicSquadScore)
                    {
                        if (this.dicSquadScore[kvpsquad.Key].teamID == this.dicPlayerScore[kvp.Key].teamID &&
                                    this.dicSquadScore[kvpsquad.Key].squadID == this.dicPlayerScore[kvp.Key].playerSquad)
                        {
                            this.dicSquadScore[kvpsquad.Key].squadScore = this.dicSquadScore[kvpsquad.Key].squadScore + this.dicPlayerScore[kvp.Key].playerValue;
                            this.dicSquadScore[kvpsquad.Key].squadsize++;
                            squadexists = true;
                            break;
                        }
                    }
                    if (!squadexists)
                    {
                        CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[kvp.Key].teamID, this.dicPlayerScore[kvp.Key].playerSquad, 1, this.dicPlayerScore[kvp.Key].playerValue, false);
                        squadIDnew++;
                        this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                    }
                }

            }


            foreach (KeyValuePair<int, CSquadScoreInf> kvpsquad in this.dicSquadScore)
            {
                this.dicSquadScore[kvpsquad.Key].squadScore = this.dicSquadScore[kvpsquad.Key].squadScore / this.dicSquadScore[kvpsquad.Key].squadsize;
            }

            bool Sortiert = true;

            do
            {
                Sortiert = true;
                for (int j = 1; j < this.dicSquadScore.Count; j++)
                {
                    if (this.dicSquadScore[j].squadsize < this.dicSquadScore[j + 1].squadsize)
                    {
                        CSquadScoreInf tempsquad = new CSquadScoreInf(this.dicSquadScore[j].teamID, this.dicSquadScore[j].squadID, this.dicSquadScore[j].squadsize, this.dicSquadScore[j].squadScore, false);
                        this.dicSquadScore[j] = this.dicSquadScore[j + 1];
                        this.dicSquadScore[j + 1] = tempsquad;
                        Sortiert = false;
                    }
                    else if (this.dicSquadScore[j].squadsize == this.dicSquadScore[j + 1].squadsize && this.dicSquadScore[j].squadScore < this.dicSquadScore[j + 1].squadScore)
                    {
                        CSquadScoreInf tempsquad = new CSquadScoreInf(this.dicSquadScore[j].teamID, this.dicSquadScore[j].squadID, this.dicSquadScore[j].squadsize, this.dicSquadScore[j].squadScore, false);
                        this.dicSquadScore[j] = this.dicSquadScore[j + 1];
                        this.dicSquadScore[j + 1] = tempsquad;
                        Sortiert = false;
                    }
                }

            } while (!Sortiert);

            string DebugSquadSorted = "";
            foreach (KeyValuePair<int, CSquadScoreInf> kvpsquad in this.dicSquadScore)
            {
                DebugSquadSorted = DebugSquadSorted + "^0" + this.dicSquadScore[kvpsquad.Key].teamID + "." + this.dicSquadScore[kvpsquad.Key].squadID +
                                    ": ^7" + this.dicSquadScore[kvpsquad.Key].squadsize + "^9*^5" + this.dicSquadScore[kvpsquad.Key].squadScore + "^9 --- ";
            }


            DebugScoreList = "Before Scramble:\nTeam 1: " + strTeam1 + "\n\nTeam 2: " + strTeam2;
            this.DebugInfoSkill(DebugScoreList);

            bestSquadTeamID = this.dicSquadScore[1].teamID;
            this.DebugInfoSkill("Squads sorted: " + DebugSquadSorted);

            List<int> SquadsTeamA = new List<int>();
            List<int> SquadsTeamB = new List<int>();
            double TeamValueA = 0;
            int TeamSizeA = 0;
            double TeamValueB = 0;
            int TeamSizeB = 0;

            int n = 0;

            foreach (KeyValuePair<int, CSquadScoreInf> kvpsquad in this.dicSquadScore)
            {
                if (n == 0 && kvpsquad.Key == 1)
                    SquadsTeamA.Add(kvpsquad.Key);
                else if (n <= 2)
                    SquadsTeamB.Add(kvpsquad.Key);
                else if (n > 2)
                    SquadsTeamA.Add(kvpsquad.Key);

                n++;

                if (n == 5)
                    n = 1;
            }

            foreach (int squadID in SquadsTeamA)
            {
                TeamValueA = TeamValueA + this.dicSquadScore[squadID].squadScore * this.dicSquadScore[squadID].squadsize;
                TeamSizeA = TeamSizeA + this.dicSquadScore[squadID].squadsize;
            }

            foreach (int squadID in SquadsTeamB)
            {
                TeamValueB = TeamValueB + this.dicSquadScore[squadID].squadScore * this.dicSquadScore[squadID].squadsize;
                TeamSizeB = TeamSizeB + this.dicSquadScore[squadID].squadsize;
            }


            double AverageTeamA = TeamValueA / TeamSizeA;
            double AverageTeamB = TeamValueB / TeamSizeB;
            double AverageDiff = AverageTeamA - AverageTeamB;

            this.DebugInfoSkill("SortValue before adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
            this.DebugInfoSkill("Average Difference before adjustment: ^b^2" + AverageDiff);

            bool adjusted = false;
            do
            {
                adjusted = true;
                double tempTeamValueA = 0;
                double tempTeamValueB = 0;
                double tempAverageTeamA = 0;
                double tempAverageTeamB = 0;
                double tempAverageDiff = 0;
                double adjustvalue = AverageDiff;
                int moveIDA = 0;
                int moveIDB = 0;

                foreach (int squadIDA in SquadsTeamA)
                {
                    foreach (int squadIDB in SquadsTeamB)
                    {
                        if (adjustvalue > 0 && this.dicSquadScore[squadIDA].squadScore > this.dicSquadScore[squadIDB].squadScore && this.dicSquadScore[squadIDA].squadsize == this.dicSquadScore[squadIDB].squadsize)
                        {
                            tempTeamValueA = TeamValueA - this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize +
                                this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize;
                            tempTeamValueB = TeamValueB - this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize +
                                this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = squadIDA;
                                moveIDB = squadIDB;
                                adjusted = false;
                            }

                        }
                        else if (adjustvalue < 0 && this.dicSquadScore[squadIDA].squadScore < this.dicSquadScore[squadIDB].squadScore && this.dicSquadScore[squadIDA].squadsize == this.dicSquadScore[squadIDB].squadsize)
                        {
                            tempTeamValueA = TeamValueA - this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize +
                                this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize;
                            tempTeamValueB = TeamValueB - this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize +
                                this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = squadIDA;
                                moveIDB = squadIDB;
                                adjusted = false;
                            }

                        }

                    }
                }



                if (!adjusted)
                {
                    SquadsTeamA.Remove(moveIDA);
                    SquadsTeamA.Add(moveIDB);
                    TeamValueA = TeamValueA - this.dicSquadScore[moveIDA].squadScore * this.dicSquadScore[moveIDA].squadsize +
                                this.dicSquadScore[moveIDB].squadScore * this.dicSquadScore[moveIDB].squadsize;
                    AverageTeamA = TeamValueA / TeamSizeA;

                    SquadsTeamB.Remove(moveIDB);
                    SquadsTeamB.Add(moveIDA);
                    TeamValueB = TeamValueB - this.dicSquadScore[moveIDB].squadScore * this.dicSquadScore[moveIDB].squadsize +
                                this.dicSquadScore[moveIDA].squadScore * this.dicSquadScore[moveIDA].squadsize;
                    AverageTeamB = TeamValueB / TeamSizeB;

                    AverageDiff = AverageTeamA - AverageTeamB;

                    this.DebugInfoSkill("Adjustment: ^b^2 1." + moveIDA + " ^9<-->^2 2." + moveIDB);
                }

            } while (!adjusted);


            this.DebugInfoSkill("SortValue ^bafter^n adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
            this.DebugInfoSkill("Average Difference ^bafter ^nadjustment: ^b^2" + AverageDiff);




            int squadscrambledA = 0;
            int squadscrambledB = 0;
            n = 0;

            Dictionary<string, int> dicNewSquad = new Dictionary<string, int>();

            foreach (int SquadIDA in SquadsTeamA)
            {
                foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == this.dicSquadScore[SquadIDA].teamID && this.dicPlayerScore[kvp.Key].playerSquad == this.dicSquadScore[SquadIDA].squadID && !this.dicPlayerScore[kvp.Key].balanced)
                    {
                        string strTeamSquad = this.dicSquadScore[SquadIDA].teamID + "." + this.dicSquadScore[SquadIDA].squadID;

                        if (dicNewSquad.ContainsKey(strTeamSquad))
                        {
                            this.dicPlayerScore[kvp.Key].playerSquad = dicNewSquad[strTeamSquad];
                        }
                        else
                        {
                            squadscrambledA++;
                            dicNewSquad.Add(strTeamSquad, squadscrambledA);
                            this.dicPlayerScore[kvp.Key].playerSquad = squadscrambledA;
                        }

                        this.dicPlayerScore[kvp.Key].teamID = 1;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                }

            }

            foreach (int SquadIDB in SquadsTeamB)
            {
                foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == this.dicSquadScore[SquadIDB].teamID && this.dicPlayerScore[kvp.Key].playerSquad == this.dicSquadScore[SquadIDB].squadID && !this.dicPlayerScore[kvp.Key].balanced)
                    {
                        string strTeamSquad = this.dicSquadScore[SquadIDB].teamID + "." + this.dicSquadScore[SquadIDB].squadID;

                        if (dicNewSquad.ContainsKey(strTeamSquad))
                        {
                            this.dicPlayerScore[kvp.Key].playerSquad = dicNewSquad[strTeamSquad];
                        }
                        else
                        {
                            squadscrambledB++;
                            dicNewSquad.Add(strTeamSquad, squadscrambledB);
                            this.dicPlayerScore[kvp.Key].playerSquad = squadscrambledB;
                        }

                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                }

            }

            n = 0;
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].playerSquad == 0)
                {
                    if (n == 0 && this.dicPlayerScore[kvp.Key].teamID != 2)
                    {
                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                    else if (n > 0 && n <= 2 && dicPlayerScore[kvp.Key].teamID != 1)
                    {
                        this.dicPlayerScore[kvp.Key].teamID = 1;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                    else if (n > 2 && this.dicPlayerScore[kvp.Key].teamID != 2)
                    {
                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                    n++;
                    if (n == 5) n = 1;
                }
            }

            string DebugSortedList = "";
            strTeam1 = "";
            strTeam2 = "";
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].teamID == 1)
                {
                    strTeam1 = strTeam1 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
                else if (this.dicPlayerScore[kvp.Key].teamID == 2)
                {
                    strTeam2 = strTeam2 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
            }

            DebugSortedList = "\n\nAfter Scramble:\nTeam 1: " + strTeam1 + "\n\nTeam 2: " + strTeam2;
            this.DebugInfoSkill(DebugSortedList);


            this.dicSquadList.Clear();
            this.strFinalSquad = "";
            this.intSquadA = 0;
            this.intSquadB = 0;

            this.DebugInfoSkill("Keeping ALL Squads");
            // KEEP ALL SQUADS END

        }

        public void KeepClanMates()
        {
            //KEEP CLAN SQUADS START
            string DebugScoreList = "";
            string strTeam1 = "";
            string strTeam2 = "";

            int intTeamA = 0;
            int intTeamB = 0;
            int squadless = 0;
            double squadvalue = 0;
            int squadsize = 0;
            int squadIDnew = 0;
            List<string> KeepSquads = new List<string>();
            List<string> squadTags = new List<string>();

            List<int> PlayerTeamA = new List<int>();
            List<int> PlayerTeamB = new List<int>();
            List<int> SquadsTeamA = new List<int>();
            List<int> SquadsTeamB = new List<int>();
            List<int> squadplayers = new List<int>();

            List<int> toremoveKeys = new List<int>();
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].teamID == 1)
                {
                    strTeam1 = strTeam1 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
                else if (this.dicPlayerScore[kvp.Key].teamID == 2)
                {
                    strTeam2 = strTeam2 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }

                if (this.dicPlayerCache.ContainsKey(this.dicPlayerScore[kvp.Key].playerName))
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == 1)
                        intTeamA++;
                    if (this.dicPlayerScore[kvp.Key].teamID == 2)
                        intTeamB++;
                }
                else
                {
                    toremoveKeys.Add(kvp.Key);
                }
            }


            foreach (int removeKey in toremoveKeys)
            {
                this.dicPlayerScore.Remove(removeKey);
            }


            DebugScoreList = "Before Scramble:\nTeam 1: " + strTeam1 + "\n\nTeam 2: " + strTeam2;
            this.DebugInfoSkill(DebugScoreList);



            bool SWITCHsquad = false;
            bool SWITCHplayer = false;

            foreach (KeyValuePair<int, CPlayerScoreInf> kvpCheck in this.dicPlayerScore)
            {
                squadTags.Clear();

                if (this.dicPlayerScore[kvpCheck.Key].playerSquad == 0)
                {
                    squadless++;

                    if (!SWITCHplayer)
                    {
                        SWITCHplayer = true;
                        PlayerTeamA.Add(kvpCheck.Key);
                    }
                    else if (SWITCHplayer)
                    {
                        SWITCHplayer = false;
                        PlayerTeamB.Add(kvpCheck.Key);
                    }

                }
                else
                {
                    squadvalue = this.dicPlayerScore[kvpCheck.Key].playerValue;
                    squadsize = 1;
                    squadplayers = new List<int>();
                    squadplayers.Add(kvpCheck.Key);
                    string CheckSquad = this.dicPlayerScore[kvpCheck.Key].teamID + "." + this.dicPlayerScore[kvpCheck.Key].playerSquad;

                    if (this.dicPlayerScore[kvpCheck.Key].tag != "")
                    {
                        squadTags.Add(this.dicPlayerScore[kvpCheck.Key].tag);
                    }

                    if (KeepSquads.Contains(CheckSquad) == false)
                    {
                        bool deletesquad = true;
                        foreach (KeyValuePair<int, CPlayerScoreInf> kvp2 in this.dicPlayerScore)
                        {
                            if (kvpCheck.Key != kvp2.Key && this.dicPlayerScore[kvpCheck.Key].teamID == this.dicPlayerScore[kvp2.Key].teamID && this.dicPlayerScore[kvpCheck.Key].playerSquad == this.dicPlayerScore[kvp2.Key].playerSquad)
                            {
                                squadvalue = squadvalue + this.dicPlayerScore[kvp2.Key].playerValue;
                                squadsize++;
                                squadplayers.Add(kvp2.Key);
                                if (((((IList<string>)this.strAClantagWhitelistScrambler).Contains(this.dicPlayerScore[kvp2.Key].tag) && this.dicPlayerScore[kvp2.Key].tag != "") || (((IList<string>)this.strAClantagWhitelistScrambler).Contains(this.dicPlayerScore[kvpCheck.Key].tag) && this.dicPlayerScore[kvpCheck.Key].tag != "")) && !KeepSquads.Contains(CheckSquad))
                                {
                                    KeepSquads.Add(this.dicPlayerScore[kvpCheck.Key].teamID + "." + this.dicPlayerScore[kvpCheck.Key].playerSquad);
                                    deletesquad = false;
                                    CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[kvpCheck.Key].teamID, this.dicPlayerScore[kvpCheck.Key].playerSquad, 0, 0, false);
                                    squadIDnew++;
                                    this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                                    DebugInfoSkill(string.Format("WL-Clantag detected, Clantag: ^b^2{0}^n^9 or Clantag: ^b^2{1}^n^9. Best Player: ^2{2}", this.dicPlayerScore[kvp2.Key].tag, this.dicPlayerScore[kvpCheck.Key].tag, this.dicPlayerScore[kvp2.Key].playerName));
                                    //BLEIBT BESTEHEN!
                                }
                                else if (squadTags.Contains(this.dicPlayerScore[kvp2.Key].tag) && this.dicPlayerScore[kvp2.Key].tag != "" && !KeepSquads.Contains(CheckSquad))
                                {

                                    KeepSquads.Add(this.dicPlayerScore[kvpCheck.Key].teamID + "." + this.dicPlayerScore[kvpCheck.Key].playerSquad);
                                    deletesquad = false;
                                    CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[kvpCheck.Key].teamID, this.dicPlayerScore[kvpCheck.Key].playerSquad, 0, 0, false);
                                    squadIDnew++;
                                    this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                                    DebugInfoSkill(string.Format("Clanmates detected, Clantag: ^b^2{0}^n^9. Best Player: ^2{1}", this.dicPlayerScore[kvp2.Key].tag, this.dicPlayerScore[kvpCheck.Key].playerName));
                                    //BLEIBT BESTEHEN!
                                }
                                else if (!KeepSquads.Contains(CheckSquad) && this.dicPlayerScore[kvp2.Key].tag != "")
                                {
                                    squadTags.Add(this.dicPlayerScore[kvp2.Key].tag);
                                }
                            }
                        }

                        if (deletesquad)
                        {

                            this.dicPlayerScore[kvpCheck.Key].playerSquad = 0;
                            squadless++;

                            if (!SWITCHplayer)
                            {
                                SWITCHplayer = true;
                                PlayerTeamA.Add(kvpCheck.Key);
                            }
                            else if (SWITCHplayer)
                            {
                                SWITCHplayer = false;
                                PlayerTeamB.Add(kvpCheck.Key);
                            }

                        }
                        else
                        {
                            this.dicSquadScore[squadIDnew].squadScore = squadvalue;
                            this.dicSquadScore[squadIDnew].squadsize = squadsize;

                            if (!SWITCHsquad)
                            {
                                SWITCHsquad = true;
                                PlayerTeamA.AddRange(squadplayers);
                                SquadsTeamA.Add(squadIDnew);
                            }
                            else if (SWITCHsquad)
                            {
                                SWITCHsquad = false;
                                PlayerTeamB.AddRange(squadplayers);
                                SquadsTeamB.Add(squadIDnew);
                            }

                        }
                    }


                }

            }

            int teamsizedifference = PlayerTeamA.Count - PlayerTeamB.Count;

            if (teamsizedifference >= 2)
            {
                DebugInfoSkill("Before. ^bTeamSizeA: " + PlayerTeamA.Count + "^n --- TeamSizeB: " + PlayerTeamB.Count);
                int fromsize = PlayerTeamA.Count - 1;
                for (int j = fromsize; j >= 0; j--)
                {
                    if (this.dicPlayerScore[PlayerTeamA[j]].playerSquad == 0 && (PlayerTeamA.Count - PlayerTeamB.Count) >= 2)
                    {
                        DebugInfoSkill("Player moved to even Teams, 1->2: " + "[" + this.dicPlayerScore[PlayerTeamA[j]].tag + "]^b" + this.dicPlayerScore[PlayerTeamA[j]].playerName);
                        PlayerTeamB.Add(PlayerTeamA[j]);
                        PlayerTeamA.RemoveAt(j);
                    }

                    if ((PlayerTeamA.Count - PlayerTeamB.Count) < 2)
                    {
                        DebugInfoSkill("Done!");
                        break;
                    }
                }

                DebugInfoSkill("^2After. TeamSizeA: " + PlayerTeamA.Count + "^n --- TeamSizeB: " + PlayerTeamB.Count);

            }
            else if (teamsizedifference <= -2)
            {
                DebugInfoSkill("Before. TeamSizeA: " + PlayerTeamA.Count + "^n --- ^bTeamSizeB: " + PlayerTeamB.Count);
                int fromsize = PlayerTeamB.Count - 1;
                for (int j = fromsize; j >= 0; j--)
                {
                    if (this.dicPlayerScore[PlayerTeamB[j]].playerSquad == 0 && (PlayerTeamA.Count - PlayerTeamB.Count) <= -2)
                    {
                        DebugInfoSkill("Player moved to even Teams, 2->1: " + "[" + this.dicPlayerScore[PlayerTeamB[j]].tag + "]^b" + this.dicPlayerScore[PlayerTeamB[j]].playerName);
                        PlayerTeamA.Add(PlayerTeamB[j]);
                        PlayerTeamB.RemoveAt(j);
                    }

                    if ((PlayerTeamA.Count - PlayerTeamB.Count) > -2)
                    {
                        DebugInfoSkill("Done!");
                        break;
                    }
                }

                DebugInfoSkill("^2After. TeamSizeA: " + PlayerTeamA.Count + "^n --- TeamSizeB: " + PlayerTeamB.Count);

            }
            else
            {
                DebugInfoSkill("^2TeamSize A: " + PlayerTeamA.Count + "TeamSizeB: " + PlayerTeamB.Count);
            }



            double TeamValueA = 0;
            int TeamSizeA = PlayerTeamA.Count;
            double TeamValueB = 0;
            int TeamSizeB = PlayerTeamB.Count;

            foreach (int PlayerIDa in PlayerTeamA)
            {
                TeamValueA = TeamValueA + this.dicPlayerScore[PlayerIDa].playerValue;
            }
            foreach (int PlayerIDb in PlayerTeamB)
            {
                TeamValueB = TeamValueB + this.dicPlayerScore[PlayerIDb].playerValue;
            }

            double AverageTeamA = TeamValueA / TeamSizeA;
            double AverageTeamB = TeamValueB / TeamSizeB;
            double AverageDiff = AverageTeamA - AverageTeamB;

            this.DebugInfoSkill("SortValue before adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
            this.DebugInfoSkill("Average Difference before adjustment: ^b^2" + AverageDiff);


            bool adjusted = false;
            do
            {
                adjusted = true;
                double tempTeamValueA = 0;
                double tempTeamValueB = 0;
                double tempAverageTeamA = 0;
                double tempAverageTeamB = 0;
                double tempAverageDiff = 0;
                double adjustvalue = AverageDiff;
                int moveIDA = 0;
                int moveIDB = 0;

                foreach (int playerIDa in PlayerTeamA)
                {
                    foreach (int playerIDb in PlayerTeamB)
                    {
                        if (adjustvalue > 0 && this.dicPlayerScore[playerIDa].playerValue > this.dicPlayerScore[playerIDb].playerValue
                            && this.dicPlayerScore[playerIDa].playerSquad == 0 && this.dicPlayerScore[playerIDb].playerSquad == 0)
                        {
                            tempTeamValueA = TeamValueA - this.dicPlayerScore[playerIDa].playerValue + this.dicPlayerScore[playerIDb].playerValue;
                            tempTeamValueB = TeamValueB - this.dicPlayerScore[playerIDb].playerValue + this.dicPlayerScore[playerIDa].playerValue;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = playerIDa;
                                moveIDB = playerIDb;
                                adjusted = false;
                            }

                        }
                        else if (adjustvalue < 0 && this.dicPlayerScore[playerIDa].playerValue < this.dicPlayerScore[playerIDb].playerValue
                            && this.dicPlayerScore[playerIDa].playerSquad == 0 && this.dicPlayerScore[playerIDb].playerSquad == 0)
                        {
                            tempTeamValueA = TeamValueA - this.dicPlayerScore[playerIDa].playerValue + this.dicPlayerScore[playerIDb].playerValue;
                            tempTeamValueB = TeamValueB - this.dicPlayerScore[playerIDb].playerValue + this.dicPlayerScore[playerIDa].playerValue;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = playerIDa;
                                moveIDB = playerIDb;
                                adjusted = false;
                            }

                        }

                    }
                }

                if (!adjusted)
                {
                    PlayerTeamA.Remove(moveIDA);
                    PlayerTeamA.Add(moveIDB);
                    TeamValueA = TeamValueA - this.dicPlayerScore[moveIDA].playerValue + this.dicPlayerScore[moveIDB].playerValue;
                    AverageTeamA = TeamValueA / TeamSizeA;

                    PlayerTeamB.Remove(moveIDB);
                    PlayerTeamB.Add(moveIDA);
                    TeamValueB = TeamValueB - this.dicPlayerScore[moveIDB].playerValue + this.dicPlayerScore[moveIDA].playerValue;
                    AverageTeamB = TeamValueB / TeamSizeB;

                    AverageDiff = AverageTeamA - AverageTeamB;

                    this.DebugInfoSkill("Adjustment: " + "^0[" + this.dicPlayerScore[moveIDA].tag + "]^b" + this.dicPlayerScore[moveIDA].playerName + "^n^9/^2" + this.dicPlayerScore[moveIDA].playerValue + " ^9 <--> " + "^0[" + this.dicPlayerScore[moveIDB].tag + "]^b" + this.dicPlayerScore[moveIDB].playerName + "n^9/^2" + this.dicPlayerScore[moveIDB].playerValue);
                }

            } while (!adjusted);


            this.DebugInfoSkill("SortValue ^bafter^n PLAYER adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
            this.DebugInfoSkill("Average Difference ^bafter ^nPLAYER adjustment: ^b^2" + AverageDiff);


            bool Sortiert;
            do
            {
                Sortiert = true;
                for (int j = 0; j < (PlayerTeamA.Count - 1); j++)
                {
                    if (this.dicPlayerScore[PlayerTeamA[j]].playerValue < this.dicPlayerScore[PlayerTeamA[j + 1]].playerValue)
                    {
                        int temp = PlayerTeamA[j];
                        PlayerTeamA[j] = PlayerTeamA[j + 1];
                        PlayerTeamA[j + 1] = temp;
                        Sortiert = false;
                    }
                }
            } while (!Sortiert);

            do
            {
                Sortiert = true;
                for (int j = 0; j < (PlayerTeamB.Count - 1); j++)
                {
                    if (this.dicPlayerScore[PlayerTeamB[j]].playerValue < this.dicPlayerScore[PlayerTeamB[j + 1]].playerValue)
                    {
                        int temp = PlayerTeamB[j];
                        PlayerTeamB[j] = PlayerTeamB[j + 1];
                        PlayerTeamB[j + 1] = temp;
                        Sortiert = false;
                    }
                }
            } while (!Sortiert);


            int count1 = 1;
            foreach (int playerIDa in PlayerTeamA)
            {
                if (this.dicPlayerScore[playerIDa].playerSquad == 0)
                {
                    if (count1 == 1)
                    {
                        squadIDnew++;
                        this.dicPlayerScore[playerIDa].teamID = 100;
                        this.dicPlayerScore[playerIDa].playerSquad = 100 + squadIDnew;

                        CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[playerIDa].teamID, this.dicPlayerScore[playerIDa].playerSquad, 1, this.dicPlayerScore[playerIDa].playerValue, false);
                        this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                        SquadsTeamA.Add(squadIDnew);

                    }
                    else
                    {
                        this.dicPlayerScore[playerIDa].teamID = 100;
                        this.dicPlayerScore[playerIDa].playerSquad = 100 + squadIDnew;
                        this.dicSquadScore[squadIDnew].squadsize++;
                        this.dicSquadScore[squadIDnew].squadScore = this.dicSquadScore[squadIDnew].squadScore + this.dicPlayerScore[playerIDa].playerValue;
                    }

                    count1++;
                    if (count1 == 5)
                        count1 = 1;
                }
            }


            count1 = 1;
            foreach (int playerIDb in PlayerTeamB)
            {
                if (this.dicPlayerScore[playerIDb].playerSquad == 0)
                {
                    if (count1 == 1)
                    {
                        squadIDnew++;
                        this.dicPlayerScore[playerIDb].teamID = 200;
                        this.dicPlayerScore[playerIDb].playerSquad = 200 + squadIDnew;

                        CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[playerIDb].teamID, this.dicPlayerScore[playerIDb].playerSquad, 1, this.dicPlayerScore[playerIDb].playerValue, false);
                        this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                        SquadsTeamB.Add(squadIDnew);

                    }
                    else
                    {
                        this.dicPlayerScore[playerIDb].teamID = 200;
                        this.dicPlayerScore[playerIDb].playerSquad = 200 + squadIDnew;
                        this.dicSquadScore[squadIDnew].squadsize++;
                        this.dicSquadScore[squadIDnew].squadScore = this.dicSquadScore[squadIDnew].squadScore + this.dicPlayerScore[playerIDb].playerValue;
                    }

                    count1++;
                    if (count1 == 5)
                        count1 = 1;
                }
            }



            adjusted = false;
            bool squadadjust = false;
            do
            {
                adjusted = true;
                double tempTeamValueA = 0;
                double tempTeamValueB = 0;
                double tempAverageTeamA = 0;
                double tempAverageTeamB = 0;
                double tempAverageDiff = 0;
                double adjustvalue = AverageDiff;
                int moveIDA = 0;
                int moveIDB = 0;

                foreach (int squadIDA in SquadsTeamA)
                {
                    foreach (int squadIDB in SquadsTeamB)
                    {
                        if (adjustvalue > 0 && this.dicSquadScore[squadIDA].squadScore > this.dicSquadScore[squadIDB].squadScore && this.dicSquadScore[squadIDA].squadsize == this.dicSquadScore[squadIDB].squadsize)
                        {
                            tempTeamValueA = TeamValueA - this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize +
                                this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize;
                            tempTeamValueB = TeamValueB - this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize +
                                this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = squadIDA;
                                moveIDB = squadIDB;
                                adjusted = false;
                            }

                        }
                        else if (adjustvalue < 0 && this.dicSquadScore[squadIDA].squadScore < this.dicSquadScore[squadIDB].squadScore && this.dicSquadScore[squadIDA].squadsize == this.dicSquadScore[squadIDB].squadsize)
                        {
                            tempTeamValueA = TeamValueA - this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize +
                                this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize;
                            tempTeamValueB = TeamValueB - this.dicSquadScore[squadIDB].squadScore * this.dicSquadScore[squadIDB].squadsize +
                                this.dicSquadScore[squadIDA].squadScore * this.dicSquadScore[squadIDA].squadsize;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = squadIDA;
                                moveIDB = squadIDB;
                                adjusted = false;
                            }

                        }

                    }
                }



                if (!adjusted)
                {
                    squadadjust = true;
                    SquadsTeamA.Remove(moveIDA);
                    SquadsTeamA.Add(moveIDB);
                    TeamValueA = TeamValueA - this.dicSquadScore[moveIDA].squadScore * this.dicSquadScore[moveIDA].squadsize +
                                this.dicSquadScore[moveIDB].squadScore * this.dicSquadScore[moveIDB].squadsize;
                    AverageTeamA = TeamValueA / TeamSizeA;

                    SquadsTeamB.Remove(moveIDB);
                    SquadsTeamB.Add(moveIDA);
                    TeamValueB = TeamValueB - this.dicSquadScore[moveIDB].squadScore * this.dicSquadScore[moveIDB].squadsize +
                                this.dicSquadScore[moveIDA].squadScore * this.dicSquadScore[moveIDA].squadsize;
                    AverageTeamB = TeamValueB / TeamSizeB;

                    AverageDiff = AverageTeamA - AverageTeamB;

                    this.DebugInfoSkill("SQUAD Adjustment: ^b^2 1." + moveIDA + " ^9<-->^2 2." + moveIDB);
                }

            } while (!adjusted);

            if (squadadjust)
            {
                this.DebugInfoSkill("SortValue ^bafter^n SQUAD adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                    "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
                this.DebugInfoSkill("Average Difference ^bafter ^nSQUAD adjustment: ^b^2" + AverageDiff);
            }



            int squadscrambledA = 0;
            int squadscrambledB = 0;

            Dictionary<string, int> dicNewSquad = new Dictionary<string, int>();

            foreach (int SquadIDA in SquadsTeamA)
            {
                foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == this.dicSquadScore[SquadIDA].teamID && this.dicPlayerScore[kvp.Key].playerSquad == this.dicSquadScore[SquadIDA].squadID && !this.dicPlayerScore[kvp.Key].balanced)
                    {
                        string strTeamSquad = this.dicSquadScore[SquadIDA].teamID + "." + this.dicSquadScore[SquadIDA].squadID;

                        if (dicNewSquad.ContainsKey(strTeamSquad))
                        {
                            this.dicPlayerScore[kvp.Key].playerSquad = dicNewSquad[strTeamSquad];
                        }
                        else
                        {
                            squadscrambledA++;
                            dicNewSquad.Add(strTeamSquad, squadscrambledA);
                            this.dicPlayerScore[kvp.Key].playerSquad = squadscrambledA;
                        }

                        this.dicPlayerScore[kvp.Key].teamID = 1;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                }

            }

            foreach (int SquadIDB in SquadsTeamB)
            {
                foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == this.dicSquadScore[SquadIDB].teamID && this.dicPlayerScore[kvp.Key].playerSquad == this.dicSquadScore[SquadIDB].squadID && !this.dicPlayerScore[kvp.Key].balanced)
                    {
                        string strTeamSquad = this.dicSquadScore[SquadIDB].teamID + "." + this.dicSquadScore[SquadIDB].squadID;

                        if (dicNewSquad.ContainsKey(strTeamSquad))
                        {
                            this.dicPlayerScore[kvp.Key].playerSquad = dicNewSquad[strTeamSquad];
                        }
                        else
                        {
                            squadscrambledB++;
                            dicNewSquad.Add(strTeamSquad, squadscrambledB);
                            this.dicPlayerScore[kvp.Key].playerSquad = squadscrambledB;
                        }

                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                }

            }

            string DebugSortedList = "";
            strTeam1 = "";
            strTeam2 = "";
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].teamID == 1)
                {
                    strTeam1 = strTeam1 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
                else if (this.dicPlayerScore[kvp.Key].teamID == 2)
                {
                    strTeam2 = strTeam2 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
            }

            DebugSortedList = "\n\nAfter Scramble:\nTeam 1: " + strTeam1 + "\n\nTeam 2: " + strTeam2;
            this.DebugInfoSkill(DebugSortedList);


            this.dicSquadList.Clear();
            this.strFinalSquad = "";
            this.intSquadA = 0;
            this.intSquadB = 0;

            this.DebugInfoSkill("Keeping only CLAN-Squads");

            // KEEP CLAN SQUADS END


        }

        public void KeepNoSquads()
        {
            //KEEP NO SQUADS START
            string DebugScoreList = "";
            string strTeam1 = "";
            string strTeam2 = "";

            int intTeamA = 0;
            int intTeamB = 0;
            int squadIDnew = 0;
            List<string> KeepSquads = new List<string>();
            List<string> squadTags = new List<string>();

            List<int> PlayerTeamA = new List<int>();
            List<int> PlayerTeamB = new List<int>();
            List<int> SquadsTeamA = new List<int>();
            List<int> SquadsTeamB = new List<int>();


            List<int> toremoveKeys = new List<int>();
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].teamID == 1)
                {
                    strTeam1 = strTeam1 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
                else if (this.dicPlayerScore[kvp.Key].teamID == 2)
                {
                    strTeam2 = strTeam2 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }


                if (this.dicPlayerCache.ContainsKey(this.dicPlayerScore[kvp.Key].playerName))
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == 1)
                        intTeamA++;
                    if (this.dicPlayerScore[kvp.Key].teamID == 2)
                        intTeamB++;
                }
                else
                {
                    toremoveKeys.Add(kvp.Key);
                }
            }


            foreach (int removeKey in toremoveKeys)
            {
                this.dicPlayerScore.Remove(removeKey);
            }

            DebugScoreList = "Before Scramble:\nTeam 1: " + strTeam1 + "\n\nTeam 2: " + strTeam2;
            this.DebugInfoSkill(DebugScoreList);




            bool SWITCHplayer = false;

            foreach (KeyValuePair<int, CPlayerScoreInf> kvpCheck in this.dicPlayerScore)
            {
                if (!SWITCHplayer)
                {
                    SWITCHplayer = true;
                    this.dicPlayerScore[kvpCheck.Key].teamID = 100;
                    this.dicPlayerScore[kvpCheck.Key].playerSquad = 0;
                    PlayerTeamA.Add(kvpCheck.Key);
                }
                else if (SWITCHplayer)
                {
                    SWITCHplayer = false;
                    this.dicPlayerScore[kvpCheck.Key].teamID = 200;
                    this.dicPlayerScore[kvpCheck.Key].playerSquad = 0;
                    PlayerTeamB.Add(kvpCheck.Key);
                }

            }

            int teamsizedifference = PlayerTeamA.Count - PlayerTeamB.Count;

            double TeamValueA = 0;
            int TeamSizeA = PlayerTeamA.Count;
            double TeamValueB = 0;
            int TeamSizeB = PlayerTeamB.Count;

            foreach (int PlayerIDa in PlayerTeamA)
            {
                TeamValueA = TeamValueA + this.dicPlayerScore[PlayerIDa].playerValue;
            }
            foreach (int PlayerIDb in PlayerTeamB)
            {
                TeamValueB = TeamValueB + this.dicPlayerScore[PlayerIDb].playerValue;
            }

            double AverageTeamA = TeamValueA / TeamSizeA;
            double AverageTeamB = TeamValueB / TeamSizeB;
            double AverageDiff = AverageTeamA - AverageTeamB;

            this.DebugInfoSkill("SortValue before adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
            this.DebugInfoSkill("Average Difference before adjustment: ^b^2" + AverageDiff);


            bool adjusted = false;
            do
            {
                adjusted = true;
                double tempTeamValueA = 0;
                double tempTeamValueB = 0;
                double tempAverageTeamA = 0;
                double tempAverageTeamB = 0;
                double tempAverageDiff = 0;
                double adjustvalue = AverageDiff;
                int moveIDA = 0;
                int moveIDB = 0;

                foreach (int playerIDa in PlayerTeamA)
                {
                    foreach (int playerIDb in PlayerTeamB)
                    {
                        if (adjustvalue > 0 && this.dicPlayerScore[playerIDa].playerValue > this.dicPlayerScore[playerIDb].playerValue
                            && this.dicPlayerScore[playerIDa].playerSquad == 0 && this.dicPlayerScore[playerIDb].playerSquad == 0)
                        {
                            tempTeamValueA = TeamValueA - this.dicPlayerScore[playerIDa].playerValue + this.dicPlayerScore[playerIDb].playerValue;
                            tempTeamValueB = TeamValueB - this.dicPlayerScore[playerIDb].playerValue + this.dicPlayerScore[playerIDa].playerValue;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = playerIDa;
                                moveIDB = playerIDb;
                                adjusted = false;
                            }

                        }
                        else if (adjustvalue < 0 && this.dicPlayerScore[playerIDa].playerValue < this.dicPlayerScore[playerIDb].playerValue
                            && this.dicPlayerScore[playerIDa].playerSquad == 0 && this.dicPlayerScore[playerIDb].playerSquad == 0)
                        {
                            tempTeamValueA = TeamValueA - this.dicPlayerScore[playerIDa].playerValue + this.dicPlayerScore[playerIDb].playerValue;
                            tempTeamValueB = TeamValueB - this.dicPlayerScore[playerIDb].playerValue + this.dicPlayerScore[playerIDa].playerValue;
                            tempAverageTeamA = tempTeamValueA / TeamSizeA;
                            tempAverageTeamB = tempTeamValueB / TeamSizeB;
                            tempAverageDiff = tempAverageTeamA - tempAverageTeamB;

                            if (Math.Abs(adjustvalue) > Math.Abs(tempAverageDiff))
                            {
                                adjustvalue = tempAverageDiff;
                                moveIDA = playerIDa;
                                moveIDB = playerIDb;
                                adjusted = false;
                            }

                        }

                    }
                }

                if (!adjusted)
                {
                    PlayerTeamA.Remove(moveIDA);
                    PlayerTeamA.Add(moveIDB);
                    TeamValueA = TeamValueA - this.dicPlayerScore[moveIDA].playerValue + this.dicPlayerScore[moveIDB].playerValue;
                    AverageTeamA = TeamValueA / TeamSizeA;

                    PlayerTeamB.Remove(moveIDB);
                    PlayerTeamB.Add(moveIDA);
                    TeamValueB = TeamValueB - this.dicPlayerScore[moveIDB].playerValue + this.dicPlayerScore[moveIDA].playerValue;
                    AverageTeamB = TeamValueB / TeamSizeB;

                    AverageDiff = AverageTeamA - AverageTeamB;

                    this.DebugInfoSkill("Adjustment: ^b^2" + this.dicPlayerScore[moveIDA].playerName + "/" + this.dicPlayerScore[moveIDA].playerValue + " ^9 <--> ^2" + this.dicPlayerScore[moveIDB].playerName + "/" + this.dicPlayerScore[moveIDB].playerValue);
                }

            } while (!adjusted);


            this.DebugInfoSkill("SortValue ^bafter^n adjustment: ^bTeam 1: ^7" + TeamSizeA + "^9*^2" + AverageTeamA +
                "^9^n --- ^bTeam 2: ^7" + TeamSizeB + "^9*^2" + AverageTeamB);
            this.DebugInfoSkill("Average Difference ^bafter ^nadjustment: ^b^2" + AverageDiff);

            int count1 = 1;
            foreach (int playerIDa in PlayerTeamA)
            {
                if (this.dicPlayerScore[playerIDa].playerSquad == 0)
                {
                    if (count1 == 1)
                    {
                        squadIDnew++;
                        this.dicPlayerScore[playerIDa].teamID = 100;
                        this.dicPlayerScore[playerIDa].playerSquad = 100 + squadIDnew;

                        CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[playerIDa].teamID, this.dicPlayerScore[playerIDa].playerSquad, 1, this.dicPlayerScore[playerIDa].playerValue, false);
                        this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                        SquadsTeamA.Add(squadIDnew);

                    }
                    else
                    {
                        this.dicPlayerScore[playerIDa].teamID = 100;
                        this.dicPlayerScore[playerIDa].playerSquad = 100 + squadIDnew;
                        this.dicSquadScore[squadIDnew].squadsize++;
                        this.dicSquadScore[squadIDnew].squadScore = this.dicSquadScore[squadIDnew].squadScore + this.dicPlayerScore[playerIDa].playerValue;
                    }

                    count1++;
                    if (count1 == 5)
                        count1 = 1;
                }
            }


            count1 = 1;
            foreach (int playerIDb in PlayerTeamB)
            {
                if (this.dicPlayerScore[playerIDb].playerSquad == 0)
                {
                    if (count1 == 1)
                    {
                        squadIDnew++;
                        this.dicPlayerScore[playerIDb].teamID = 200;
                        this.dicPlayerScore[playerIDb].playerSquad = 200 + squadIDnew;

                        CSquadScoreInf newEntrySquad = new CSquadScoreInf(this.dicPlayerScore[playerIDb].teamID, this.dicPlayerScore[playerIDb].playerSquad, 1, this.dicPlayerScore[playerIDb].playerValue, false);
                        this.dicSquadScore.Add(squadIDnew, newEntrySquad);
                        SquadsTeamB.Add(squadIDnew);

                    }
                    else
                    {
                        this.dicPlayerScore[playerIDb].teamID = 200;
                        this.dicPlayerScore[playerIDb].playerSquad = 200 + squadIDnew;
                        this.dicSquadScore[squadIDnew].squadsize++;
                        this.dicSquadScore[squadIDnew].squadScore = this.dicSquadScore[squadIDnew].squadScore + this.dicPlayerScore[playerIDb].playerValue;
                    }

                    count1++;
                    if (count1 == 5)
                        count1 = 1;
                }
            }


            int squadscrambledA = 0;
            int squadscrambledB = 0;
            int n = 0;

            Dictionary<string, int> dicNewSquad = new Dictionary<string, int>();

            foreach (int SquadIDA in SquadsTeamA)
            {
                foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == this.dicSquadScore[SquadIDA].teamID && this.dicPlayerScore[kvp.Key].playerSquad == this.dicSquadScore[SquadIDA].squadID && !this.dicPlayerScore[kvp.Key].balanced)
                    {
                        string strTeamSquad = this.dicSquadScore[SquadIDA].teamID + "." + this.dicSquadScore[SquadIDA].squadID;

                        if (dicNewSquad.ContainsKey(strTeamSquad))
                        {
                            this.dicPlayerScore[kvp.Key].playerSquad = dicNewSquad[strTeamSquad];
                        }
                        else
                        {
                            squadscrambledA++;
                            dicNewSquad.Add(strTeamSquad, squadscrambledA);
                            this.dicPlayerScore[kvp.Key].playerSquad = squadscrambledA;
                        }

                        this.dicPlayerScore[kvp.Key].teamID = 1;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                }

            }

            foreach (int SquadIDB in SquadsTeamB)
            {
                foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                {
                    if (this.dicPlayerScore[kvp.Key].teamID == this.dicSquadScore[SquadIDB].teamID && this.dicPlayerScore[kvp.Key].playerSquad == this.dicSquadScore[SquadIDB].squadID && !this.dicPlayerScore[kvp.Key].balanced)
                    {
                        string strTeamSquad = this.dicSquadScore[SquadIDB].teamID + "." + this.dicSquadScore[SquadIDB].squadID;

                        if (dicNewSquad.ContainsKey(strTeamSquad))
                        {
                            this.dicPlayerScore[kvp.Key].playerSquad = dicNewSquad[strTeamSquad];
                        }
                        else
                        {
                            squadscrambledB++;
                            dicNewSquad.Add(strTeamSquad, squadscrambledB);
                            this.dicPlayerScore[kvp.Key].playerSquad = squadscrambledB;
                        }

                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                }

            }


            n = 0;
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].playerSquad == 0)
                {
                    if (n == 0 && this.dicPlayerScore[kvp.Key].teamID != 2)
                    {
                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                    else if (n > 0 && n <= 2 && dicPlayerScore[kvp.Key].teamID != 1)
                    {
                        this.dicPlayerScore[kvp.Key].teamID = 1;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                    else if (n > 2 && this.dicPlayerScore[kvp.Key].teamID != 2)
                    {
                        this.dicPlayerScore[kvp.Key].teamID = 2;
                        this.dicPlayerScore[kvp.Key].balanced = true;
                    }
                    n++;
                    if (n == 5) n = 1;
                }
            }

            string DebugSortedList = "";
            strTeam1 = "";
            strTeam2 = "";
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].teamID == 1)
                {
                    strTeam1 = strTeam1 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
                else if (this.dicPlayerScore[kvp.Key].teamID == 2)
                {
                    strTeam2 = strTeam2 + "^1" + this.dicPlayerScore[kvp.Key].playerSquad + "^9.^0" + "[" + this.dicPlayerScore[kvp.Key].tag + "]^b" + this.dicPlayerScore[kvp.Key].playerName +
                    "^n^9: ^4" + this.dicPlayerScore[kvp.Key].playerValue + "^9 --- ";
                }
            }

            DebugSortedList = "\n\nAfter Scramble:\nTeam 1: " + strTeam1 + "\n\nTeam 2: " + strTeam2;
            this.DebugInfoSkill(DebugSortedList);


            this.dicSquadList.Clear();
            this.strFinalSquad = "";
            this.intSquadA = 0;
            this.intSquadB = 0;

            this.DebugInfoSkill("Keeping NO Squads");

            // KEEP NO SQUADS END
        }

        public double TBValue(double TBrank, double TBskill, double TBspm, double TBspmcombat, double TBkdr)
        {
            double _TBValue = TBrank * 5 + TBskill * 4 + TBspm + TBspmcombat * 8 + TBkdr * 500;

            return _TBValue;
        }



        #endregion
    }
}
