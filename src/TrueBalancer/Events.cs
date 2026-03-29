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
        #region ProconPluginInterface

        public override void OnRestartLevel()
        {
            DebugInfoSkill("^9Restart Round");
            this.intTicketcount = 123987123;
            this.boolLevelStart = false;
            this.boolLevelLoaded = false;
            this.boolneedbalance = false;
            this.boolstartBalance = false;
            this.intcountWarnings = 0;
            this.TSLevelStartWait = new TimeSpan();
            this.boolFirstOP = false;
            this.boolgametype = false;
            this.boolmanuellchange = false;
            this.boolTeamsScrambled = false;
            this.intScrambledPlayers = 0;
            this.teamswitcher.Clear();
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                if (this.dicPlayerCache[kvp.Key].playerWL == 0 && !dicPlayerCache[kvp.Key].IsCommander && !dicPlayerCache[kvp.Key].IsSpectator)
                {
                    this.dicPlayerCache[kvp.Key].tobebalanced = true;
                }
            }
        }

        public override void OnRunNextLevel()
        {
            DebugInfoSkill("^9Next Round");
            this.intTicketcount = 123987123;
            this.boolLevelStart = false;
            this.boolLevelLoaded = false;
            this.boolneedbalance = false;
            this.boolstartBalance = false;
            this.intcountWarnings = 0;
            this.TSLevelStartWait = new TimeSpan();
            this.boolFirstOP = false;
            this.boolgametype = false;
            this.boolmanuellchange = false;
            this.boolTeamsScrambled = false;
            this.intScrambledPlayers = 0;
            this.teamswitcher.Clear();
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                if (this.dicPlayerCache[kvp.Key].playerWL == 0 && !dicPlayerCache[kvp.Key].IsCommander && !dicPlayerCache[kvp.Key].IsSpectator)
                {
                    this.dicPlayerCache[kvp.Key].tobebalanced = true;
                }
            }
        }

        public virtual void OnReservedSlotsList(List<string> soldierNames)
        {


            this.strAWhitelistComplete = this.strAWhitelist;
            List<string> listWL = new List<string>(this.strAWhitelist);

            if (this.ynbWhitelist == enumBoolYesNo.Yes && this.ynbincludeVIPlist == enumBoolYesNo.Yes)
            {

                //this.DebugInfo ("reservedSlots");
                foreach (string name in soldierNames)
                {
                    listWL.Add(name);
                }
                this.strAWhitelistComplete = listWL.ToArray();
                string whitelist = "";
                foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
                {
                    if (((IList<string>)this.strAWhitelistComplete).Contains(kvp.Key))
                    {
                        whitelist = whitelist + kvp.Key + ", ";
                    }
                }

                //this.DebugInfo("Online VIPs: " + whitelist);  
            }
        }

        public virtual void OnPlayerMovedByAdmin(string soldierName, int destinationTeamId, int destinationSquadId, bool forceKilled)
        {
            this.boolscramblefailed = true;
            this.boolbalanced = true;
            // if (this.boolscrambleNow){
            // foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore){
            // if (this.dicPlayerScore[kvp.Key].playerName == soldierName){
            // this.dicPlayerScore[kvp.Key].scrambled = true;
            // DebugInfoSkill (soldierName + " --- MOVED: team= " + destinationTeamId.ToString() + ", squad= " + destinationSquadId.ToString());
            // break;
            // }
            // }
            // ScrambleNow();
            // }
        }

        public override void OnResponseError(List<string> lstRequestWords, string strError)
        {

            //DebugInfoSkill ("^3W: ^9" + strError);
            if (this.boolscrambleNow && this.strErrorMsg == "")
            {
                this.boolscramblefailed = true;
                this.strErrorMsg = strError;
                if (strError == "SetSquadFailed")
                {
                    DebugInfoSkill("^b^8ScrambleNow: SetSquadFailed");
                }
            }
        }

        public virtual void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal)
        {


            this.DTLevelLoaded = DateTime.Now;
            this.DebugInfo("^9New Level loaded");
            this.EndRoundSeconds = DateTime.Now - this.EndRoundTime;
            this.DebugInfoSkill("^9New Level loaded, after " + this.EndRoundSeconds.TotalSeconds.ToString("F2") + " seconds loading screen.");
            this.intTicketcount = 123987123;
            //this.boolRoundOver = false;
            this.strcurrentGametype = Gamemode;
            this.DebugInfo("^9Gamemode: " + strcurrentGametype);
            this.boolLevelLoaded = true;
            this.intCurrentRoundCount = roundsTotal - roundsPlayed;
            this.boolticketdif = false;
            this.boolbalanced = true;
            this.boolscramblebyadminroundend = false;
            this.OnCommandMove.Clear();
            this.OnCommandMoveDone.Clear();
            this.BalancedPlayers.Clear();

            if (this.boolscrambleActive)
            {
                this.DebugInfoSkill("^8^bScrambler active at Maploaded. STOP! Teams partly scrambled!");
                this.boolTeamsScrambled = true;
                this.intScrambledPlayers = 0;
                this.boolscrambleNow = false;
                this.boolscrambleActive = false;
                this.intScrambleCount = 0;
            }
        }

        public override void OnRoundOver(int iWinningTeamID)
        {

        }

        public virtual void OnRoundOverTeamScores(List<TeamScore> teamScores)
        {
            DebugInfoSkill("^9^bONROUNDOVERTEAMSCORES");

            this.EndRoundSeconds = new TimeSpan(0);
            this.EndRoundTime = DateTime.Now.AddSeconds(60);
            this.showfirstmove = false;

            int TeamAScore = 0;
            int TeamBScore = 0;

            this.intTicketsdif = 0;

            if (teamScores.Count == 1)
            {
                TeamAScore = teamScores[0].Score;
                TeamBScore = 0;
            }
            else if (teamScores.Count == 2)
            {
                TeamAScore = teamScores[0].Score;
                TeamBScore = teamScores[1].Score;
            }
            else
            {
                DebugInfoSkill("^8^bGamemode not Supported! Not scrambling.");
            }

            if (TeamAScore > TeamBScore) this.intTicketsdif = TeamAScore - TeamBScore;
            else if (TeamBScore > TeamAScore) this.intTicketsdif = TeamBScore - TeamAScore;
            else this.intTicketsdif = 0;

            /*
            if(this.ynbScrambleMap == enumBoolYesNo.Yes){
                this.boolticketdif = true;
                DebugInfoSkill("Scrambling on every new Map.");
            } */

            this.boolLevelStart = false;
            this.boolLevelLoaded = false;
            this.boolneedbalance = false;
            this.boolstartBalance = false;
            this.intcountWarnings = 0;
            this.TSLevelStartWait = new TimeSpan();
            this.boolFirstOP = false;
            this.boolgametype = false;
            this.boolmanuellchange = false;
            this.boolTeamsScrambled = false;
            this.intScrambledPlayers = 0;
            this.teamswitcher.Clear();

            /*
            this.skillA = 0;
            this.skillB = 0;
            
            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in dicPlayerCache){
                if (dicPlayerCache[kvp.Key].teamID == 1){
                    skillA = skillA + dicPlayerCache[kvp.Key].spm;
                }else if (dicPlayerCache[kvp.Key].teamID == 2){
                    skillB = skillB + dicPlayerCache[kvp.Key].spm;
                }
            }
            
            this.skillA = Math.Round(this.skillA/this.TeamA, 2);
            this.skillB = Math.Round(this.skillB/this.TeamB, 2);
            
            string strSKILL = "TEAM1: " + skillA.ToString() + " --- " + "TEAM2: " + skillB.ToString();
            
            DebugInfoSkill("Ticketdifference at roundend: " + this.intTicketsdif.ToString());
            this.DebugInfo("Round Over, Roundnumber: " + this.intCurrentRoundCount.ToString());  
            DebugInfoSkill("SKILL: " + strSKILL);
            
            */

            this.DebugInfo("^9Round Over, Roundnumber: ^b" + this.intCurrentRoundCount.ToString());
            this.DebugInfoSkill("^bTicketdifference at roundend: ^3" + this.intTicketsdif.ToString());
            this.DebugInfoSkill("^9Team 1 Data: ^7TBValue: ^b" + this.TBvalueA.ToString("F1") + "^n^0 - Rank: ^b" + this.rankA.ToString("F2") + "^n^1 - Skill: ^b" + this.skillA.ToString("F2") + "^n^2 - SPM: ^b" + this.spmA.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.spmcombatA.ToString("F2") + "^n^4 - K/D: ^b" + this.kdrA.ToString("F2"));
            this.DebugInfoSkill("^9Team 2 Data: ^7TBValue: ^b" + this.TBvalueB.ToString("F1") + "^n^0 - Rank: ^b" + this.rankB.ToString("F2") + "^n^1 - Skill: ^b" + this.skillB.ToString("F2") + "^n^2 - SPM: ^b" + this.spmB.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.spmcombatB.ToString("F2") + "^n^4 - K/D: ^b" + this.kdrB.ToString("F2"));



            if (this.boolscramblebyadminroundend && this.strcurrentGametype != "SquadDeathMatch0")
            {
                DebugInfoSkill("^9" + this.strcurrentGametype + ": ^b^2Scrambling on roundend requested by an admin!");
                this.boolscramblebyadminroundend = false;
                this.boolfirstscrambler = true;
                this.boolscramblefailed = false;
                this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
            }
            else if ((this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger0") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink")) && this.ynbenableSkillCONQUEST == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("^9CQ - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapCONQUEST == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("^9Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByCONQUEST);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else if ((this.intwonTicketsCONQUEST * this.intTicketcount / 100) <= this.intTicketsdif && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundCONQUEST == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("Won with to much tickets^b --> ^1Starting scrambler! ^nScrambling by: ^b^4" + this.ScrambleByCONQUEST);
                    if (this.ynbScrambleEveryRoundCONQUEST == enumBoolYesNo.Yes)
                    {
                        DebugInfoSkill("Ticketdifference checked every new round");
                    }
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else
                {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            else if (this.strcurrentGametype.Contains("Domination0") && this.ynbenableSkillDOM == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("^9CQ - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapDOM == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("^9Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByDOM);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else if ((this.intwonTicketsDOM * this.intTicketcount / 100) <= this.intTicketsdif && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundDOM == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("Won with to much tickets^b --> ^1Starting scrambler! ^nScrambling by: ^b^4" + this.ScrambleByDOM);
                    if (this.ynbScrambleEveryRoundDOM == enumBoolYesNo.Yes)
                    {
                        DebugInfoSkill("Ticketdifference checked every new round");
                    }
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else
                {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            else if (this.strcurrentGametype.Contains("Obliteration") && this.ynbenableSkillOB == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("^9OB - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapOB == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("^9Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByOB);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else if ((this.intwonTicketsOB * this.intTicketcount / 100) <= this.intTicketsdif && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundOB == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("Won with to much tickets^b --> ^1Starting scrambler! ^nScrambling by: ^b^4" + this.ScrambleByOB);
                    if (this.ynbScrambleEveryRoundOB == enumBoolYesNo.Yes)
                    {
                        DebugInfoSkill("Ticketdifference checked every new round");
                    }
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else
                {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            else if ((this.strcurrentGametype.Contains("Rush")) && this.ynbenableSkillRUSH == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("R - Gamemode: ^b" + this.strcurrentGametype);
                if (this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByRUSH);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else
                {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            else if ((this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag")) && this.ynbenableSkillGM == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("GM/CTF - Gamemode: ^b" + this.strcurrentGametype);
                if (this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByGM);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else
                {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            else if (this.strcurrentGametype.Contains("Elimination") && this.ynbenableSkillDF == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("DF - Gamemode: ^b" + this.strcurrentGametype);
                if (this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByDF);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else
                {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            //else if (this.strcurrentGametype.Contains("Obliteration") && this.ynbenableSkillOB == enumBoolYesNo.Yes)
            //{
            //    DebugInfoSkill("OB - Gamemode: ^b" + this.strcurrentGametype);
            //    if (this.intCurrentRoundCount == 1)
            //    {
            //        DebugInfoSkill("Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByOB);
            //        //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
            //        this.boolfirstscrambler = true;
            //        this.boolscramblefailed = false;
            //        this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
            //        // StartScrambler();
            //    }
            //    else
            //    {
            //        DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
            //    }
            //}
            else if (this.strcurrentGametype.Contains("TeamDeathMatch") && this.ynbenableSkillTDM == enumBoolYesNo.Yes)
            {
                DebugInfoSkill("TDM - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapTDM == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("Scramble needed ^bNEW MAP --> ^2Starting scrambler!^n Scrambling by:^b^4 " + this.ScrambleByTDM);
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else if ((this.intwonTicketsTDM * this.intTicketcount / 100) <= this.intTicketsdif && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundTDM == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("Won with to much tickets^b --> ^1Starting scrambler! ^nScrambling by: ^b^4" + this.ScrambleByTDM);
                    if (this.ynbScrambleEveryRoundTDM == enumBoolYesNo.Yes)
                    {
                        DebugInfoSkill("Ticketdifference checked every new round");
                    }
                    //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                    this.boolfirstscrambler = true;
                    this.boolscramblefailed = false;
                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1", "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                    // StartScrambler();
                }
                else
                {
                    DebugInfoSkill(this.strcurrentGametype + ": ^1^bNo reason to scramble!");
                }
            }
            else
            {
                DebugInfoSkill(this.strcurrentGametype + ": ^8^bGamemode not supported or EndRound-Scrambler turned off for this gamemode.");
            }


            //          if (!this.boolTeamsScrambled && this.intCurrentRoundCount == 1 && this.boolticketdif == true  && this.ynbenableSkill==enumBoolYesNo.Yes){
            /*          if (!this.boolTeamsScrambled && this.boolticketdif == true  && this.ynbenableSkill==enumBoolYesNo.Yes){
                            if(this.ynbScrambleMap == enumBoolYesNo.No && this.ynbScrambleEveryRound == enumBoolYesNo.Yes && this.strcurrentGametype != "squadrush0" && this.strcurrentGametype != "rushlarge0"){
                                this.boolticketdif = false;
                                DebugInfoSkill("Won with to much tickets --> Starting scrambler!");
                                //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                                if (strcurrentGametype != "squaddeathmatch0"){
                                    this.boolfirstscrambler = true;
                                    this.boolscramblefailed = false;
                                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1",  "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                                    // StartScrambler();
                                }else if (strcurrentGametype != ""){
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "SkillBalancer:  Still no data or GameMode not supported: " + "I" + strcurrentGametype + "I" );
                                }
                            }else if (this.intCurrentRoundCount == 1) {
                                this.boolticketdif = false;
                                DebugInfoSkill("Scramble needed NEW ROUND --> Starting scrambler!");
                                //this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                                if (strcurrentGametype != "squaddeathmatch0"){
                                    this.boolfirstscrambler = true;
                                    this.boolscramblefailed = false;
                                    this.ExecuteCommand("procon.protected.tasks.add", "WaitScrambleTimer", "49", "1", "1",  "procon.protected.plugins.call", "TrueBalancer", "StartScrambler");
                                    // StartScrambler();
                                }else if (strcurrentGametype != ""){
                                    this.ExecuteCommand("procon.protected.pluginconsole.write", "SkillBalancer:  Still no data or GameMode not supported: " + "I" + strcurrentGametype + "I" );
                                }
                            }else{
                                DebugInfoSkill("Won with to much tickets BUT same map again.");
                            }
                        }
                        */

            this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");

        }

        public virtual void OnRoundOverPlayers(List<CPlayerInfo> lstPlayers)
        {

            foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
            {
                if (this.dicPlayerCache[kvp.Key].playerWL != 1 && !dicPlayerCache[kvp.Key].IsCommander && !dicPlayerCache[kvp.Key].IsSpectator)
                {
                    this.dicPlayerCache[kvp.Key].tobebalanced = true;
                }
            }

            int i = 1;
            this.dicPlayerScore.Clear();
            this.dicSquadScore.Clear();
            this.bestSquadTeamID = 0;

            foreach (CPlayerInfo cpiPlayer in lstPlayers)
            {
                double value = 0;
                string tag = "";
                if (this.dicPlayerCache.ContainsKey(cpiPlayer.SoldierName))
                {
                    value = this.dicPlayerCache[cpiPlayer.SoldierName].playerValue;
                    tag = this.dicPlayerCache[cpiPlayer.SoldierName].tag;
                }
                CPlayerScoreInf newEntry = new CPlayerScoreInf(cpiPlayer.SoldierName, cpiPlayer.TeamID, cpiPlayer.SquadID, value, false, false, tag);
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


            // bool Sortiert;
            // do{
            // Sortiert = true; 
            // for (int j = 1; j < this.dicPlayerScore.Count; j++) {
            // if (this.dicPlayerScore[j].playerScore < this.dicPlayerScore[j+1].playerScore){ 
            // CPlayerScoreInf temp = new CPlayerScoreInf(this.dicPlayerScore[j].playerName, this.dicPlayerScore[j].teamID, this.dicPlayerScore[j].playerSquad, this.dicPlayerScore[j].playerScore, false, false);
            // this.dicPlayerScore[j] = this.dicPlayerScore[j+1];
            // this.dicPlayerScore[j+1] = temp;
            // Sortiert = false;
            // }
            // }  
            // } while (!Sortiert);

        }

        public virtual void OnPlayerSpawned(string soldierName, Inventory spawnedInventory)
        {

            if (this.boolLevelStart == false && this.boolLevelLoaded)
            {
                this.boolmessagesent = false;
                this.boolfirstscrambler = false;
                this.boolLevelLoaded = false;
                this.boolLevelStart = true;
                this.DebugInfo("First Spawner: ^bRound started!");
                this.DebugInfoGuard("First Spawner: ^bRound started!");
                this.DTLevelStart = DateTime.Now;
            }

            if (this.dicPlayerCache.ContainsKey(soldierName))
            {
                this.dicPlayerCache[soldierName].tobebalanced = false;
            }
            //string msg = soldierName + " spawned.";
            //this.ExecuteCommand("procon.protected.pluginconsole.write", msg);
        }

        public override void OnLogin()
        {
            this.DebugInfo("=================OnLogin=============");
            //this.boolOnLogin = true;
            this.boolfirstwarningWL = false;
            this.m_isPluginEnabled = true;
            this.RegisterAllCommands();
            //this.boolOnLogin = true;
            this.boolLevelStart = false;
            this.boolLevelLoaded = true;
            this.boolFirstOP = false;
            this.boolgametype = false;
            this.DTLevelStart = new DateTime();
            this.boolnoplayer = false;
            this.intScrambledPlayers = 0;
            this.boolscrambleNow = false;
            this.boolscrambleActive = false;
            this.intTicketsdif = -1;
            this.boolTeamsScrambled = false;
            this.boolticketdif = false;
            this.DTScramblestarted = new DateTime();
            this.boolRunOnList = false;

        }

        public override void OnServerInfo(CServerInfo csiServerInfo)
        {

            //DebugInfo(csiServerInfo.GameMod.ToString());
            //DebugInfo(csiServerInfo.GameMode.ToString());

            //this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n " + csiServerInfo.BlazeGameState);
            //this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n " + csiServerInfo.BlazePlayerCount);

            if (Servertype == "AUTOMATIC")
            {
                if (string.IsNullOrEmpty(csiServerInfo.BlazeGameState))
                {
                    DebugInfoSkill("BF3 detected");
                    Servertype = "BF3";
                }
                else
                {
                    DebugInfoSkill("BF4 detected");
                    Servertype = "BF4";
                }
            }

            this.strcurrentGametype = csiServerInfo.GameMode;
            this.intMaxSlots = csiServerInfo.MaxPlayerCount;
            this.intCurrentRoundCount = csiServerInfo.TotalRounds - csiServerInfo.CurrentRound;

            if (intCurrentRoundCount < 1)
            {
                intCurrentRoundCount = 1;
            }

            this.intScoreTeamA = 0;
            this.intScoreTeamB = 0;
            //DebugInfoSkill("Before Check: " + this.intTicketcount + ", GameMode: " + this.strcurrentGametype);

            if (this.strcurrentGametype.Contains("Elimination"))
            {
                this.intScoreTeamA = 10000;
                this.intScoreTeamB = 10000;
            }
            else if (this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
            {
                this.intScoreTeamA = 10000;
                this.intScoreTeamB = 10000;
            }
            else
            {
                foreach (TeamScore score in csiServerInfo.TeamScores)
                {
                    if (score.TeamID == 1)
                    {
                        this.intScoreTeamA = score.Score;
                        if (this.intTicketcount == 123987123 && this.strcurrentGametype.Contains("TeamDeathMatch"))
                        {
                            this.intTicketcount = score.WinningScore;
                        }
                    }
                    else if (score.TeamID == 2)
                    {
                        this.intScoreTeamB = score.Score;
                    }
                }
            }


            if ((this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger0") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink")) && this.boolLevelStart)
            {
                if (this.intTicketcount == 123987123)
                {
                    if (this.intScoreTeamA > this.intScoreTeamB) this.intTicketcount = this.intScoreTeamB;
                    else this.intTicketcount = this.intScoreTeamA;
                }
                this.intminScore = intminScoreCONQUEST;
                if ((this.TeamA + this.TeamB) < this.intTreshCONQUEST)
                {
                    this.intAllowDif = this.intAllowDif1CONQUEST;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2CONQUEST;
                }
            }

            if (this.strcurrentGametype.Contains("Domination0") && this.boolLevelStart)
            {
                if (this.intTicketcount == 123987123)
                {
                    if (this.intScoreTeamA > this.intScoreTeamB) this.intTicketcount = this.intScoreTeamB;
                    else this.intTicketcount = this.intScoreTeamA;
                }
                this.intminScore = intminScoreDOM;
                if ((this.TeamA + this.TeamB) < this.intTreshDOM)
                {
                    this.intAllowDif = this.intAllowDif1DOM;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2DOM;
                }
            }

            else if (this.strcurrentGametype.Contains("Obliteration") && this.boolLevelStart)
            {
                if (this.intTicketcount == 123987123)
                {
                    if (this.intScoreTeamA > this.intScoreTeamB) this.intTicketcount = this.intScoreTeamB;
                    else this.intTicketcount = this.intScoreTeamA;
                }
                this.intminScore = intminScoreOB;
                if ((this.TeamA + this.TeamB) < this.intTreshOB)
                {
                    this.intAllowDif = this.intAllowDif1OB;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2OB;
                }
            }

            else if ((this.strcurrentGametype.Contains("Rush")) && this.boolLevelStart)
            {
                if (this.intTicketcount == 123987123)
                {
                    if (this.intScoreTeamA > this.intScoreTeamB) this.intTicketcount = this.intScoreTeamB;
                    else this.intTicketcount = this.intScoreTeamA;
                }
                this.intminScore = intminScoreRUSH;

                if ((this.TeamA + this.TeamB) < this.intTreshRUSH)
                {
                    this.intAllowDif = this.intAllowDif1RUSH;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2RUSH;
                }
            }
            //else if (this.strcurrentGametype.Contains("GunMaster") && this.boolLevelStart)
            else if ((this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0")) && this.boolLevelStart)
            {
                this.intminScore = 0;
                if ((this.TeamA + this.TeamB) < this.intTreshGM)
                {
                    this.intAllowDif = this.intAllowDif1GM;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2GM;
                }
            }
            else if ((this.strcurrentGametype.Contains("Elimination")) && this.boolLevelStart)
            {
                this.intminScore = 0;
                if ((this.TeamA + this.TeamB) < this.intTreshDF)
                {
                    this.intAllowDif = this.intAllowDif1DF;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2DF;
                }
            }
            else if (this.strcurrentGametype.Contains("TeamDeathMatch") && this.boolLevelStart)
            {
                this.intminScore = this.intTicketcount - intminScoreTDM;
                if ((this.TeamA + this.TeamB) < this.intTreshTDM)
                {
                    this.intAllowDif = this.intAllowDif1TDM;
                }
                else
                {
                    this.intAllowDif = this.intAllowDif2TDM;
                }
            }

            int intticketdiffrence = 0;
            //DebugInfoSkill("TicketCount: " + this.intTicketcount);
            if (this.intScoreTeamA > this.intScoreTeamB) intticketdiffrence = this.intScoreTeamA - this.intScoreTeamB;
            else if (this.intScoreTeamA < this.intScoreTeamB) intticketdiffrence = this.intScoreTeamB - this.intScoreTeamA;

            if (!this.boolmessagesent && (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger0") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink")) && this.ynbenableSkillCONQUEST == enumBoolYesNo.Yes && (this.intScoreTeamA < 40 || this.intScoreTeamB < 40))
            {
                this.boolmessagesent = true;
                DebugInfoSkill("SI - CQ - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapCONQUEST == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("SI - Scrambling on every new map: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageCONQUEST != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageCONQUEST);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageCONQUEST, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageCONQUEST, "30");
                        }
                    }
                }
                else if ((this.intwonTicketsCONQUEST * this.intTicketcount / 100) <= intticketdiffrence && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundCONQUEST == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("SI - Won with too much tickets: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageCONQUEST != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageCONQUEST);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageCONQUEST, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageCONQUEST, "30");
                        }
                    }
                }
                else
                {
                    DebugInfoSkill("SI - " + this.strcurrentGametype + ":^b^1 No reason to scramble!");
                }
            }
            if (!this.boolmessagesent && this.strcurrentGametype.Contains("Domination0") && this.ynbenableSkillDOM == enumBoolYesNo.Yes && (this.intScoreTeamA < 40 || this.intScoreTeamB < 40))
            {
                this.boolmessagesent = true;
                DebugInfoSkill("SI - DOM - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapDOM == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("SI - Scrambling on every new map: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageDOM != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageDOM);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageDOM, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageDOM, "30");
                        }
                    }
                }
                else if ((this.intwonTicketsDOM * this.intTicketcount / 100) <= intticketdiffrence && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundDOM == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("SI - Won with too much tickets: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageDOM != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageDOM);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageDOM, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageDOM, "30");
                        }
                    }
                }
                else
                {
                    DebugInfoSkill("SI - " + this.strcurrentGametype + ":^b^1 No reason to scramble!");
                }
            }
            else if (!this.boolmessagesent && this.strcurrentGametype.Contains("Obliteration") && this.ynbenableSkillOB == enumBoolYesNo.Yes && (this.intScoreTeamA < 2 || this.intScoreTeamB < 2))
            {
                this.boolmessagesent = true;
                DebugInfoSkill("SI - OB - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapOB == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("SI - Scrambling on every new map: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageOB != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageOB);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageOB, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageOB, "30");
                        }
                    }
                }
                else if ((this.intwonTicketsOB * this.intTicketcount / 100) <= intticketdiffrence && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundOB == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("SI - Won with too much tickets: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageOB != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageOB);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageOB, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageOB, "30");
                        }
                    }
                }
                else
                {
                    DebugInfoSkill("SI - " + this.strcurrentGametype + ":^b^1 No reason to scramble!");
                }
            }
            else if (!this.boolmessagesent && this.strcurrentGametype.Contains("TeamDeathMatch") && this.ynbenableSkillTDM == enumBoolYesNo.Yes && (this.intScoreTeamA > (this.intTicketcount - 25) || this.intScoreTeamB > (this.intTicketcount - 25)))
            {
                this.boolmessagesent = true;
                DebugInfoSkill("SI - TDM - Gamemode: ^b" + this.strcurrentGametype);
                if (this.ynbScrambleMapTDM == enumBoolYesNo.Yes && this.intCurrentRoundCount == 1)
                {
                    DebugInfoSkill("SI - Scrambling on every new map: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageTDM != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageTDM);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageTDM, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes && !this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageTDM, "30");
                        }
                    }
                }
                else if ((this.intwonTicketsTDM * this.intTicketcount / 100) <= intticketdiffrence && (this.intCurrentRoundCount == 1 || this.ynbScrambleEveryRoundTDM == enumBoolYesNo.Yes))
                {
                    DebugInfoSkill("SI - Won with too much tickets: ^b" + this.strcurrentGametype);
                    if (this.strScrambleMessageTDM != "")
                    {
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL:^n say all - " + this.strScrambleMessageTDM);
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessageTDM, "all");
                        }
                        if (this.ynbYellScrambleMessage == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessageTDM, "30");
                        }
                    }
                }
                else
                {
                    DebugInfoSkill("SI - " + this.strcurrentGametype + ": ^b^1No reason to scramble!");
                }
            }

            /*
                if ((this.intScoreTeamA < 40 || this.intScoreTeamB < 40) && this.ynbenableSkill==enumBoolYesNo.Yes && this.strcurrentGametype != "squadrush0" && this.strcurrentGametype != "rushlarge0"){
    //              if ((this.intwonTickets <= intticketdiffrence || this.ynbScrambleMap == enumBoolYesNo.Yes)&& this.intCurrentRoundCount == 1){
                    if (this.intwonTickets <= intticketdiffrence || this.ynbScrambleMap == enumBoolYesNo.Yes){
                    
                        if (this.strScrambleMessage !="" && this.boolticketdif == false){
                            this.ExecuteCommand("procon.protected.send", "admin.say", this.strScrambleMessage , "all");
                            this.ExecuteCommand("procon.protected.send", "admin.yell", this.strScrambleMessage, "30");
                        }
                        this.boolticketdif = true;      
                    }
                }
            
            */


            this.DebugInfo("Serverinfo: ^1ScoreA: ^i" + this.intScoreTeamA.ToString() + "^n, ^2ScoreB: ^i" + this.intScoreTeamB.ToString() + "^n, ^9RoundCount: ^i " + this.intCurrentRoundCount.ToString());


            UpdateCheck();

        }

        private void UpdateCheck()
        {
            if (Check4Update == enumBoolYesNo.Yes)
            {
                try
                {
                    DateTime updatehelper = lastupdatecheck.AddHours(3);
                    if (DateTime.Compare(updatehelper, DateTime.Now) <= 0)
                    {
                        WebClient wc = new WebClient();
                        string latestversion = wc.DownloadString("https://forum.myrcon.com/showthread.php?7169");

                        latestversion = latestversion.Substring(latestversion.IndexOf("<title>") + 7);
                        latestversion = latestversion.Substring(0, latestversion.IndexOf("</title>"));
                        latestversion = latestversion.Substring(latestversion.IndexOf("TrueBalancer") + 13);

                        if (GetPluginVersion() != latestversion)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "TrueBalancer: ^b^2UPDATE " + latestversion + " AVAILABLE");
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "TrueBalancer: your version: " + GetPluginVersion());
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "TrueBalancer: latest version " + latestversion);
                        }
                        lastupdatecheck = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "[TB] ERROR checking for Update: " + ex);
                }
            }
        }

        public override void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset)
        {
            int numStatsFetch = this.intMaxPlayersToFetch;
            int numRemaining = lstPlayers.Count;

            if (lstPlayers.Count == 0 && this.boolnoplayer == false)
            {
                this.boolnoplayer = true;
                this.dicPlayerCache.Clear();
                this.DebugInfo("^3^b" + lstPlayers.Count.ToString() + " Players on the server.");
                return;
            }
            else if (lstPlayers.Count != 0 && this.boolnoplayer)
            {
                this.boolnoplayer = false;
            }
            else if (lstPlayers.Count == 0)
            {
                return; // suppress debugging messages for empty server
            }

            TimeSpan ScrambleTime = new TimeSpan(0);
            ScrambleTime = DateTime.Now - this.DTScramblestarted;
            if (ScrambleTime.TotalSeconds > 20 && this.boolscrambleActive)
            {
                this.DebugInfoSkill("^b^8Was not able to scramble teams in 15 seconds! Teams partly scrambled.");
                this.boolTeamsScrambled = true;
                this.intScrambledPlayers = 0;
                this.boolscrambleNow = false;
                this.boolscrambleActive = false;
                this.intScrambleCount = 0;
            }

            if (!this.boolscrambleActive)
            {
                List<String> removeFromCache = new List<String>();

                int numWithStats = 0;
                foreach (String k in this.dicPlayerCache.Keys)
                {
                    if (this.dicPlayerCache[k].statsFetched) ++numWithStats;
                }
                DebugInfoSkill("OnListPlayers: Players = " + lstPlayers.Count + " vs Known with stats = " + numWithStats);

                foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
                {
                    if (this.m_isPluginEnabled == false) break;

                    this.boolplayerexists = false;
                    foreach (CPlayerInfo cpiPlayer in lstPlayers)
                    {
                        if (this.m_isPluginEnabled == false) break;

                        if (cpiPlayer.SoldierName == kvp.Key)
                        {
                            boolplayerexists = true;
                            if (this.dicPlayerCache[kvp.Key].teamID != cpiPlayer.TeamID)
                            {
                                if (this.boolmanuellchange && this.dicPlayerCache[kvp.Key].teamID != 0 && kvp.Key != this.strMovedPlayer)
                                {

                                    this.dicPlayerCache[kvp.Key].Playerjoined = DateTime.Now;
                                    this.dicPlayerCache[kvp.Key].playerWL = 0;
                                    this.DebugInfo("Switched Manually!");

                                }
                                this.dicPlayerCache[kvp.Key].teamID = cpiPlayer.TeamID;

                            }
                            this.dicPlayerCache[kvp.Key].playerSquad = cpiPlayer.SquadID;
                            this.dicPlayerCache[kvp.Key].score = cpiPlayer.Score;


                            if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger0") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                            {
                                if (this.ScrambleByCONQUEST == "Rank")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }
                                else if (this.ScrambleByCONQUEST == "Skill")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }
                                else if (this.ScrambleByCONQUEST == "SPM")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }
                                else if (this.ScrambleByCONQUEST == "SPMcombat")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }
                                else if (this.ScrambleByCONQUEST == "K/D")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByCONQUEST == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            if (this.strcurrentGametype.Contains("Domination"))
                            {
                                if (this.ScrambleByDOM == "Rank")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }
                                else if (this.ScrambleByDOM == "Skill")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }
                                else if (this.ScrambleByDOM == "SPM")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }
                                else if (this.ScrambleByDOM == "SPMcombat")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }
                                else if (this.ScrambleByDOM == "K/D")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByDOM == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            else if (this.strcurrentGametype.Contains("Obliteration"))
                            {
                                if (this.ScrambleByOB == "Rank")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }
                                else if (this.ScrambleByOB == "Skill")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }
                                else if (this.ScrambleByOB == "SPM")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }
                                else if (this.ScrambleByOB == "SPMcombat")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }
                                else if (this.ScrambleByOB == "K/D")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByOB == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            else if (this.strcurrentGametype.Contains("Rush"))
                            {
                                if (this.ScrambleByRUSH == "Rank")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }
                                else if (this.ScrambleByRUSH == "Skill")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }
                                else if (this.ScrambleByRUSH == "SPM")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }
                                else if (this.ScrambleByRUSH == "SPMcombat")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }
                                else if (this.ScrambleByRUSH == "K/D")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByRUSH == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            else if (this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                            {
                                if (this.ScrambleByGM == "Rank")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }
                                else if (this.ScrambleByGM == "Skill")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }
                                else if (this.ScrambleByGM == "SPM")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }
                                else if (this.ScrambleByGM == "SPMcombat")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }
                                else if (this.ScrambleByGM == "K/D")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByGM == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            else if (this.strcurrentGametype.Contains("Elimination"))
                            {
                                if (this.ScrambleByDF == "Rank")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }
                                else if (this.ScrambleByDF == "Skill")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }
                                else if (this.ScrambleByDF == "SPM")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }
                                else if (this.ScrambleByDF == "SPMcombat")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }
                                else if (this.ScrambleByDF == "K/D")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByDF == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }
                            else if (this.strcurrentGametype.Contains("TeamDeathMatch"))
                            {
                                if (this.ScrambleByTDM == "Rank")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].rank;
                                }
                                else if (this.ScrambleByTDM == "Skill")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].skill;
                                }
                                else if (this.ScrambleByTDM == "SPM")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spm;
                                }
                                else if (this.ScrambleByTDM == "SPMcombat")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].spmcombat;
                                }
                                else if (this.ScrambleByTDM == "K/D")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].kdr;
                                }
                                else if (this.ScrambleByTDM == "TB-Value")
                                {
                                    this.dicPlayerCache[kvp.Key].playerValue = this.dicPlayerCache[kvp.Key].TBvalue;
                                }
                            }



                            if ((((IList<string>)this.strAWhitelistComplete).Contains(cpiPlayer.SoldierName) || (((IList<string>)this.strAClantagWhitelist).Contains(dicPlayerCache[cpiPlayer.SoldierName].tag) && dicPlayerCache[cpiPlayer.SoldierName].tag != String.Empty)) && this.ynbWhitelist == enumBoolYesNo.Yes)
                            {
                                this.dicPlayerCache[kvp.Key].playerWL = 1;
                            }

                            break;
                        }
                    }
                    if (boolplayerexists == false)
                    {
                        if (!removeFromCache.Contains(kvp.Key))
                        {
                            removeFromCache.Add(kvp.Key);
                        }
                        // FIXME this.dicPlayerCache.Remove(kvp.Key);
                        //PlayersOnServer.Remove(kvp.Key);
                    }
                }

                foreach (String s in removeFromCache)
                {
                    if (this.dicPlayerCache.ContainsKey(s))
                    {
                        this.dicPlayerCache.Remove(s);
                    }
                }


                foreach (CPlayerInfo cpiPlayer in lstPlayers)
                {
                    if (this.m_isPluginEnabled == false) break;

                    --numRemaining;

                    if (this.dicPlayerCache.ContainsKey(cpiPlayer.SoldierName) == true && this.dicPlayerCache[cpiPlayer.SoldierName].statsFetched)
                    {
                        if (this.dicPlayerCache[cpiPlayer.SoldierName].teamID != cpiPlayer.TeamID)
                        {
                            if (this.boolmanuellchange && this.dicPlayerCache[cpiPlayer.SoldierName].teamID != 0 && cpiPlayer.SoldierName != this.strMovedPlayer)
                            {

                                this.dicPlayerCache[cpiPlayer.SoldierName].Playerjoined = DateTime.Now;
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerWL = 0;
                                this.DebugInfo("Switched Manually!(2)");

                            }
                            this.dicPlayerCache[cpiPlayer.SoldierName].teamID = cpiPlayer.TeamID;

                        }
                        this.dicPlayerCache[cpiPlayer.SoldierName].playerSquad = cpiPlayer.SquadID;
                        this.dicPlayerCache[cpiPlayer.SoldierName].score = cpiPlayer.Score;

                        if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                        {
                            if (this.ScrambleByCONQUEST == "Rank")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }
                            else if (this.ScrambleByCONQUEST == "Skill")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }
                            else if (this.ScrambleByCONQUEST == "SPM")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }
                            else if (this.ScrambleByCONQUEST == "SPMcombat")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }
                            else if (this.ScrambleByCONQUEST == "K/D")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByCONQUEST == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        if (this.strcurrentGametype.Contains("Domination"))
                        {
                            if (this.ScrambleByDOM == "Rank")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }
                            else if (this.ScrambleByDOM == "Skill")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }
                            else if (this.ScrambleByDOM == "SPM")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }
                            else if (this.ScrambleByDOM == "SPMcombat")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }
                            else if (this.ScrambleByDOM == "K/D")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByDOM == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Obliteration"))
                        {
                            if (this.ScrambleByOB == "Rank")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }
                            else if (this.ScrambleByOB == "Skill")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }
                            else if (this.ScrambleByOB == "SPM")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }
                            else if (this.ScrambleByOB == "SPMcombat")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }
                            else if (this.ScrambleByOB == "K/D")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByOB == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Rush"))
                        {
                            if (this.ScrambleByRUSH == "Rank")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }
                            else if (this.ScrambleByRUSH == "Skill")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }
                            else if (this.ScrambleByRUSH == "SPM")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }
                            else if (this.ScrambleByRUSH == "SPMcombat")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }
                            else if (this.ScrambleByRUSH == "K/D")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByRUSH == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                        {
                            if (this.ScrambleByGM == "Rank")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }
                            else if (this.ScrambleByGM == "Skill")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }
                            else if (this.ScrambleByGM == "SPM")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }
                            else if (this.ScrambleByGM == "SPMcombat")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }
                            else if (this.ScrambleByGM == "K/D")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByGM == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Elimination"))
                        {
                            if (this.ScrambleByDF == "Rank")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }
                            else if (this.ScrambleByDF == "Skill")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }
                            else if (this.ScrambleByDF == "SPM")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }
                            else if (this.ScrambleByDF == "SPMcombat")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }
                            else if (this.ScrambleByDF == "K/D")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByDF == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("TeamDeathMatch"))
                        {
                            if (this.ScrambleByTDM == "Rank")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].rank;
                            }
                            else if (this.ScrambleByTDM == "Skill")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].skill;
                            }
                            else if (this.ScrambleByTDM == "SPM")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spm;
                            }
                            else if (this.ScrambleByTDM == "SPMcombat")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].spmcombat;
                            }
                            else if (this.ScrambleByTDM == "K/D")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].kdr;
                            }
                            else if (this.ScrambleByTDM == "TB-Value")
                            {
                                this.dicPlayerCache[cpiPlayer.SoldierName].playerValue = this.dicPlayerCache[cpiPlayer.SoldierName].TBvalue;
                            }
                        }



                        if ((((IList<string>)this.strAWhitelistComplete).Contains(cpiPlayer.SoldierName) || (((IList<string>)this.strAClantagWhitelist).Contains(dicPlayerCache[cpiPlayer.SoldierName].tag) && dicPlayerCache[cpiPlayer.SoldierName].tag != String.Empty)) && this.ynbWhitelist == enumBoolYesNo.Yes)
                        {
                            this.dicPlayerCache[cpiPlayer.SoldierName].playerWL = 1;
                        }
                    }
                    else
                    {
                        PlayerStats stats = new PlayerStats();
                        stats.reset();

                        /* If stats fetching is taking too long, skip for the rest of the current players list */

                        if (numStatsFetch > 0)
                        {
                            DebugInfoSkill("Starting Battlelog stats fetch for: ^b^5" + cpiPlayer.SoldierName);
                            DateTime startTime = DateTime.Now;

                            if (Servertype == "BF3")
                            {
                                stats = this.bclient.getPlayerStats(cpiPlayer.SoldierName, BattlelogClient.ServerType.BF3);
                            }
                            if (Servertype == "BF4")
                            {
                                stats = this.bclient.getPlayerStats(cpiPlayer.SoldierName, BattlelogClient.ServerType.BF4);
                            }

                            stats.statsFetched = true;

                            String name = (stats.tag != String.Empty) ? "[" + stats.tag + "]" + cpiPlayer.SoldierName : cpiPlayer.SoldierName;
                            DebugInfoSkill("^5^b" + name + "^n^9 stats fetched, rank is " + stats.rank + ", spm is " + stats.spm.ToString("F0") + ", ^b" + numRemaining + "^n players still need stats. ^2ELAPSED TIME: " + DateTime.Now.Subtract(startTime).TotalSeconds.ToString("F1") + " seconds");

                            --numStatsFetch;
                        }

                        double ValueTemp = 0;

                        double tempskill = stats.skill;

                        if (tempskill == 0)
                            tempskill = (this.skillA + this.skillB) / 2;
                        else if (tempskill < 0)
                            tempskill = 0;

                        double tempspm = stats.spm;
                        if (tempspm == 0)
                            tempspm = (this.spmA + this.spmB) / 2;

                        double tempspmcombat = stats.spmcombat;
                        if (tempspmcombat == 0)
                        {
                            tempspmcombat = (this.spmcombatA + this.spmcombatB) / 2;
                        }

                        double tempkdr = stats.kdr;
                        if (tempkdr == 0)
                            tempkdr = (this.kdrA + this.kdrB) / 2;


                        double TBvalueTemp = TBValue(stats.rank, tempskill, tempspm, tempspmcombat, tempkdr);

                        if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                        {
                            if (this.ScrambleByCONQUEST == "Rank")
                            {
                                ValueTemp = stats.rank;
                            }
                            else if (this.ScrambleByCONQUEST == "Skill")
                            {
                                ValueTemp = tempskill;
                            }
                            else if (this.ScrambleByCONQUEST == "SPM")
                            {
                                ValueTemp = tempspm;
                            }
                            else if (this.ScrambleByCONQUEST == "SPMcombat")
                            {
                                ValueTemp = tempspmcombat;
                            }
                            else if (this.ScrambleByCONQUEST == "K/D")
                            {
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByCONQUEST == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }
                        if (this.strcurrentGametype.Contains("Domination"))
                        {
                            if (this.ScrambleByDOM == "Rank")
                            {
                                ValueTemp = stats.rank;
                            }
                            else if (this.ScrambleByDOM == "Skill")
                            {
                                ValueTemp = tempskill;
                            }
                            else if (this.ScrambleByDOM == "SPM")
                            {
                                ValueTemp = tempspm;
                            }
                            else if (this.ScrambleByDOM == "SPMcombat")
                            {
                                ValueTemp = tempspmcombat;
                            }
                            else if (this.ScrambleByDOM == "K/D")
                            {
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByDOM == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Obliteration"))
                        {
                            if (this.ScrambleByCONQUEST == "Rank")
                            {
                                ValueTemp = stats.rank;
                            }
                            else if (this.ScrambleByOB == "Skill")
                            {
                                ValueTemp = tempskill;
                            }
                            else if (this.ScrambleByOB == "SPM")
                            {
                                ValueTemp = tempspm;
                            }
                            else if (this.ScrambleByOB == "SPMcombat")
                            {
                                ValueTemp = tempspmcombat;
                            }
                            else if (this.ScrambleByOB == "K/D")
                            {
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByOB == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Rush"))
                        {
                            if (this.ScrambleByRUSH == "Rank")
                            {
                                ValueTemp = stats.rank;
                            }
                            else if (this.ScrambleByRUSH == "Skill")
                            {
                                ValueTemp = tempskill;
                            }
                            else if (this.ScrambleByRUSH == "SPM")
                            {
                                ValueTemp = tempspm;
                            }
                            else if (this.ScrambleByRUSH == "SPMcombat")
                            {
                                ValueTemp = tempspmcombat;
                            }
                            else if (this.ScrambleByRUSH == "K/D")
                            {
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByRUSH == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                        {
                            if (this.ScrambleByGM == "Rank")
                            {
                                ValueTemp = stats.rank;
                            }
                            else if (this.ScrambleByGM == "Skill")
                            {
                                ValueTemp = tempskill;
                            }
                            else if (this.ScrambleByGM == "SPM")
                            {
                                ValueTemp = tempspm;
                            }
                            else if (this.ScrambleByGM == "SPMcombat")
                            {
                                ValueTemp = tempspmcombat;
                            }
                            else if (this.ScrambleByGM == "K/D")
                            {
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByGM == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Elimination"))
                        {
                            if (this.ScrambleByDF == "Rank")
                            {
                                ValueTemp = stats.rank;
                            }
                            else if (this.ScrambleByDF == "Skill")
                            {
                                ValueTemp = tempskill;
                            }
                            else if (this.ScrambleByDF == "SPM")
                            {
                                ValueTemp = tempspm;
                            }
                            else if (this.ScrambleByDF == "SPMcombat")
                            {
                                ValueTemp = tempspmcombat;
                            }
                            else if (this.ScrambleByDF == "K/D")
                            {
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByDF == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("TeamDeathMatch"))
                        {
                            if (this.ScrambleByTDM == "Rank")
                            {
                                ValueTemp = stats.rank;
                            }
                            else if (this.ScrambleByTDM == "Skill")
                            {
                                ValueTemp = tempskill;
                            }
                            else if (this.ScrambleByTDM == "SPM")
                            {
                                ValueTemp = tempspm;
                            }
                            else if (this.ScrambleByTDM == "SPMcombat")
                            {
                                ValueTemp = tempspmcombat;
                            }
                            else if (this.ScrambleByTDM == "K/D")
                            {
                                ValueTemp = tempkdr;
                            }
                            else if (this.ScrambleByTDM == "TB-Value")
                            {
                                ValueTemp = TBvalueTemp;
                            }
                        }

                        bool commander = cpiPlayer.Type == 1 || cpiPlayer.Type == 2 ? true : false;
                        bool spectator = cpiPlayer.Type == 3 ? true : false;
                        if ((((IList<string>)this.strAWhitelistComplete).Contains(cpiPlayer.SoldierName) || (((IList<string>)this.strAClantagWhitelist).Contains(stats.tag) && stats.tag != String.Empty)) && this.ynbWhitelist == enumBoolYesNo.Yes)
                        {

                            CPlayerJoinInf newEntry = new CPlayerJoinInf(cpiPlayer.TeamID, 1, cpiPlayer.SquadID, DateTime.Now, cpiPlayer.Score, stats.rank, tempskill, tempspm, tempspmcombat, tempkdr, TBvalueTemp, ValueTemp, stats.tag, false, commander, spectator);
                            newEntry.statsFetched = stats.statsFetched;
                            this.dicPlayerCache[cpiPlayer.SoldierName] = newEntry;
                            //this.dicPlayerCache.Add(cpiPlayer.SoldierName, newEntry);
                            //PlayersOnServer.Add(cpiPlayer.SoldierName);
                            //this.ExecuteCommand("procon.protected.pluginconsole.write", spm.ToString());
                        }
                        else
                        {
                            if (cpiPlayer.TeamID == 0)
                            {
                                CPlayerJoinInf newEntry = new CPlayerJoinInf(cpiPlayer.TeamID, 0, cpiPlayer.SquadID, DateTime.Now, cpiPlayer.Score, stats.rank, tempskill, tempspm, tempspmcombat, tempkdr, TBvalueTemp, ValueTemp, stats.tag, true, commander, spectator);
                                newEntry.statsFetched = stats.statsFetched;
                                this.dicPlayerCache[cpiPlayer.SoldierName] = newEntry;
                                //this.dicPlayerCache.Add(cpiPlayer.SoldierName, newEntry);
                                //PlayersOnServer.Add(cpiPlayer.SoldierName);
                            }
                            else
                            {
                                CPlayerJoinInf newEntry = new CPlayerJoinInf(cpiPlayer.TeamID, 0, cpiPlayer.SquadID, DateTime.Now, cpiPlayer.Score, stats.rank, tempskill, tempspm, tempspmcombat, tempkdr, TBvalueTemp, ValueTemp, stats.tag, false, commander, spectator);
                                newEntry.statsFetched = stats.statsFetched;
                                this.dicPlayerCache[cpiPlayer.SoldierName] = newEntry;
                                //this.dicPlayerCache.Add(cpiPlayer.SoldierName, newEntry);
                                //PlayersOnServer.Add(cpiPlayer.SoldierName);
                            }
                            //this.ExecuteCommand("procon.protected.pluginconsole.write", spm.ToString());
                        }

                    }
                }

                if (this.ynbWhitelist == enumBoolYesNo.Yes && this.ynbincludeVIPlist == enumBoolYesNo.Yes)
                {
                    this.ExecuteCommand("procon.protected.send", "reservedSlotsList.list");
                }


                this.strMovedPlayer = "";
                this.boolmanuellchange = false;
                this.DebugInfo("-------OP-------");


                string printSoldier = "";
                string strPlayerlist = "";
                string strPLTeam1 = "";
                string strPLTeam2 = "";
                string strPLNeutral = "";


                /*  Dictionary<string, CPlayerJoinInf> dicSorted = new Dictionary<string, CPlayerJoinInf>();
                
                dicSorted = from k in dicPlayerCache.Keys
                            orderby dicPlayerCache[k].Playerjoined ascending
                            select k; */

                Dictionary<string, CPlayerJoinInf> dicPlayerSorted = new Dictionary<string, CPlayerJoinInf>();

                string whitelisttemp = "^2";

                foreach (KeyValuePair<string, CPlayerJoinInf> kvp1 in this.dicPlayerCache)
                {
                    if (this.dicPlayerCache[kvp1.Key].playerWL == 1 || dicPlayerCache[kvp1.Key].IsCommander || dicPlayerCache[kvp1.Key].IsSpectator)
                    {
                        whitelisttemp = whitelisttemp + "^b" + kvp1.Key + "^n" + ", ";
                    }
                    // DateTime maxValueJoined = new DateTime();
                    double minpoints = 100000000;
                    KeyValuePair<string, CPlayerJoinInf> kvplastjoiner = new KeyValuePair<string, CPlayerJoinInf>();

                    foreach (KeyValuePair<string, CPlayerJoinInf> kvp2 in this.dicPlayerCache)
                    {
                        if (this.dicPlayerCache[kvp2.Key].score <= minpoints && dicPlayerSorted.ContainsKey(kvp2.Key) == false)
                        {
                            minpoints = this.dicPlayerCache[kvp2.Key].score;
                            kvplastjoiner = kvp2;
                        }
                    }
                    dicPlayerSorted.Add(kvplastjoiner.Key, kvplastjoiner.Value);
                }


                foreach (KeyValuePair<string, CPlayerJoinInf> kvp in dicPlayerSorted)
                {
                    printSoldier = kvp.Key.Replace("{", "(");
                    printSoldier = printSoldier.Replace("}", ")");
                    if (dicPlayerSorted[kvp.Key].teamID == 1)
                    {
                        strPLTeam1 = strPLTeam1 + "^0^n[" + dicPlayerSorted[kvp.Key].tag + "]^b" + printSoldier + "^n:^2" + Convert.ToString(dicPlayerSorted[kvp.Key].playerWL) +
                        "^0.^1" + Convert.ToString(dicPlayerSorted[kvp.Key].playerSquad) + " - ^i^3" + dicPlayerSorted[kvp.Key].TBvalue.ToString("F1") + "^0/^3" + dicPlayerSorted[kvp.Key].rank.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].skill.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].spm.ToString("F2") + "^0/^3" +
                        dicPlayerSorted[kvp.Key].spmcombat.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].kdr.ToString("F2") + "^b^4 -=- ";
                    }
                    else if (dicPlayerSorted[kvp.Key].teamID == 2)
                    {
                        strPLTeam2 = strPLTeam2 + "^0^n[" + dicPlayerSorted[kvp.Key].tag + "]^b" + printSoldier + "^n:^2" + Convert.ToString(dicPlayerSorted[kvp.Key].playerWL) +
                        "^0.^1" + Convert.ToString(dicPlayerSorted[kvp.Key].playerSquad) + " - ^i^3" + dicPlayerSorted[kvp.Key].TBvalue.ToString("F1") + "^0/^3" + dicPlayerSorted[kvp.Key].rank.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].skill.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].spm.ToString("F2") + "^0/^3" +
                        dicPlayerSorted[kvp.Key].spmcombat.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].kdr.ToString("F2") + "^b^4 -=- ";
                    }
                    else
                    {
                        strPLNeutral = strPLNeutral + "^0^n[" + dicPlayerSorted[kvp.Key].tag + "]^b" + printSoldier + "^n:^2" + Convert.ToString(dicPlayerSorted[kvp.Key].playerWL) +
                        "^0.^1" + Convert.ToString(dicPlayerSorted[kvp.Key].playerSquad) + " - ^i^3" + dicPlayerSorted[kvp.Key].TBvalue.ToString("F1") + "^0/^3" + dicPlayerSorted[kvp.Key].rank.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].skill.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].spm.ToString("F2") + "^0/^3" +
                        dicPlayerSorted[kvp.Key].spmcombat.ToString("F2") + "^0/^3" + dicPlayerSorted[kvp.Key].kdr.ToString("F2") + "^b^4 -=- ";
                    }
                }

                /*          foreach (KeyValuePair<string, CPlayerJoinInf> kvp in this.dicPlayerCache)
                            {
                                printSoldier = kvp.Key.Replace("{", "(");
                                printSoldier = printSoldier.Replace("}", ")");
                                if (this.dicPlayerCache[kvp.Key].teamID == 1){
                                    strPLTeam1 = strPLTeam1 + Convert.ToString(this.dicPlayerCache[kvp.Key].playerWL) + 
                                    "." + Convert.ToString(this.dicPlayerCache[kvp.Key].playerSquad) + "." + 
                                    this.dicPlayerCache[kvp.Key].Playerjoined.ToString("HH:mm:ss") + "-" + printSoldier + " -=- ";
                                }else if (this.dicPlayerCache[kvp.Key].teamID == 2){
                                    strPLTeam2 = strPLTeam2 + Convert.ToString(this.dicPlayerCache[kvp.Key].playerWL) + 
                                    "." + Convert.ToString(this.dicPlayerCache[kvp.Key].playerSquad) + "." + 
                                    this.dicPlayerCache[kvp.Key].Playerjoined.ToString("HH:mm:ss") + "-" + printSoldier + " -=- ";
                                }else{
                                    strPLNeutral = strPLNeutral + Convert.ToString(this.dicPlayerCache[kvp.Key].playerWL) + 
                                    "." + Convert.ToString(this.dicPlayerCache[kvp.Key].playerSquad) + "." + 
                                    this.dicPlayerCache[kvp.Key].Playerjoined.ToString("HH:mm:ss") + "-" + printSoldier + " -=- ";
                                }
                            } */

                strPlayerlist = "\n^b^4TEAM 1:^n " + strPLTeam1 + "\n\n^bTEAM 2:^n " + strPLTeam2 + "\n\n^bNeutral:^n " + strPLNeutral;

                this.DebugInfo(strPlayerlist);
                this.DebugInfo("Online whitelisted players: " + whitelisttemp);

                //this.DebugInfo("WaitSeconds: " + this.intWaitSeconds.ToString());


                this.TSLevelStartWait = DateTime.Now - this.DTLevelStart;
                if (this.boolLevelStart && this.TSLevelStartWait.TotalSeconds > 30)
                {
                    if (this.boolFirstOP == false)
                    {
                        this.boolFirstOP = true;
                    }

                    if (strcurrentGametype != "squaddeathmatch0")
                    {
                        // if ( this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore){
                        CompareTeams();
                        // } else {
                        // this.DebugInfo("Not comparing teams. Tickets till Endround: TeamA: " + this.intScoreTeamA + ", TeamB: " + this.intScoreTeamB  );
                        // }
                    }
                    else if (strcurrentGametype != "")
                    {
                        if (this.boolgametype == false)
                        {
                            this.boolgametype = true;
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "TrueBalancer:  Still no data or GameMode not supported: " + "I" + strcurrentGametype + "I");

                        }
                    }
                }
            }
            else
            {

                DebugInfoSkill("^3OnListPlayers, skill scramble in progress");

                double afterscrambleValueA = 0;
                double afterscrambleTeamSizeA = 0;
                double afterscrambleValueB = 0;
                double afterscrambleTeamSizeB = 0;
                if (this.boolscrambleNow && this.boolRunOnList)
                {
                    this.boolTeamsScrambled = true;
                    foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
                    {
                        if (this.m_isPluginEnabled == false) break;
                        foreach (CPlayerInfo cpiPlayer in lstPlayers)
                        {
                            if (this.m_isPluginEnabled == false) break;
                            if (this.dicPlayerScore[kvp.Key].playerName == cpiPlayer.SoldierName)
                            {
                                if (cpiPlayer.TeamID == this.dicPlayerScore[kvp.Key].teamID && cpiPlayer.SquadID == this.dicPlayerScore[kvp.Key].playerSquad)
                                {
                                    this.dicPlayerScore[kvp.Key].scrambled = true;
                                    if (cpiPlayer.TeamID == 1)
                                    {
                                        afterscrambleValueA = afterscrambleValueA + this.dicPlayerScore[kvp.Key].playerValue;
                                        afterscrambleTeamSizeA++;
                                    }
                                    else
                                    {
                                        afterscrambleValueB = afterscrambleValueB + this.dicPlayerScore[kvp.Key].playerValue;
                                        afterscrambleTeamSizeB++;
                                    }
                                }
                                else
                                {
                                    this.dicPlayerScore[kvp.Key].scrambled = false;
                                    this.boolTeamsScrambled = false;
                                    this.DebugInfoSkill("^3Not scrambled: ^b" + cpiPlayer.SoldierName + "^n. Is in: ^b" + cpiPlayer.TeamID.ToString() + "." + cpiPlayer.SquadID + "^n - Goal: ^b" + this.dicPlayerScore[kvp.Key].teamID.ToString() + "." + this.dicPlayerScore[kvp.Key].playerSquad.ToString());
                                }
                                break;
                            }
                        }
                    }
                    if (this.boolTeamsScrambled)
                    {
                        this.boolRunOnList = false;
                        this.DebugInfoSkill("^b^2Teams are scrambled now!");
                        this.DebugInfoSkill("Team 1 Value: ^b^2" + afterscrambleValueA / afterscrambleTeamSizeA + "^9*^7" + afterscrambleTeamSizeA + "^n^9 --- Team 2 Value: ^b^2" + afterscrambleValueB / afterscrambleTeamSizeB + "^9*^7" + afterscrambleTeamSizeB);
                        double valuediffendround = afterscrambleValueA / afterscrambleTeamSizeA - afterscrambleValueB / afterscrambleTeamSizeB;
                        this.DebugInfoSkill("ValueDifference: ^b^2" + valuediffendround);
                        TimeSpan ScrambleDuration = DateTime.Now - this.DTScramblestarted;
                        this.DebugInfoSkill("ScrambleDuration: " + ScrambleDuration.TotalSeconds.ToString("F2") + " seconds");
                        this.boolscrambleNow = false;
                        this.boolscrambleActive = false;
                        this.intScrambleCount = 0;
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
                    else
                    {
                        this.boolRunOnList = false;
                        ScrambleNow();
                    }

                }
                else
                {
                    DebugInfoSkill("^3Scrambler Active OnList!");
                }
            }

            /* Since the playername matching feature requires the lastest list of player names, we have
             * to re-register commands after every player list update, bleah!
             */
            this.RegisterAllCommands();
            DebugInfoSkill("OnListPlayers handler returning");
        }

        public virtual void OnPlayerSquadChange(string soldierName, int teamId, int squadId)
        {

            TimeSpan ScrambleTime = new TimeSpan(0);
            ScrambleTime = DateTime.Now - this.DTScramblestarted;

            if (!this.boolscrambleActive && ScrambleTime.TotalSeconds > 30)
            {
                if (this.boolLevelStart == true && !this.boolplayerleft)
                {
                    this.DebugInfo("player squad change: ^b" + soldierName + " ^n" + this.boolLevelStart.ToString());
                    this.dicPlayerCache[soldierName].playerSquad = squadId;
                    //this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
                }
            }
            if (this.boolplayerleft)
                this.boolplayerleft = false;
        }

        public override void OnPlayerTeamChange(string strSoldierName, int iTeamID, int iSquadID)
        {


            if (DateTime.Now > this.EndRoundTime && this.showfirstmove == false && this.dicPlayerCache[strSoldierName].teamID != 0)
            {
                TimeSpan firstmovetime = DateTime.Now - this.DTLevelLoaded;
                this.showfirstmove = true;
                DebugInfoSkill("First Player moved by the game: ^n" + firstmovetime.TotalSeconds.ToString("F2") + " ^nseconds after Level Loaded");
            }

            this.TSLevelStartWait = DateTime.Now - this.DTLevelStart;
            if (!this.boolscrambleActive)
            {

                if (this.boolLevelStart == true && this.boolFirstOP == true)
                {
                    if (!this.teamswitcher.Contains(strSoldierName))
                    {
                        if (this.dicPlayerCache[strSoldierName].teamID != 0)
                        {
                            this.DebugInfoGuard("^1^b*** TEAMSWITCHER ***: " + "^0^n[" + this.dicPlayerCache[strSoldierName].tag + "]^b" + strSoldierName + "^n^9--- From: ^0" + this.dicPlayerCache[strSoldierName].teamID.ToString() + "." + this.dicPlayerCache[strSoldierName].playerSquad.ToString() + "^9, to: ^0" + iTeamID.ToString());
                            // this.DebugInfoGuard("Rank: " + this.dicPlayerCache[strSoldierName].rank + " - Skill: " + this.dicPlayerCache[strSoldierName].skill + " - SPM: " + this.dicPlayerCache[strSoldierName].spm + " - SPMcombat: " + this.dicPlayerCache[strSoldierName].spmcombat + " - K/D: " + this.dicPlayerCache[strSoldierName].kdr);
                        }
                        else
                        {
                            this.DebugInfoGuard("^0*** New Player ***: " + "^0^n[" + this.dicPlayerCache[strSoldierName].tag + "]^b" + strSoldierName + "^n^9--- From: ^0" + this.dicPlayerCache[strSoldierName].teamID.ToString() + "." + this.dicPlayerCache[strSoldierName].playerSquad.ToString() + "^9, to: ^0" + iTeamID.ToString());
                            // this.DebugInfoGuard("Rank: " + this.dicPlayerCache[strSoldierName].rank + " - Skill: " + this.dicPlayerCache[strSoldierName].skill + " - SPM: " + this.dicPlayerCache[strSoldierName].spm + " - SPMcombat: " + this.dicPlayerCache[strSoldierName].spmcombat + " - K/D: " + this.dicPlayerCache[strSoldierName].kdr);
                        }
                    }

                    if (this.boolFirstOP == true)
                    {
                        this.boolmanuellchange = true;
                    }

                    if (this.ynbBalancingGuard == enumBoolYesNo.Yes && this.boolFirstOP && this.OnCommandMove.ContainsKey(strSoldierName) == false)
                    {

                        string sortBy = "";
                        double sortValueA = 0;
                        double sortValueB = 0;
                        double goodplayer = 0;
                        double badplayer = 0;
                        double dblValueDiff = 0;

                        if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                        {
                            dblValueDiff = this.dblValueDiffCONQUEST;

                            if (this.ScrambleByCONQUEST == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByCONQUEST == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByCONQUEST == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByCONQUEST == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByCONQUEST == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByCONQUEST == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }
                        if (this.strcurrentGametype.Contains("Domination"))
                        {
                            dblValueDiff = this.dblValueDiffDOM;

                            if (this.ScrambleByDOM == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByDOM == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByDOM == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByDOM == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByDOM == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByDOM == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Obliteration"))
                        {
                            dblValueDiff = this.dblValueDiffCONQUEST;

                            if (this.ScrambleByCONQUEST == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByCONQUEST == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByCONQUEST == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByCONQUEST == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByCONQUEST == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByCONQUEST == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Rush"))
                        {
                            dblValueDiff = this.dblValueDiffRUSH;

                            if (this.ScrambleByRUSH == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByRUSH == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByRUSH == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByRUSH == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByRUSH == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByRUSH == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                        {
                            dblValueDiff = this.dblValueDiffGM;

                            if (this.ScrambleByGM == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByGM == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByGM == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByGM == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByGM == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByGM == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("Elimination"))
                        {
                            dblValueDiff = this.dblValueDiffRUSH;

                            if (this.ScrambleByDF == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByDF == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByDF == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByDF == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByDF == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByDF == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }
                        else if (this.strcurrentGametype.Contains("TeamDeathMatch"))
                        {
                            dblValueDiff = this.dblValueDiffTDM;

                            if (this.ScrambleByTDM == "TB-Value")
                            {
                                sortBy = "TBValue";
                                sortValueA = this.TBvalueA;
                                sortValueB = this.TBvalueB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].TBvalue;
                            }
                            else if (this.ScrambleByTDM == "Rank")
                            {
                                sortBy = "Rank";
                                sortValueA = this.rankA;
                                sortValueB = this.rankB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].rank;
                            }
                            else if (this.ScrambleByTDM == "Skill")
                            {
                                sortBy = "Skill";
                                sortValueA = this.skillA;
                                sortValueB = this.skillB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].skill;
                            }
                            else if (this.ScrambleByTDM == "SPM")
                            {
                                sortBy = "SPM";
                                sortValueA = this.spmA;
                                sortValueB = this.spmB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spm;
                            }
                            else if (this.ScrambleByTDM == "SPMcombat")
                            {
                                sortBy = "SPMcombat";
                                sortValueA = this.spmcombatA;
                                sortValueB = this.spmcombatB;

                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].spmcombat;
                            }
                            else if (this.ScrambleByTDM == "K/D")
                            {
                                sortBy = "K/D";
                                sortValueA = this.kdrA;
                                sortValueB = this.kdrB;


                                this.dicPlayerCache[strSoldierName].playerValue = this.dicPlayerCache[strSoldierName].kdr;
                            }
                        }


                        double serveraverage = (sortValueA * this.TeamA + sortValueB * this.TeamB) / (this.TeamA + this.TeamB);
                        goodplayer = 1.25 * serveraverage;
                        badplayer = 0.75 * serveraverage;


                        //bool TeamAbetter = false;
                        //bool TeamBbetter = false;



                        int Maxdifference = this.intAllowDif - 1;
                        double goodvalue = 0;
                        double badvalue = 0;
                        double valuediff = 0;
                        int betterteam = 0;
                        int otherteam = 0;
                        int scorediff = 0;
                        int playerdiff = 0;

                        if (sortValueA > sortValueB)
                        {
                            betterteam = 1;
                            otherteam = 2;
                            goodvalue = sortValueA;
                            badvalue = sortValueB;
                            valuediff = sortValueA - sortValueB;

                        }
                        else if (sortValueA < sortValueB)
                        {
                            betterteam = 2;
                            otherteam = 1;
                            goodvalue = sortValueB;
                            badvalue = sortValueA;
                            valuediff = sortValueB - sortValueA;

                        }

                        if (this.dicPlayerCache[strSoldierName].teamID == 0 && this.dicPlayerCache[strSoldierName].playerWL != 1 && !dicPlayerCache[strSoldierName].IsCommander && !dicPlayerCache[strSoldierName].IsSpectator)
                        {

                            if (!this.teamswitcher.Contains(strSoldierName))
                            {
                                int newdiff = 0;
                                int teamsizemax = 0;
                                if (iTeamID == 1)
                                {
                                    newdiff = this.TeamA - this.TeamB;
                                    scorediff = this.intScoreTeamA - this.intScoreTeamB;
                                    teamsizemax = this.intMaxSlots / 2 - this.TeamB;
                                }
                                else
                                {
                                    newdiff = this.TeamB - this.TeamA;
                                    scorediff = this.intScoreTeamB - this.intScoreTeamA;
                                    teamsizemax = this.intMaxSlots / 2 - this.TeamA;
                                }
                                if (this.dicPlayerCache[strSoldierName].playerValue >= goodplayer)
                                {
                                    this.DebugInfoGuard("^2Player's " + sortBy + ": ^b" + this.dicPlayerCache[strSoldierName].playerValue.ToString("F2"));
                                }
                                else if (this.dicPlayerCache[strSoldierName].playerValue <= badplayer)
                                {
                                    this.DebugInfoGuard("^1Player's " + sortBy + ": ^b" + this.dicPlayerCache[strSoldierName].playerValue.ToString("F2"));
                                }
                                else
                                {
                                    this.DebugInfoGuard("^0Player's " + sortBy + ": ^b" + this.dicPlayerCache[strSoldierName].playerValue.ToString("F2"));
                                }
                                this.DebugInfoGuard("Declared good, if " + sortBy + " >^2 " + goodplayer.ToString("F2") + "^9 --- Declared bad, if " + sortBy + " < ^1" + badplayer.ToString("F2"));
                                this.DebugInfoGuard("PlayerNumber --- Team 1: ^0" + this.TeamA + "^9, Team 2: ^0" + this.TeamB);
                                this.DebugInfoGuard("Score --- Team 1: ^0" + this.intScoreTeamA + "^9, Team 2: ^0" + this.intScoreTeamB);

                                if (sortValueA > sortValueB)
                                {
                                    this.DebugInfoGuard("Team ^0^b1^9^n is the better team.");

                                    if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("TeamDeathMatch") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                                    {
                                        if ((scorediff >= 0 && iTeamID == 1) || (scorediff <= 0 && iTeamID == 2))
                                        {
                                            this.DebugInfoGuard("^4^bStart sorting NewJoiners." + "\n");
                                        }
                                        else
                                        {
                                            this.DebugInfoGuard("No reason to sort NewJoiners." + "\n");
                                        }
                                    }
                                    if (this.strcurrentGametype.Contains("Domination"))
                                    {
                                        if ((scorediff >= 0 && iTeamID == 1) || (scorediff <= 0 && iTeamID == 2))
                                        {
                                            this.DebugInfoGuard("^4^bStart sorting NewJoiners." + "\n");
                                        }
                                        else
                                        {
                                            this.DebugInfoGuard("No reason to sort NewJoiners." + "\n");
                                        }
                                    }
                                    else if (this.strcurrentGametype.Contains("Obliteration"))
                                    {
                                        if ((scorediff >= 0 && iTeamID == 1) || (scorediff <= 0 && iTeamID == 2))
                                        {
                                            this.DebugInfoGuard("^4^bStart sorting NewJoiners." + "\n");
                                        }
                                        else
                                        {
                                            this.DebugInfoGuard("No reason to sort NewJoiners." + "\n");
                                        }
                                    }
                                    else if (this.strcurrentGametype.Contains("Rush") || this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                                    {
                                        this.DebugInfoGuard("^4^bSorting NewJoiners." + "\n");
                                    }
                                    else if (this.strcurrentGametype.Contains("Elimination"))
                                    {
                                        this.DebugInfoGuard("^4^bSorting NewJoiners." + "\n");
                                    }

                                }
                                else if (sortValueB > sortValueA)
                                {
                                    this.DebugInfoGuard("Team ^0^b2^9^n is the better team.");

                                    if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("TeamDeathMatch") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                                    {
                                        if ((scorediff >= 0 && iTeamID == 2) || (scorediff <= 0 && iTeamID == 1))
                                        {
                                            this.DebugInfoGuard("^4^bStart sorting NewJoiners." + "\n");
                                        }
                                        else
                                        {
                                            this.DebugInfoGuard("No reason to sort NewJoiners." + "\n");
                                        }
                                    }
                                    if (this.strcurrentGametype.Contains("Domination"))
                                    {
                                        if ((scorediff >= 0 && iTeamID == 2) || (scorediff <= 0 && iTeamID == 1))
                                        {
                                            this.DebugInfoGuard("^4^bStart sorting NewJoiners." + "\n");
                                        }
                                        else
                                        {
                                            this.DebugInfoGuard("No reason to sort NewJoiners." + "\n");
                                        }
                                    }
                                    else if (this.strcurrentGametype.Contains("Obliteration"))
                                    {
                                        if ((scorediff >= 0 && iTeamID == 2) || (scorediff <= 0 && iTeamID == 1))
                                        {
                                            this.DebugInfoGuard("^4^bStart sorting NewJoiners." + "\n");
                                        }
                                        else
                                        {
                                            this.DebugInfoGuard("No reason to sort NewJoiners." + "\n");
                                        }
                                    }
                                    else if (this.strcurrentGametype.Contains("Rush") || this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                                    {
                                        this.DebugInfoGuard("^4^bSorting NewJoiners." + "\n");
                                    }
                                    else if (this.strcurrentGametype.Contains("Elimination"))
                                    {
                                        this.DebugInfoGuard("^4^bSorting NewJoiners." + "\n");
                                    }

                                }
                                else this.DebugInfoGuard("\n");

                                //this.DebugInfoGuard(sortBy + " 1: " + sortValueA.ToString("F2") + " --- " + sortBy + " 2: " + sortValueB.ToString("F2") + "\n");

                                this.DebugInfoGuard("Player - Data: ^7TBValue: ^b" + this.dicPlayerCache[strSoldierName].TBvalue.ToString("F1") + "^n^0 - Rank: ^b" + this.dicPlayerCache[strSoldierName].rank.ToString("F2") + "^n^1 - Skill: ^b" + this.dicPlayerCache[strSoldierName].skill.ToString("F2") + "^n^2 - SPM: ^b" + this.dicPlayerCache[strSoldierName].spm.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.dicPlayerCache[strSoldierName].spmcombat.ToString("F2") + "^n^4 - K/D: ^b" + this.dicPlayerCache[strSoldierName].kdr.ToString("F2"));
                                this.DebugInfoGuard("Team 1 Data: ^7TBValue: ^b" + this.TBvalueA.ToString("F1") + "^n^0 - Rank: ^b" + this.rankA.ToString("F2") + "^n^1 - Skill: ^b" + this.skillA.ToString("F2") + "^n^2 - SPM: ^b" + this.spmA.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.spmcombatA.ToString("F2") + "^n^4 - K/D: ^b" + this.kdrA.ToString("F2"));
                                this.DebugInfoGuard("Team 2 Data: ^7TBValue: ^b" + this.TBvalueB.ToString("F1") + "^n^0 - Rank: ^b" + this.rankB.ToString("F2") + "^n^1 - Skill: ^b" + this.skillB.ToString("F2") + "^n^2 - SPM: ^b" + this.spmB.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.spmcombatB.ToString("F2") + "^n^4 - K/D: ^b" + this.kdrB.ToString("F2"));
                                this.DebugInfoGuard("Team - Diff: ^7TBValue: ^b" + (this.TBvalueA - this.TBvalueB).ToString("F1") + "^n^0 - Rank: ^b" + (this.rankA - this.rankB).ToString("F2") + "^n^1 - Skill: ^b" + (this.skillA - this.skillB).ToString("F2") + "^n^2 - SPM: ^b" + (this.spmA - this.spmB).ToString("F2") + "^n^3 - SPMcombat: ^b" + (this.spmcombatA - this.spmcombatB).ToString("F2") + "^n^4 - K/D: ^b" + (this.kdrA - this.kdrB).ToString("F2"));

                                if (this.strcurrentGametype.Contains("Conquest") || this.strcurrentGametype.Contains("TankSuperiority0") || this.strcurrentGametype.Contains("TeamDeathMatch") || this.strcurrentGametype.Contains("Scavenger") || this.strcurrentGametype.Contains("AirSuperiority0") || this.strcurrentGametype.Contains("CarrierAssault") || this.strcurrentGametype.Contains("Chainlink"))
                                {
                                    if ((sortValueA > sortValueB || sortValueB > sortValueA) && teamsizemax > 0 && newdiff >= (0 - Maxdifference))
                                    {
                                        if (scorediff >= 0 && this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && iTeamID == betterteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + otherteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, otherteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^2Good new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                        }
                                        else if (scorediff <= 0 && this.dicPlayerCache[strSoldierName].playerValue <= badplayer && iTeamID == otherteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + betterteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, betterteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^1Bad new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                        }
                                    }
                                }
                                else if (this.strcurrentGametype.Contains("Domination"))
                                {
                                    if ((sortValueA > sortValueB || sortValueB > sortValueA) && teamsizemax > 0 && newdiff >= (0 - Maxdifference))
                                    {
                                        if (scorediff >= 0 && this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && iTeamID == betterteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + otherteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, otherteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^2Good new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                        }
                                        else if (scorediff <= 0 && this.dicPlayerCache[strSoldierName].playerValue <= badplayer && iTeamID == otherteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + betterteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, betterteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^1Bad new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                        }
                                    }
                                }
                                else if (this.strcurrentGametype.Contains("Obliteration"))
                                {
                                    if ((sortValueA > sortValueB || sortValueB > sortValueA) && teamsizemax > 0 && newdiff >= (0 - Maxdifference))
                                    {
                                        if (scorediff >= 0 && this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && iTeamID == betterteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + otherteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, otherteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^2Good new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                        }
                                        else if (scorediff <= 0 && this.dicPlayerCache[strSoldierName].playerValue <= badplayer && iTeamID == otherteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + betterteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, betterteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^1Bad new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                        }
                                    }
                                }
                                else if (this.strcurrentGametype.Contains("Rush") || this.strcurrentGametype.Contains("GunMaster") || this.strcurrentGametype.Contains("CaptureTheFlag0"))
                                {
                                    if ((sortValueA > sortValueB || sortValueB > sortValueA) && teamsizemax > 0 && newdiff >= (0 - Maxdifference))
                                    {
                                        if (this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && iTeamID == betterteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + otherteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, otherteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^2Good new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                        }
                                        else if (this.dicPlayerCache[strSoldierName].playerValue <= badplayer && iTeamID == otherteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + betterteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, betterteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^1Bad new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                        }
                                    }
                                }
                                else if (this.strcurrentGametype.Contains("Elimination"))
                                {
                                    if ((sortValueA > sortValueB || sortValueB > sortValueA) && teamsizemax > 0 && newdiff >= (0 - Maxdifference))
                                    {
                                        if (this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && iTeamID == betterteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + otherteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, otherteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^2Good new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", good new player moved to other team to balance the teams.");
                                            }
                                        }
                                        else if (this.dicPlayerCache[strSoldierName].playerValue <= badplayer && iTeamID == otherteam)
                                        {
                                            if (this.boolVirtual)
                                            {
                                                this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + betterteam.ToString() + " 0 false");
                                            }
                                            else
                                            {
                                                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, betterteam.ToString(), "0", "false");
                                                LogMove("BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                            this.DebugInfoGuard("^b^1Bad new player moved to other team to balance the teams.");
                                            this.teamswitcher.Add(strSoldierName);
                                            if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                            {
                                                this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", bad new player moved to other team to balance the teams.");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    this.DebugInfoGuard("^8^bGameMode not supported.");
                                }

                            }
                            else
                            {
                                this.teamswitcher.Remove(strSoldierName);
                                //this.DebugInfoGuard("New player moved: " + strSoldierName);
                            }

                        }
                        else if (this.dicPlayerCache[strSoldierName].playerWL != 1 && this.TSLevelStartWait.TotalSeconds > 90 && !dicPlayerCache[strSoldierName].IsCommander && !dicPlayerCache[strSoldierName].IsSpectator)
                        {

                            // if (iTeamID == 1 && this.dicPlayerCache[strSoldierName].teamID != 0){
                            // this.TeamA = this.TeamA + 1;
                            // this.TeamB = this.TeamB - 1;
                            // }else if(iTeamID == 2 && this.dicPlayerCache[strSoldierName].teamID != 0){
                            // this.TeamA = this.TeamA - 1;
                            // this.TeamB = this.TeamB + 1;
                            // }

                            if (!this.teamswitcher.Contains(strSoldierName))
                            {

                                int toteam = iTeamID;
                                double switchingvalue = 0;
                                int fromteam = this.dicPlayerCache[strSoldierName].teamID;

                                if (this.BalancedPlayers.Contains(strSoldierName) == false)
                                {

                                    if (this.dicPlayerCache[strSoldierName].playerValue >= goodplayer)
                                    {
                                        this.DebugInfoGuard("^2Player's " + sortBy + ": ^b" + this.dicPlayerCache[strSoldierName].playerValue.ToString("F2"));
                                    }
                                    else if (this.dicPlayerCache[strSoldierName].playerValue <= badplayer)
                                    {
                                        this.DebugInfoGuard("^1Player's " + sortBy + ": ^b" + this.dicPlayerCache[strSoldierName].playerValue.ToString("F2"));
                                    }
                                    else
                                    {
                                        this.DebugInfoGuard("^0Player's " + sortBy + ": ^b" + this.dicPlayerCache[strSoldierName].playerValue.ToString("F2"));
                                    }
                                    this.DebugInfoGuard("Declared good, if " + sortBy + " >^2 " + goodplayer.ToString("F2") + "^9 --- Declared bad, if " + sortBy + " < ^1" + badplayer.ToString("F2"));
                                    this.DebugInfoGuard("PlayerNumber --- Team 1: ^0" + this.TeamA + "^9, Team 2: ^0" + this.TeamB);
                                    this.DebugInfoGuard("Score --- Team 1: ^0" + this.intScoreTeamA + "^9, Team 2: ^0" + this.intScoreTeamB);

                                    if (sortValueA > sortValueB) this.DebugInfoGuard("Team ^0^b1^9^n is the better team." + "\n");
                                    else if (sortValueB > sortValueA) this.DebugInfoGuard("Team ^0^b2^9^n is the better team." + "\n");
                                    else this.DebugInfoGuard("\n");

                                    //this.DebugInfoGuard(sortBy + " 1: " + sortValueA.ToString("F2") + " --- " + sortBy + " 2: " + sortValueB.ToString("F2") + "\n");

                                    this.DebugInfoGuard("Player - Data: ^7TBValue: ^b" + this.dicPlayerCache[strSoldierName].TBvalue.ToString("F1") + "^n^0 - Rank: ^b" + this.dicPlayerCache[strSoldierName].rank.ToString("F2") + "^n^1 - Skill: ^b" + this.dicPlayerCache[strSoldierName].skill.ToString("F2") + "^n^2 - SPM: ^b" + this.dicPlayerCache[strSoldierName].spm.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.dicPlayerCache[strSoldierName].spmcombat.ToString("F2") + "^n^4 - K/D: ^b" + this.dicPlayerCache[strSoldierName].kdr.ToString("F2"));
                                    this.DebugInfoGuard("Team 1 Data: ^7TBValue: ^b" + this.TBvalueA.ToString("F1") + "^n^0 - Rank: ^b" + this.rankA.ToString("F2") + "^n^1 - Skill: ^b" + this.skillA.ToString("F2") + "^n^2 - SPM: ^b" + this.spmA.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.spmcombatA.ToString("F2") + "^n^4 - K/D: ^b" + this.kdrA.ToString("F2"));
                                    this.DebugInfoGuard("Team 2 Data: ^7TBValue: ^b" + this.TBvalueB.ToString("F1") + "^n^0 - Rank: ^b" + this.rankB.ToString("F2") + "^n^1 - Skill: ^b" + this.skillB.ToString("F2") + "^n^2 - SPM: ^b" + this.spmB.ToString("F2") + "^n^3 - SPMcombat: ^b" + this.spmcombatB.ToString("F2") + "^n^4 - K/D: ^b" + this.kdrB.ToString("F2"));
                                    this.DebugInfoGuard("Team - Diff: ^7TBValue: ^b" + (this.TBvalueA - this.TBvalueB).ToString("F1") + "^n^0 - Rank: ^b" + (this.rankA - this.rankB).ToString("F2") + "^n^1 - Skill: ^b" + (this.skillA - this.skillB).ToString("F2") + "^n^2 - SPM: ^b" + (this.spmA - this.spmB).ToString("F2") + "^n^3 - SPMcombat: ^b" + (this.spmcombatA - this.spmcombatB).ToString("F2") + "^n^4 - K/D: ^b" + (this.kdrA - this.kdrB).ToString("F2"));
                                }
                                else
                                {
                                    this.DebugInfoGuard("^5-= TB Automatically =- Team 1: ^b" + this.TeamA + "^n --- Team 2: ^b" + this.TeamB);
                                    this.DebugInfoGuard("^5-= TB Automatically =- PlayerScore: ^b" + this.dicPlayerCache[strSoldierName].score);
                                    // this.ExecuteCommand("procon.protected.chat.write", "AutoBalancing Teams: " + strSoldierName);
                                }

                                if (toteam == 1)
                                {
                                    scorediff = this.intScoreTeamA - this.intScoreTeamB;
                                    playerdiff = this.TeamA - this.TeamB;
                                    switchingvalue = sortValueA;
                                }
                                else
                                {
                                    scorediff = this.intScoreTeamB - this.intScoreTeamA;
                                    playerdiff = this.TeamB - this.TeamA;
                                    switchingvalue = sortValueB;
                                }

                                TimeSpan balancetimer = new TimeSpan(0);
                                balancetimer = DateTime.Now - this.dicPlayerCache[strSoldierName].Playerjoined;

                                if (!this.BalancedPlayers.Contains(strSoldierName) && this.dicPlayerCache[strSoldierName].playerValue >= badplayer && scorediff >= (intScoreWTS * this.intTicketcount / 100) && playerdiff >= -1 && (this.strcurrentGametype.Contains("Rush") == false && this.strcurrentGametype.Contains("GunMaster") == false && this.strcurrentGametype.Contains("CaptureTheFlag0") == false && this.strcurrentGametype.Contains("Elimination") == false))
                                {
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + fromteam.ToString() + " " + this.dicPlayerCache[strSoldierName].playerSquad.ToString() + " true");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, fromteam.ToString(), this.dicPlayerCache[strSoldierName].playerSquad.ToString(), "true");
                                    }

                                    if (this.ynbShameMessage == enumBoolYesNo.Yes)
                                    {
                                        string strTEMP = this.strShameMessage.Replace("%TeamSwitcher%", strSoldierName);
                                        if (this.boolVirtual)
                                        {
                                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say all - " + strTEMP);
                                        }
                                        else
                                        {
                                            this.ExecuteCommand("procon.protected.send", "admin.say", strTEMP, "all");
                                        }
                                    }

                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are not allowed to switch into the winning team.");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are switched back into your squad.");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are not allowed to switch into the winning team.", "player", strSoldierName);
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are switched back into your squad.", "player", strSoldierName);
                                    }
                                    this.DebugInfoGuard("^b^1You are not allowed to switch into the winning team. ^3SHAME ON YOU!");
                                    this.DebugInfoGuard("^1You are switched back into your squad: ^b" + fromteam.ToString() + "." + this.dicPlayerCache[strSoldierName].playerSquad.ToString());
                                    this.teamswitcher.Add(strSoldierName);
                                    if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                    {
                                        this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", you are not allowed to switch into the winning team. SHAME ON YOU!");
                                    }
                                }
                                else if (!this.BalancedPlayers.Contains(strSoldierName) && (this.strcurrentGametype.Contains("Rush") == false && this.strcurrentGametype.Contains("GunMaster") == false && this.strcurrentGametype.Contains("Elimination") == false && this.strcurrentGametype.Contains("CaptureTheFlag0") == false) && playerdiff >= -1 /*&& valuediff >= dblValueDiff*/ && ((this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && toteam == betterteam && scorediff > 5) || (this.dicPlayerCache[strSoldierName].playerValue <= badplayer && toteam == otherteam && scorediff < -5)))
                                {
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + fromteam.ToString() + " " + this.dicPlayerCache[strSoldierName].playerSquad.ToString() + " false");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are not allowed to switch into this team, because you would unbalance the teams.");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are switched back into your squad.");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, fromteam.ToString(), this.dicPlayerCache[strSoldierName].playerSquad.ToString(), "false");
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are not allowed to switch into this team, because you would unbalance the teams.", "player", strSoldierName);
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are switched back into your squad.", "player", strSoldierName);
                                    }
                                    this.DebugInfoGuard("^5^bYou are not allowed to switch into this team, because you would unbalance the teams. (by Skill)");
                                    this.DebugInfoGuard("^5You are switched back into your squad: ^b" + fromteam.ToString() + "." + this.dicPlayerCache[strSoldierName].playerSquad.ToString());
                                    this.teamswitcher.Add(strSoldierName);
                                    if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                    {
                                        this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", you are not allowed to switch into this team, because you would unbalance the teams. (by Skill)");
                                    }
                                }
                                else if (!this.BalancedPlayers.Contains(strSoldierName) && (this.strcurrentGametype.Contains("Rush") == true || this.strcurrentGametype.Contains("GunMaster") == true || this.strcurrentGametype.Contains("Elimination") == true || this.strcurrentGametype.Contains("CaptureTheFlag0") == true) && playerdiff >= -1 /*&& valuediff >= dblValueDiff*/ && ((this.dicPlayerCache[strSoldierName].playerValue >= goodplayer && toteam == betterteam) || (this.dicPlayerCache[strSoldierName].playerValue <= badplayer && toteam == otherteam)))
                                {
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + fromteam.ToString() + " " + this.dicPlayerCache[strSoldierName].playerSquad.ToString() + " false");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are not allowed to switch into this team, because you would unbalance the teams.");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are switched back into your squad.");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, fromteam.ToString(), this.dicPlayerCache[strSoldierName].playerSquad.ToString(), "false");
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are not allowed to switch into this team, because you would unbalance the teams.", "player", strSoldierName);
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are switched back into your squad.", "player", strSoldierName);
                                    }
                                    this.DebugInfoGuard("^b^5You are not allowed to switch into this team, because you would unbalance the teams. (by Skill)");
                                    this.DebugInfoGuard("^5You are switched back into your squad: ^b" + fromteam.ToString() + "." + this.dicPlayerCache[strSoldierName].playerSquad.ToString());
                                    this.teamswitcher.Add(strSoldierName);
                                    if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                    {
                                        this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", you are not allowed to switch into this team, because you would unbalance the teams. (by Skill)");
                                    }
                                }
                                else if (!this.BalancedPlayers.Contains(strSoldierName) && playerdiff >= Maxdifference)
                                {
                                    if (this.boolVirtual)
                                    {
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + strSoldierName + " " + fromteam.ToString() + " " + this.dicPlayerCache[strSoldierName].playerSquad.ToString() + " false");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are not allowed to switch into this team, because you would unbalance the teams.");
                                        this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You are switched back into your squad.");
                                    }
                                    else
                                    {
                                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", strSoldierName, fromteam.ToString(), this.dicPlayerCache[strSoldierName].playerSquad.ToString(), "false");
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are not allowed to switch into this team, because you would unbalance the teams.", "player", strSoldierName);
                                        this.ExecuteCommand("procon.protected.send", "admin.say", "You are switched back into your squad.", "player", strSoldierName);
                                    }
                                    this.DebugInfoGuard("^b^4You are not allowed to switch into this team, because you would unbalance the teams.");
                                    this.DebugInfoGuard("^4You are switched back into your squad: ^b" + fromteam.ToString() + "." + this.dicPlayerCache[strSoldierName].playerSquad.ToString());
                                    this.teamswitcher.Add(strSoldierName);
                                    if (this.ynbDebugModeGuard == enumBoolYesNo.Yes)
                                    {
                                        this.ExecuteCommand("procon.protected.chat.write", "BalancingGuard: " + strSoldierName + ", you are not allowed to switch into this team, because you would unbalance the teams.");
                                    }
                                }
                                else if (this.BalancedPlayers.Contains(strSoldierName))
                                {
                                    this.BalancedPlayers.Remove(strSoldierName);
                                }
                            }
                            else
                            {

                                this.teamswitcher.Remove(strSoldierName);
                                //this.DebugInfoGuard("TEAMSWITCHER moved: " + strSoldierName);
                            }
                        }
                        else if (this.dicPlayerCache[strSoldierName].playerWL == 1 || dicPlayerCache[strSoldierName].IsCommander || dicPlayerCache[strSoldierName].IsSpectator)
                        {
                            this.DebugInfoGuard("^2^b" + strSoldierName + "^n is VIP, Commander or Spectator.");
                        }
                    }
                    else if (this.OnCommandMove.ContainsKey(strSoldierName))
                    {
                        this.DebugInfoGuard("^6BG: Player moved by ^bTB-AdminCommand");
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You have been moved to the other team by an admin.");
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", "You have been moved to the other team by an admin.", "player", strSoldierName);
                        }
                        this.OnCommandMove.Remove(strSoldierName);
                        if (this.OnCommandMoveDone.Contains(strSoldierName))
                            this.OnCommandMoveDone.Remove(strSoldierName);
                    }

                    this.DebugInfoGuard("^0^b**************************************************************");
                    if (this.dicPlayerCache[strSoldierName].teamID == 0)
                    {
                        this.dicPlayerCache[strSoldierName].teamID = iTeamID;
                    }
                    this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");

                }
                else
                {
                    if (this.OnCommandMove.ContainsKey(strSoldierName))
                    {
                        this.DebugInfoGuard("^6Player moved by ^bTB-AdminCommand");
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.say player " + strSoldierName + " - " + "You have been moved to the other team by an admin.");
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.say", "You have been moved to the other team by an admin.", "player", strSoldierName);
                        }
                        this.OnCommandMove.Remove(strSoldierName);
                        if (this.OnCommandMoveDone.Contains(strSoldierName))
                            this.OnCommandMoveDone.Remove(strSoldierName);
                    }
                }
            }
        }

        public override void OnPlayerLeft(CPlayerInfo cpiPlayer)
        {

            this.boolplayerleft = true;
            // int WaitforLevel = 0;


            // WaitforLevel = this.TSLevelStartWait.Hours * 3600 + this.TSLevelStartWait.Minutes * 60 + this.TSLevelStartWait.Seconds;

            this.DebugInfo("player left: ^0^b" + cpiPlayer.SoldierName + "^n^0 " + this.boolLevelStart.ToString());

            // if (this.boolLevelStart == true)
            // {        
            // this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
            // } 

            string printSoldier = "";
            printSoldier = cpiPlayer.SoldierName.Replace("{", "(");
            printSoldier = printSoldier.Replace("}", ")");

            if (this.dicPlayerCache.ContainsKey(cpiPlayer.SoldierName))
            {
                this.DebugInfo("In PlayerDic. Player Deleted.");
                this.dicPlayerCache.Remove(cpiPlayer.SoldierName);
            }

            int toberomved = -1;
            foreach (KeyValuePair<int, CPlayerScoreInf> kvp in this.dicPlayerScore)
            {
                if (this.dicPlayerScore[kvp.Key].playerName == cpiPlayer.SoldierName)
                {
                    toberomved = kvp.Key;
                    break;
                }
            }
            if (toberomved > -1)
            {
                this.dicPlayerScore.Remove(toberomved);
            }

            this.TSLevelStartWait = DateTime.Now - this.DTLevelStart;
            if (this.boolFirstOP && this.boolLevelStart)
            {
                if (strcurrentGametype != "squaddeathmatch0")
                {
                    // if ( this.intScoreTeamA > this.intminScore && this.intScoreTeamB > this.intminScore){
                    CompareTeams();
                    // } else {
                    // this.DebugInfo("Not comparing teams. Tickets till Endround: TeamA: " + this.intScoreTeamA + ", TeamB: " + this.intScoreTeamB);
                    // }
                }
                else if (strcurrentGametype != "")
                {
                    if (this.boolgametype == false)
                    {
                        this.boolgametype = true;
                        this.ExecuteCommand("procon.protected.pluginconsole.write", "TrueBalancer:  Still no data or GameMode not supported: " + "I" + strcurrentGametype + "I");
                    }
                }
            }
            else
            {
                this.DebugInfo("Waiting for FirstOP (plyrLeft)");
            }
        }

        public void OnPlayerKilled(Kill killInfo)
        {



            if (killInfo == null)
                return;

            if (this.OnCommandMove.ContainsKey(killInfo.Victim.SoldierName))

            {
                if (this.OnCommandMove[killInfo.Victim.SoldierName] == false)
                {
                    //normal move
                    if (this.OnCommandMoveDone.Contains(killInfo.Victim.SoldierName))
                    {
                        this.OnCommandMoveDone.Remove(killInfo.Victim.SoldierName);
                        this.OnCommandMove.Remove(killInfo.Victim.SoldierName);
                        this.DebugInfoGuard("^3^bMoving player to the other team went wrong on his last death. Not trying again! - ^8" + killInfo.Victim.SoldierName);
                    }
                    else
                    {
                        this.DebugInfoGuard("^2Trying to move dead player to the other side, due to TB-MoveCommand: ^b" + killInfo.Victim.SoldierName);
                        int team = 0;

                        if (this.dicPlayerCache[killInfo.Victim.SoldierName].teamID == 1)
                            team = 2;
                        else
                            team = 1;

                        this.OnCommandMoveDone.Add(killInfo.Victim.SoldierName);
                        if (this.boolVirtual)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "^b[TB] VIRTUAL^n admin.movePlayer " + killInfo.Victim.SoldierName + " " + team.ToString() + " 0 false");
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.movePlayer", killInfo.Victim.SoldierName, team.ToString(), "0", "false");
                        }
                    }
                }

                else if (this.OnCommandMove[killInfo.Victim.SoldierName] == true)
                {
                    //force move  
                    this.TSForceMove = DateTime.Now - this.DTForceMove;
                    if (this.TSForceMove.TotalMilliseconds > 10000)
                    {
                        this.DebugInfoGuard("^3^bForce Moving player to the other team went wrong! - ^8" + killInfo.Victim.SoldierName);
                        this.OnCommandMove.Remove(killInfo.Victim.SoldierName);
                    }

                }


            }
            else if (this.boolwaitfordeath)
            {
                CPlayerInfo victim = killInfo.Victim;
                if (dicPlayerCache[victim.SoldierName].teamID == intFromTeam)
                {
                    this.DebugInfo("Player ^b^0" + victim.SoldierName + "^n^9 died.");
                    if (dicPlayerCache[victim.SoldierName].playerWL == 0 && !dicPlayerCache[victim.SoldierName].IsCommander && !dicPlayerCache[victim.SoldierName].IsSpectator)
                    {
                        if (this.boolwaitdead == false)
                        {
                            this.DebugInfo("^4Player ^b" + victim.SoldierName + "^n needs to be balanced.");
                            this.strdeadplayer = victim.SoldierName;
                            //this.dicPlayerCache[victim.SoldierName].tobebalanced = true;
                            //CompareTeams();
                            this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
                        }
                    }
                    else
                    {
                        this.DebugInfo("^1Player ^b" + victim.SoldierName + "^n is VIP, Commander, Spectator or has been allready moved once.");
                    }
                }
            }
        }



        #endregion
    }
}
