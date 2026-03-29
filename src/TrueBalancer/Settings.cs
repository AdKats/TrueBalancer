using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

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
        #region PluginSetup

        public String GetPluginName()
        {
            return "TrueBalancer";
        }

        public String GetPluginVersion()
        {
            return "0.5.3.0";
        }

        public String GetPluginAuthor()
        {
            return "onegrizzlybeer, versions < 0.5 by Panther (maintained by Prophet731)";
        }

        public String GetPluginWebsite()
        {
            return "forum.myrcon.com/showthread.php?7169-TrueBalancer-0-5-0-0";
        }

        // A note to plugin authors: DO NOT change how a tag works, instead make a whole new tag.
        public String GetPluginDescription()
        {
            return @"<p> ... and contributors from the Procon community.</p>
<p>If you like my plugin, please feel free to donate</p>
<blockquote>
<form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"">
<input type=""hidden"" name=""cmd"" value=""_s-xclick"">
<input type=""hidden"" name=""hosted_button_id"" value=""VVZUWJJ8UFA3Q"">
<input type=""image"" src=""https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif"" border=""0"" name=""submit"" alt=""PayPal - The safer, easier way to pay online!"">
<img alt="""" border=""0"" src=""https://www.paypalobjects.com/de_DE/i/scr/pixel.gif"" width=""1"" height=""1"">
</form>

</form>

</blockquote>
<br><br>
<h2>Description</h2>
    <p>This plugin is used to autobalance Teams in BF3 (All modes except SquadDeathMatch)<br>
    There are 3 major parts within TrueBalancer:<br><br>
    <u>1. PlayerNumber - Balancer</u> <br>
    <i>- The PN-Balancer try to keep the teams even by playernumber. Eg: 30vs30</i><br><br>
    <u>2. Skill- Scrambler</u><br>
    <i>- The Scrambler trys to even out the teams at roundend by skill. Eg: Even out the teams by SPM, Skill, K/D...</i><br><br>
    <u>3. Balancing Guard</u><br>
    <i>- Balancing Guard trys to keep teams even by skill during the game. He sorts new players to the teams they fit best and doesn't allow players to switch
    into the other team if they would unbalance the teams by PlayerNumber or Skill. In all Conquest Modes and TDM - Modes Balancing Guard doesn't allow winning team switching.</i><br><br>
    
    <b><u>IMPORTANT:</b></u><br>
    If you are running the plugins in sandboxmode, make sure you have these ports open:<br>
    https://battlelog.battlefield.com 443<br>
    http://battlelog.battlefield.com 80
    <br><br>
    This script is in Beta-Phase. Test at your own Risk. Report back please!</p>
<br><br>
<h2>Tutorials and recommended Settings</h2><br>
<p><center><font size=""+2""><a href=""http://youtu.be/hYG0NqjW5p0"" target=""_blank"" filter:Glow(color=#ff0000, strength=12);>TrueBalancer Part 1 of 3 - PlayerNumber Balancer</a><br>
<a href=""http://youtu.be/c5BhRIZYoFM"" target=""_blank"">TrueBalancer Part 2 of 3 - SkillScrambler</a><br>
<a href=""http://youtu.be/73a8_6jTHGU"" target=""_blank"">TrueBalancer Part 3 of 3 - BalancingGuard</a><br><br>
<a href=""http://www.phogue.net/forumvb/showthread.php?2866-TrueBalancer-for-BF3-(CQ-Rush-TDM)-0-2-7-(30-05-2012)&p=25828&viewfull=1#post25828"" target=""_blank"">RECOMMENDED SETTINGS</a><br>
</font></center></p>
<br><br>
<h2>InGame Commands</h2><br>
<p>@scramblenow: Will scramble the teams now. Use with caution. This will make players angry.<br>
@scrambleround: Will scramble the teams at roundend.<br><br>
Due to BalacingGuard the standart move/fmove are not working. TB has it's own commands:<br>
@tb-fmove: Force move a player into the other team.<br>
@tb-move: Move a player into the other team upon death.<br>
</p>    
<br><br>
<h2>Settings</h2><br>
<p>Here is a link for the recommended settings:<br>
<a href=""http://www.phogue.net/forumvb/showthread.php?2866-TrueBalancer-for-BF3-(CQ-Rush-TDM)-0-2-7-(30-05-2012)&p=25828&viewfull=1#post25828"" target=""_blank"">RECOMMENDED SETTINGS</a></p>
<br><br><br>
<h2>Things you need to know:</h2>
<h3>Minimum values for the settings</h3>
<table border ='1'>
    <tr><th>Setting</th><th>Minimum Value</th></tr>
    <tr><td>How many Warnings?</td><td>1</td></tr>
    <tr><td>Allow Player Difference of</td><td>1</td></tr>
    <tr><td>Time between Warnings in sec</td><td>1</td></tr>
</table>

<h3>Prefixes for TrueBalance</h3>
    <table border ='1'>
    <tr><th>Prefix</th><th>Effect</th></tr>
    <tr><td>%MovedPlayer%</td><td>Will be replaced by the moved player's name. Usable for: 'Message for moved Player' and 'Balancing Message'</td></tr>
    <tr><td>%Warning%</td><td>Will be replaced by the number of the current warning. Usable for: 'Warning Message'</td></tr>
    <tr><td>%maxWarnings%</td><td>Will be replaced by the number of maximal warnings. Usable for: 'Warning Message'</td></tr>
    </table>
    <br>
    
<h3>Changelog</h3><br>
<b>0.5.3.0</b><br>
- NEW: Added Gamemode Chainlink. Chainlink is treated just like Conquest.<br><br>
<b>0.5.2.0</b><br>
- Fix: Balancing was not working for Carrier Assault. Carrier Assault will use Conquest settings. On Carrier Assault its not possible to stop balancing once a certain point is reached (no proper way to detect for plugins)<br><br>
<b>0.5.1.1</b><br>
- Fix: Commanders and Spectators are now excluded from balancing<br>
- Fix: Persona ID could not be found for some players<br><br>
<b>0.5.1.0</b><br>
- NEW: Automatic Update Check<br>
- Fix: Domination settings block<br>
- Fix: Confirmation msg when scrambling at roundend requested by admin via procon<br>
- Fix: Settings for game mode Obliteration could not be altered.<br><br>
<b>0.5.0.0</b><br>
- NEW: BF4 compatibility<br><br>
<b>0.4.0.7 (Edits by EBastard)</b><br>
- Fix: Added Capture The Flag to plugin settings<br>
- Fix: scrambling for CTF<br><br>
<b>0.4.0.6 (Edits by EBastard)</b><br>
- ADD: Support for Air Superiority. Conquest settings also apply to this gamemodde.<br>
- ADD: Support for Capture the Flag. Gunmaster settings also apply to this gamemode.<br><br>
<b>0.4.0.2 - 0.4.0.5 (Edits by PapaCharlie9)</b><br>
- (0.4.0.5) Restores the tb-move and tb-fmove commands that were lost in the 0.4.0.4 patch<br>
- Reduces lag/delays due to stats fetching (but does not eliminate lag, see https://forum.myrcon.com/showthread.php?5623)<br>
- Reduces Procon panic errors, where the plugin seems to disable itself or disappear<br>
- Adds player name to StatsException messages<br>
- Adds a new plugin setting (see https://forum.myrcon.com/showthread.php?5623)<br><br>
<b>0.4.0.1 (Edit by EBastard)</b><br>
- ADD: Support for Scavenger. Conquest settings also apply to Scavenger.<br><br>
<b>0.4.0.0</b><br>
- ADD: Support for TankSuperiority. Conquest settings also apply to TankSuperiority.<br><br>
<b>0.3.5.5</b><br>
- Small Fix: When ticket till end was reached, the message to balance to server was shown, causing it to spam on high ticket servers. Removed this message.<br>
- Small Fix: Threshold adjusting to make it work as threshold is defined. Playerdifference below and equal to/above threshold.<br><br>
<b>0.3.5.0</b><br>
- HOTFIX: Workaround for ingame bug with 5 players in a squad.<br><br>
<b>0.3.4.0</b><br>
- CHANGE: Changing the scrambling mechanism for 'keep no squads' and 'keep squads with two or more clanmates'.<br><br>
<b>0.3.3.2</b><br>
- Fix: Problems with negativ Skills fixed. Negativ Skills are set to 0.<br>
- FIX: Problem with StatsReset feature. This got fixed by calculating all values by TrueBalancer.<br>
- FIX: Fixing a bug with Rush/GM and the BalancingGuard. Wrong values have been used.<br><br>
<b>0.3.3.1</b><br>
- ADDED: ClanTagWhitelist for the SkillScrambler, when using 'Keep squads with two or more clanmates'. Squads with at least one player, who contains one of the whitelisted ClanTags will not be teared apart. They will still be scrambled<br>
- ADDED: New Value 'TB-Value'. This is a combination of all the information/skills of a player.<br>
- FIX: Fixing a small bug in the scrambler. If a player can't be put into the squad of his mates, TB tries to assign a complete new squad to all playmates.<br><br>
<b>0.3.3</b><br>
- ADDED: TrueBalancer's own move/fmove - commands, to be able to move players, even if BalancingGuard is on.<br>
- ADDED: Possiblity to split squads while scrambling. Admins can choose what they want.<br><br>
<b>0.3.2</b><br>
- CHANGE: Changing form tickets to % in settings: Sramble at % ticketsdiff, stop winning teamswitching at x %.<br>
- CHANGE: TDM settings no need for 'show message at x tickets' anymore <br>
- TWEAK: BalancinGuard tweaked to automatically detect what team is better by all skills.<br>
- FIX: Fixing minor bug for moving dead players only (A player was still marked as dead upon revive)<br>
- FIX: Fixing TrueBalancer, which could get crazy if a manual roundrestart/nextround was requested.<br>
- ADDED: Possiblitly to execute manual command '!scrambleround' via PRoCon<br><br>
<b>0.3.1</b><br>
- FIXING: Domination bug fixed. TB didn't balance at all.<br>
- ADDED: Seperating GunMaster from Rush<br>
- FIXING: Minor issues with settings, not able to be set below a certain number<br><br>
<b>0.3.0</b><br>
- Adding support for CQC<br>
- NEW: Choose what your server should be scrambled/sorted by<br><br>
<b>0.2.8</b><br>
- Tweaking Balancing Guard to let the server breath more (not so tight settings).<br>
- Fixing some minor bugs with Rush<br><br>
<b>0.2.7</b><br>
- Fixing Balancing Guard: Moving Good players to the winning team and bad players to the loosing team on join. (When winning team had a lower SPMAverage than loosing team).<br>
- Fixing Balancing Guard: Definition for 'good' and 'bad' players was missing.<br>
Definition Before:<br>
- If SPM of a player is higher than the winning Teams SPMAverage, he was declared as good player. But a player with a SPM of 250 is far away from being good ;).<br>
Definition Now:<br>
- Good Player SPM >= 450<br>
- Bad Player SPM <= 250<br><br>
<b>0.2.6</b><br>
- Manual commands added to scramble teams now or at the end of a round.<br><br>
<b>0.2.5</b><br>
- Fixing Balancing Guard for Rush<br>
- Fixing TB for the newest Serverversion that could have gone to a critical stage at roundstart.<br><br>
<b>0.2.4</b><br>
- NEW FEATURE: Balancing Guard!<br>
- Does not allow players to teamswtich and unbalance the teams by playernumber.<br>
- Does not allow players to teamswitch to the winning team.<br>
- Sorting new players by SPM and adds them to the team they should be.<br><br>
<b>0.2.2</b><br>
- Added Threshold for all gamemodes: (Playernumberbalancer above and below that threshold can be set sepperate.)<br>
- Seperate settings for all GameModes (SquadDeathMatch is not supported)<br>
- Made the scrambler abit more safe against strane lags that can happen during scrambling.<br><br>
<b>0.2.0</b><br>
- Now scrambling by SPM (Battlelog).<br>
- Scrambling Message is yelled at roundend for 30 seconds.<br>
- NEW method to balance players. Now players are only going to be balanced when they are dead.<br><br>
<b>0.1.6</b><br>
- RUSH: Only way to scramble teams in rush is on every new map.<br><br>
<b>0.1.5</b><br>
- RUSHMode Support<br><br>
<b>0.1.4</b><br>
- FIX: Showing message at roundend when SkillScrambler is deactivated. This bug is fixed.<br><br>
<b>0.1.3</b><br>
- FIX: If the server is full, TB can't scramble the teams. Checking this 40 seconds later gives the player the chance to leave the server.<br>
- TB works now for all modes except SquadDeathMatch <br>
- RUSH: Teams will not be scrambled anymore if the defenders win. (Ticketdifference > 1000)<br>
- NEW: Message near roundend, if the teams will be scrambled next round.<br><br>
<b>0.1.2</b><br>
- VIP-List can now be included into the whitelist.<br>
- Players are not slayed anymore at roundstart. Players get scrambled before the next round starts loading.<br>
(small side effect: The scoretable gets messed during the last 20 seconds at roundend. This has no effect on scores or anyhting else. This is way better than players getting slayed at roundstart.)<br><br>
<b>0.1.0</b><br>
- SquadBug fixed<br>
- Added support for conquestsmall1<br>
- Added a scrambler, which will scrambler the teams at a new map if nessesary.<br><br>
<b>0.0.6</b><br>
Changed the lone wolf option. Now it is lone wolfs first. If there are no lone wolf, than somebody from a squad.<br><br>
<b>0.0.5</b><br>
Added possibility to move lone wolfs only.<br><br>
<b>0.0.4</b><br>
Added more possibilities to edit the ingame messages.<br><br>
<b>0.0.3</b><br>
Added 'Stop balancing, when tickets till end'<br>
Important bug fix, where the jointimes could be deleted at roundstart.<br><br>
<b>0.0.2</b><br>
Added Teamdeathmatch<br><br>
<b>0.0.1</b><br>
First Release!
        
If you have any Idears for the autobalancer contact me on the Procon - Forums. I'll try to implement every good idea.
";
        }

        public void OnPluginLoaded(String strHostName, String strPort, String strPRoConVersion)
        {
            this.boolRunOnList = false;
            this.boolLevelLoaded = true;
            this.boolnoplayer = false;
            this.boolscrambleNow = false;
            this.boolscrambleActive = false;
            this.intTicketsdif = -1;
            this.boolTeamsScrambled = false;
            this.boolticketdif = false;
            this.m_strHostName = strHostName;
            this.m_strPort = strPort;
            this.m_strPRoConVersion = strPRoConVersion;
            this.RegisterEvents(this.GetType().Name, "OnReservedSlotsList",
                                                        "OnListPlayers",
                                                        "OnPlayerSquadChange",
                                                        "OnPlayerMovedByAdmin",
                                                        "OnRoundOverPlayers",
                                                        "OnRoundOverTeamScores",
                                                        "OnPlayerSpawned",
                                                        "OnLevelLoaded",
                                                        "OnResponseError",
                                                        "OnLogin",
                                                        "OnServerInfo",
                                                        "OnPlayerTeamChange",
                                                        "OnPlayerLeft",
                                                        "OnRoundOver",
                                                        //"BalancingTimer",
                                                        "OnPlayerKilled",
                                                        "OnRestartLevel",
                                                        "OnRunNextLevel");
        }

        public void OnPluginEnable()
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bTrueBalancer ^2Enabled! " + GetPluginVersion());
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

        public void OnPluginDisable()
        {
            this.UnregisterAllCommands();

            this.boolfirstwarningWL = false;
            this.m_isPluginEnabled = false;
            this.boolneedbalance = false;

            if (this.boolscrambleActive)
            {
                this.DebugInfoSkill("^8^bScrambler active at Disabled. STOP! Teams partly scrambled!");
                this.boolTeamsScrambled = true;
                this.intScrambledPlayers = 0;
                this.boolscrambleNow = false;
                this.boolscrambleActive = false;
                this.intScrambleCount = 0;
            }
            this.dicPlayerCache.Clear();
            this.dicPlayerScore.Clear();
            this.dicSquadScore.Clear();
            this.dicSquadList.Clear();
            this.OnCommandMove.Clear();
            this.OnCommandMoveDone.Clear();
            this.BalancedPlayers.Clear();

            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bTrueBalancer ^1Disabled =(");
        }

        public List<CPluginVariable> GetDisplayPluginVariables()
        {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("0. Battlelog Stats|Maximum number of players to fetch at each interval", this.intMaxPlayersToFetch.GetType(), this.intMaxPlayersToFetch));
            lstReturn.Add(new CPluginVariable("0. Battlelog Stats|Servertype", "enum.Servertype(AUTOMATIC|BF3|BF4)", this.Servertype));

            lstReturn.Add(new CPluginVariable("0. Manual Commands via PRoCon|PRoCon - Scramble Teams on Roundend?", this.ynbScrambleRoundViaPRoCon.GetType(), this.ynbScrambleRoundViaPRoCon));
            if (this.ynbScrambleRoundViaPRoCon == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("0. Manual Commands via PRoCon|PRoCon - Scramble Teams on Roundend? Are you sure?", this.ynbScrambleRoundViaPRoConConf.GetType(), this.ynbScrambleRoundViaPRoConConf));
            }

            lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|How many warnings?", this.intWarnings.GetType(), this.intWarnings));
            lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Time between Warnings in sec", this.intInterval.GetType(), this.intInterval));
            lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Show ingame warnings?", this.ynbShowWarnings.GetType(), this.ynbShowWarnings));
            if (this.ynbShowWarnings == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Warning Message", this.strWarning.GetType(), this.strWarning));
            }

            lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Show ingame balancing message?", this.ynbShowBallancing.GetType(), this.ynbShowBallancing));
            if (this.ynbShowBallancing == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Balancing Message", this.strLastWarning.GetType(), this.strLastWarning));
            }

            lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Show private message to moved player?", this.ynbShowPlayermessage.GetType(), this.ynbShowPlayermessage));
            if (this.ynbShowPlayermessage == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("1. Playernumber Balancer: Settings|Message for moved Player", this.strBeenMoved.GetType(), this.strBeenMoved));
            }

            lstReturn.Add(new CPluginVariable("1.1 Playernumber Balancer: RUSH|RUSH-Player Threshold", this.intTreshRUSH.GetType(), this.intTreshRUSH));
            lstReturn.Add(new CPluginVariable("1.1 Playernumber Balancer: RUSH|RUSH-Allowing Player Difference below Threshold", this.intAllowDif1RUSH.GetType(), this.intAllowDif1RUSH));
            lstReturn.Add(new CPluginVariable("1.1 Playernumber Balancer: RUSH|RUSH-Allowing Player Difference equal to/above Threshold", this.intAllowDif2RUSH.GetType(), this.intAllowDif2RUSH));
            lstReturn.Add(new CPluginVariable("1.1 Playernumber Balancer: RUSH|RUSH-Stop balancing, when tickets till end", this.intminScoreRUSH.GetType(), this.intminScoreRUSH));

            lstReturn.Add(new CPluginVariable("1.2 Playernumber Balancer: CONQUEST|CQ-Player Threshold", this.intTreshCONQUEST.GetType(), this.intTreshCONQUEST));
            lstReturn.Add(new CPluginVariable("1.2 Playernumber Balancer: CONQUEST|CQ-Allowing Player Difference below Threshold", this.intAllowDif1CONQUEST.GetType(), this.intAllowDif1CONQUEST));
            lstReturn.Add(new CPluginVariable("1.2 Playernumber Balancer: CONQUEST|CQ-Allowing Player Difference equal to/above Threshold", this.intAllowDif2CONQUEST.GetType(), this.intAllowDif2CONQUEST));
            lstReturn.Add(new CPluginVariable("1.2 Playernumber Balancer: CONQUEST|CQ-Stop balancing, when tickets till end", this.intminScoreCONQUEST.GetType(), this.intminScoreCONQUEST));

            lstReturn.Add(new CPluginVariable("1.3 Playernumber Balancer: TEAMDEATHMATCH|TDM-Player Threshold", this.intTreshTDM.GetType(), this.intTreshTDM));
            lstReturn.Add(new CPluginVariable("1.3 Playernumber Balancer: TEAMDEATHMATCH|TDM-Allowing Player Difference below Threshold", this.intAllowDif1TDM.GetType(), this.intAllowDif1TDM));
            lstReturn.Add(new CPluginVariable("1.3 Playernumber Balancer: TEAMDEATHMATCH|TDM-Allowing Player Difference equal to/above Threshold", this.intAllowDif2TDM.GetType(), this.intAllowDif2TDM));
            lstReturn.Add(new CPluginVariable("1.3 Playernumber Balancer: TEAMDEATHMATCH|TDM-Stop balancing, when tickets till end", this.intminScoreTDM.GetType(), this.intminScoreTDM));

            lstReturn.Add(new CPluginVariable("1.4 Playernumber Balancer: GUN MASTER / CTF|GM/CTF-Player Threshold", this.intTreshGM.GetType(), this.intTreshGM));
            lstReturn.Add(new CPluginVariable("1.4 Playernumber Balancer: GUN MASTER / CTF|GM/CTF-Allowing Player Difference below Threshold", this.intAllowDif1GM.GetType(), this.intAllowDif1GM));
            lstReturn.Add(new CPluginVariable("1.4 Playernumber Balancer: GUN MASTER / CTF|GM/CTF-Allowing Player Difference equal to/above Threshold", this.intAllowDif2GM.GetType(), this.intAllowDif2GM));

            lstReturn.Add(new CPluginVariable("1.5 Playernumber Balancer: DEFUSE|DF-Player Threshold", this.intTreshDF.GetType(), this.intTreshDF));
            lstReturn.Add(new CPluginVariable("1.5 Playernumber Balancer: DEFUSE|DF-Allowing Player Difference below Threshold", this.intAllowDif1DF.GetType(), this.intAllowDif1DF));
            lstReturn.Add(new CPluginVariable("1.5 Playernumber Balancer: DEFUSE|DF-Allowing Player Difference equal to/above Threshold", this.intAllowDif2DF.GetType(), this.intAllowDif2DF));

            lstReturn.Add(new CPluginVariable("1.6 Playernumber Balancer: OBLITERATION|OB-Player Threshold", this.intTreshOB.GetType(), this.intTreshOB));
            lstReturn.Add(new CPluginVariable("1.6 Playernumber Balancer: OBLITERATION|OB-Allowing Player Difference below Threshold", this.intAllowDif1OB.GetType(), this.intAllowDif1OB));
            lstReturn.Add(new CPluginVariable("1.6 Playernumber Balancer: OBLITERATION|OB-Allowing Player Difference equal to/above Threshold", this.intAllowDif2OB.GetType(), this.intAllowDif2OB));
            lstReturn.Add(new CPluginVariable("1.6 Playernumber Balancer: OBLITERATION|OB-Stop balancing, when tickets till end", this.intminScoreOB.GetType(), this.intminScoreOB));

            lstReturn.Add(new CPluginVariable("1.7 Playernumber Balancer: DOMINATION|DOM-Player Threshold", this.intTreshDOM.GetType(), this.intTreshDOM));
            lstReturn.Add(new CPluginVariable("1.7 Playernumber Balancer: DOMINATION|DOM-Allowing Player Difference below Threshold", this.intAllowDif1DOM.GetType(), this.intAllowDif1DOM));
            lstReturn.Add(new CPluginVariable("1.7 Playernumber Balancer: DOMINATION|DOM-Allowing Player Difference equal to/above Threshold", this.intAllowDif2DOM.GetType(), this.intAllowDif2DOM));
            lstReturn.Add(new CPluginVariable("1.7 Playernumber Balancer: DOMINATION|DOM-Stop balancing, when tickets till end", this.intminScoreDOM.GetType(), this.intminScoreDOM));

            lstReturn.Add(new CPluginVariable("2.1 Skill-Scrambler: RUSH|RUSH-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillRUSH));

            if (this.ynbenableSkillRUSH == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("2.1 Skill-Scrambler: RUSH|RUSH-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByRUSH));
                lstReturn.Add(new CPluginVariable("2.1 Skill-Scrambler: RUSH|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.1 Skill-Scrambler: RUSH|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
                /*
                
                if(this.ynbScrambleEveryRound == enumBoolYesNo.No){
                    lstReturn.Add(new CPluginVariable("4. Skill-Scrambler: Settings|Scramble on every new map no matter what score(Rush and Conquest)", typeof(enumBoolYesNo), this.ynbScrambleMap));
                }
                if(this.ynbScrambleMap == enumBoolYesNo.No){
                    lstReturn.Add(new CPluginVariable("4. Skill-Scrambler: Settings|Check balance on every new Round (else on new Map only) (Conquest)", typeof(enumBoolYesNo), this.ynbScrambleEveryRound));
                    lstReturn.Add(new CPluginVariable("4. Skill-Scrambler: Settings|Scramble if won with over x Tickets (Coquest)", this.intwonTickets.GetType(), this.intwonTickets));
                }
                lstReturn.Add(new CPluginVariable("4. Skill-Scrambler: Settings|Scrambling Message at roundend (Coquest)", this.strScrambleMessage.GetType(), this.strScrambleMessage));
                */
            }

            lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|CQ-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillCONQUEST));
            if (this.ynbenableSkillCONQUEST == enumBoolYesNo.Yes)
            {
                if (this.ynbScrambleEveryRoundCONQUEST == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|CQ-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapCONQUEST));
                }
                if (this.ynbScrambleMapCONQUEST == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|CQ-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundCONQUEST));
                    lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|CQ-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsCONQUEST.GetType(), this.intwonTicketsCONQUEST));
                }
                lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|CQ-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByCONQUEST));
                lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
                lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|CQ-Scrambling Message at roundend", this.strScrambleMessageCONQUEST.GetType(), this.strScrambleMessageCONQUEST));
                lstReturn.Add(new CPluginVariable("2.2 Skill-Scrambler: CONQUEST|Yell scramble message at roundend?", typeof(enumBoolYesNo), this.ynbYellScrambleMessage));
            }

            lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillTDM));
            if (this.ynbenableSkillTDM == enumBoolYesNo.Yes)
            {
                if (this.ynbScrambleEveryRoundTDM == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapTDM));
                }
                if (this.ynbScrambleMapTDM == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundTDM));
                    lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsTDM.GetType(), this.intwonTicketsTDM));
                }
                //lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Show Scramble message at roundend when x Tickets are reached", this.intwonTicketsTDM.GetType(), this.intshowTicketsTDM));
                lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByTDM));
                lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
                lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|TDM-Scrambling Message at roundend", this.strScrambleMessageTDM.GetType(), this.strScrambleMessageTDM));
                lstReturn.Add(new CPluginVariable("2.3 Skill-Scrambler: TEAMDEATHMATCH|Yell scramble message at roundend?", typeof(enumBoolYesNo), this.ynbYellScrambleMessage));

            }

            lstReturn.Add(new CPluginVariable("2.4 Skill-Scrambler: GUN MASTER / CTF|GM/CTF-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillGM));

            if (this.ynbenableSkillGM == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("2.4 Skill-Scrambler: GUN MASTER / CTF|GM/CTF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByGM));
                lstReturn.Add(new CPluginVariable("2.4 Skill-Scrambler: GUN MASTER / CTF|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.4 Skill-Scrambler: GUN MASTER / CTF|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
            }

            //lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Enable Command: !scrambleround", typeof(enumBoolYesNo), this.ynbEnableScrambleRound));
            //lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Enable Command: !scramblenow", typeof(enumBoolYesNo), this.ynbEnableScrambleNow));

            //if (this.ynbEnableScrambleRound == enumBoolYesNo.Yes || this.ynbEnableScrambleNow == enumBoolYesNo.Yes)
            //{
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|RUSH-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByRUSH));
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|CQ-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByCONQUEST));
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|TDM-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByTDM));
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|GM/CTF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByGM));
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
            //    if (this.strScrambleMode == "Keep squads with two or more clanmates")
            //    {
            //        lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
            //    }
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Show Scrambling messages to the server?", typeof(enumBoolYesNo), this.ynbScrambleMessage));
            //    if (this.ynbScrambleMessage == enumBoolYesNo.Yes)
            //    {
            //        lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Yell messages to the server?", this.ynbYellScrambleManuall.GetType(), this.ynbYellScrambleManuall));
            //        lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Message when admin requests a scramble at roundend", this.strScrambleRoundMsg.GetType(), this.strScrambleRoundMsg));
            //        lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Message when admin requests a scramble now", this.strScrambleNowMsg.GetType(), this.strScrambleNowMsg));
            //        lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: Manual Commands|Message when scrambling is done", this.strScrambleDoneMsg.GetType(), this.strScrambleDoneMsg));

            //    }
            //}

            lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillOB));
            if (this.ynbenableSkillOB == enumBoolYesNo.Yes)
            {
                if (this.ynbScrambleEveryRoundOB == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapOB));
                }
                if (this.ynbScrambleMapOB == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundOB));
                    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsOB.GetType(), this.intwonTicketsOB));
                }
                lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));
                lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
                lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Scrambling Message at roundend", this.strScrambleMessageOB.GetType(), this.strScrambleMessageOB));
                lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|Yell scramble message at roundend?", typeof(enumBoolYesNo), this.ynbYellScrambleMessage));
            }

            //lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillOB));

            //if (this.ynbenableSkillOB == enumBoolYesNo.Yes)
            //{
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|OB-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));
            //    lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
            //    if (this.strScrambleMode == "Keep squads with two or more clanmates")
            //    {
            //        lstReturn.Add(new CPluginVariable("2.5 Skill-Scrambler: OBLITERATION|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
            //    }
            //}

            lstReturn.Add(new CPluginVariable("2.6 Skill-Scrambler: DEFUSE|DF-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillDF));

            if (this.ynbenableSkillDF == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("2.6 Skill-Scrambler: DEFUSE|DF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDF));
                lstReturn.Add(new CPluginVariable("2.6 Skill-Scrambler: DEFUSE|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.6 Skill-Scrambler: DEFUSE|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
            }

            lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|DOM-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillDOM));
            if (this.ynbenableSkillDOM == enumBoolYesNo.Yes)
            {
                if (this.ynbScrambleEveryRoundDOM == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|DOM-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapDOM));
                }
                if (this.ynbScrambleMapDOM == enumBoolYesNo.No)
                {
                    lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|DOM-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundDOM));
                    lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|DOM-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsDOM.GetType(), this.intwonTicketsDOM));
                }
                lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|DOM-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDOM));
                lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
                lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|DOM-Scrambling Message at roundend", this.strScrambleMessageDOM.GetType(), this.strScrambleMessageDOM));
                lstReturn.Add(new CPluginVariable("2.7 Skill-Scrambler: DOMINATION|Yell scramble message at roundend?", typeof(enumBoolYesNo), this.ynbYellScrambleMessage));
            }

            lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Enable Command: !scrambleround", typeof(enumBoolYesNo), this.ynbEnableScrambleRound));
            lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Enable Command: !scramblenow", typeof(enumBoolYesNo), this.ynbEnableScrambleNow));

            if (this.ynbEnableScrambleRound == enumBoolYesNo.Yes || this.ynbEnableScrambleNow == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|RUSH-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByRUSH));
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|CQ-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByCONQUEST));
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|TDM-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByTDM));
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|GM/CTF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByGM));
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|OB-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|DF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDF));
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
                if (this.strScrambleMode == "Keep squads with two or more clanmates")
                {
                    lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));
                }
                lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Show Scrambling messages to the server?", typeof(enumBoolYesNo), this.ynbScrambleMessage));
                if (this.ynbScrambleMessage == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Yell messages to the server?", this.ynbYellScrambleManuall.GetType(), this.ynbYellScrambleManuall));
                    lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Message when admin requests a scramble at roundend", this.strScrambleRoundMsg.GetType(), this.strScrambleRoundMsg));
                    lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Message when admin requests a scramble now", this.strScrambleNowMsg.GetType(), this.strScrambleNowMsg));
                    lstReturn.Add(new CPluginVariable("2.8 Skill-Scrambler: Manual Commands|Message when scrambling is done", this.strScrambleDoneMsg.GetType(), this.strScrambleDoneMsg));

                }
            }

            lstReturn.Add(new CPluginVariable("3. Balancing Guard|Enable Balancing Guard?", this.ynbBalancingGuard.GetType(), this.ynbBalancingGuard));
            if (this.ynbBalancingGuard == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("3. Balancing Guard|Rush - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByRUSH));
                //lstReturn.Add(new CPluginVariable("3. Balancing Guard|Rush - Start sorting at Difference of", this.dblValueDiffRUSH.GetType(), this.dblValueDiffRUSH));

                lstReturn.Add(new CPluginVariable("3. Balancing Guard|Conquest - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByCONQUEST));
                //lstReturn.Add(new CPluginVariable("3. Balancing Guard|Conquest - Start sorting at Difference of", this.dblValueDiffCONQUEST.GetType(), this.dblValueDiffCONQUEST));

                lstReturn.Add(new CPluginVariable("3. Balancing Guard|TDM - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByTDM));
                //lstReturn.Add(new CPluginVariable("3. Balancing Guard|TDM - Start sorting at Difference of", this.dblValueDiffTDM.GetType(), this.dblValueDiffTDM));

                lstReturn.Add(new CPluginVariable("3. Balancing Guard|GM/CTF - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByGM));
                //lstReturn.Add(new CPluginVariable("3. Balancing Guard|GM - Start sorting at Difference of", this.dblValueDiffGM.GetType(), this.dblValueDiffGM));

                lstReturn.Add(new CPluginVariable("3. Balancing Guard|Obliteration - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));

                lstReturn.Add(new CPluginVariable("3. Balancing Guard|DF - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDF));

                lstReturn.Add(new CPluginVariable("3. Balancing Guard|CQ/TDM - Stop winning team switching, when x % TicketDiff (% of maxTickets)", this.intScoreWTS.GetType(), this.intScoreWTS));
                lstReturn.Add(new CPluginVariable("3. Balancing Guard|CQ/TDM - Enable Shame Message?", this.ynbShameMessage.GetType(), this.ynbShameMessage));
                if (this.ynbShameMessage == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("3. Balancing Guard|CQ/TDM - Shame Message", this.strShameMessage.GetType(), this.strShameMessage));
                }
            }

            lstReturn.Add(new CPluginVariable("4. Whitelist (only PlayerNumberBalancer and BalancingGuard)|Enable Whitelist", typeof(enumBoolYesNo), this.ynbWhitelist));
            if (this.ynbWhitelist == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("4. Whitelist (only PlayerNumberBalancer and BalancingGuard)|Whitelist ClanTags", this.strAClantagWhitelist.GetType(), this.strAClantagWhitelist));
                lstReturn.Add(new CPluginVariable("4. Whitelist (only PlayerNumberBalancer and BalancingGuard)|Whitelist Names", this.strAWhitelist.GetType(), this.strAWhitelist));
                lstReturn.Add(new CPluginVariable("4. Whitelist (only PlayerNumberBalancer and BalancingGuard)|Include VIP/Reserved Slots List into the whitelist?", typeof(enumBoolYesNo), this.ynbincludeVIPlist));
            }

            lstReturn.Add(new CPluginVariable("5. Debugwindow output|Enable Debug Mode for normal balancing", this.ynbDebugMode.GetType(), this.ynbDebugMode));
            lstReturn.Add(new CPluginVariable("5. Debugwindow output|Enable Debug Mode for skill balancing", this.ynbDebugModeSkill.GetType(), this.ynbDebugModeSkill));
            lstReturn.Add(new CPluginVariable("5. Debugwindow output|Enable Debug Mode for balancing guard", this.ynbDebugModeGuard.GetType(), this.ynbDebugModeGuard));
            lstReturn.Add(new CPluginVariable("5. Debugwindow output|Show Balancing Moves (PlayerNumber Skill and Guard)", this.showMoves.GetType(), this.showMoves));

            lstReturn.Add(new CPluginVariable("6. Automatic Update Check settings|Check for Update?", this.Check4Update.GetType(), this.Check4Update));

            lstReturn.Add(new CPluginVariable("7. Testing settings|Enable Virtual Mode?", this.ynbVirtualMode.GetType(), this.ynbVirtualMode));

            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables()
        {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Maximum number of players to fetch at each interval", this.intMaxPlayersToFetch.GetType(), this.intMaxPlayersToFetch));
            lstReturn.Add(new CPluginVariable("Servertype", "enum.Servertype(AUTOMATIC|BF3|BF4)", this.Servertype));

            lstReturn.Add(new CPluginVariable("PRoCon - Scramble Teams on Roundend?", this.ynbScrambleRoundViaPRoCon.GetType(), this.ynbScrambleRoundViaPRoCon));
            lstReturn.Add(new CPluginVariable("PRoCon - Scramble Teams on Roundend? Are you sure?", this.ynbScrambleRoundViaPRoConConf.GetType(), this.ynbScrambleRoundViaPRoConConf));

            lstReturn.Add(new CPluginVariable("How many warnings?", this.intWarnings.GetType(), this.intWarnings));
            lstReturn.Add(new CPluginVariable("Time between Warnings in sec", this.intInterval.GetType(), this.intInterval));

            lstReturn.Add(new CPluginVariable("RUSH-Player Threshold", this.intTreshRUSH.GetType(), this.intTreshRUSH));
            lstReturn.Add(new CPluginVariable("RUSH-Allowing Player Difference below Threshold", this.intAllowDif1RUSH.GetType(), this.intAllowDif1RUSH));
            lstReturn.Add(new CPluginVariable("RUSH-Allowing Player Difference equal to/above Threshold", this.intAllowDif2RUSH.GetType(), this.intAllowDif2RUSH));
            lstReturn.Add(new CPluginVariable("RUSH-Stop balancing, when tickets till end", this.intminScoreRUSH.GetType(), this.intminScoreRUSH));

            lstReturn.Add(new CPluginVariable("CQ-Player Threshold", this.intTreshCONQUEST.GetType(), this.intTreshCONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Allowing Player Difference below Threshold", this.intAllowDif1CONQUEST.GetType(), this.intAllowDif1CONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Allowing Player Difference equal to/above Threshold", this.intAllowDif2CONQUEST.GetType(), this.intAllowDif2CONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Stop balancing, when tickets till end", this.intminScoreCONQUEST.GetType(), this.intminScoreCONQUEST));

            lstReturn.Add(new CPluginVariable("TDM-Player Threshold", this.intTreshTDM.GetType(), this.intTreshTDM));
            lstReturn.Add(new CPluginVariable("TDM-Allowing Player Difference below Threshold", this.intAllowDif1TDM.GetType(), this.intAllowDif1TDM));
            lstReturn.Add(new CPluginVariable("TDM-Allowing Player Difference equal to/above Threshold", this.intAllowDif2TDM.GetType(), this.intAllowDif2TDM));
            lstReturn.Add(new CPluginVariable("TDM-Stop balancing, when tickets till end", this.intminScoreTDM.GetType(), this.intminScoreTDM));

            lstReturn.Add(new CPluginVariable("GM/CTF-Player Threshold", this.intTreshGM.GetType(), this.intTreshGM));
            lstReturn.Add(new CPluginVariable("GM/CTF-Allowing Player Difference below Threshold", this.intAllowDif1GM.GetType(), this.intAllowDif1GM));
            lstReturn.Add(new CPluginVariable("GM/CTF-Allowing Player Difference equal to/above Threshold", this.intAllowDif2GM.GetType(), this.intAllowDif2GM));

            lstReturn.Add(new CPluginVariable("DF-Player Threshold", this.intTreshDF.GetType(), this.intTreshDF));
            lstReturn.Add(new CPluginVariable("DF-Allowing Player Difference below Threshold", this.intAllowDif1DF.GetType(), this.intAllowDif1DF));
            lstReturn.Add(new CPluginVariable("DF-Allowing Player Difference equal to/above Threshold", this.intAllowDif2DF.GetType(), this.intAllowDif2DF));

            lstReturn.Add(new CPluginVariable("OB-Player Threshold", this.intTreshOB.GetType(), this.intTreshOB));
            lstReturn.Add(new CPluginVariable("OB-Allowing Player Difference below Threshold", this.intAllowDif1OB.GetType(), this.intAllowDif1OB));
            lstReturn.Add(new CPluginVariable("OB-Allowing Player Difference equal to/above Threshold", this.intAllowDif2OB.GetType(), this.intAllowDif2OB));
            lstReturn.Add(new CPluginVariable("OB-Stop balancing, when tickets till end", this.intminScoreOB.GetType(), this.intminScoreOB));

            lstReturn.Add(new CPluginVariable("DOM-Player Threshold", this.intTreshDOM.GetType(), this.intTreshDOM));
            lstReturn.Add(new CPluginVariable("DOM-Allowing Player Difference below Threshold", this.intAllowDif1DOM.GetType(), this.intAllowDif1DOM));
            lstReturn.Add(new CPluginVariable("DOM-Allowing Player Difference equal to/above Threshold", this.intAllowDif2DOM.GetType(), this.intAllowDif2DOM));
            lstReturn.Add(new CPluginVariable("DOM-Stop balancing, when tickets till end", this.intminScoreDOM.GetType(), this.intminScoreDOM));

            lstReturn.Add(new CPluginVariable("Show ingame warnings?", this.ynbShowWarnings.GetType(), this.ynbShowWarnings));
            lstReturn.Add(new CPluginVariable("Warning Message", this.strWarning.GetType(), this.strWarning));
            lstReturn.Add(new CPluginVariable("Show ingame balancing message?", this.ynbShowBallancing.GetType(), this.ynbShowBallancing));
            lstReturn.Add(new CPluginVariable("Balancing Message", this.strLastWarning.GetType(), this.strLastWarning));
            lstReturn.Add(new CPluginVariable("Show private message to moved player?", this.ynbShowPlayermessage.GetType(), this.ynbShowPlayermessage));
            lstReturn.Add(new CPluginVariable("Message for moved Player", this.strBeenMoved.GetType(), this.strBeenMoved));
            lstReturn.Add(new CPluginVariable("Enable Debug Mode for normal balancing", this.ynbDebugMode.GetType(), this.ynbDebugMode));
            lstReturn.Add(new CPluginVariable("Enable Debug Mode for skill balancing", this.ynbDebugModeSkill.GetType(), this.ynbDebugModeSkill));
            lstReturn.Add(new CPluginVariable("Enable Debug Mode for balancing guard", this.ynbDebugModeGuard.GetType(), this.ynbDebugModeGuard));
            lstReturn.Add(new CPluginVariable("Enable Whitelist", typeof(enumBoolYesNo), this.ynbWhitelist));
            lstReturn.Add(new CPluginVariable("Whitelist ClanTags", this.strAClantagWhitelist.GetType(), this.strAClantagWhitelist));
            lstReturn.Add(new CPluginVariable("Whitelist Names", this.strAWhitelist.GetType(), this.strAWhitelist));
            lstReturn.Add(new CPluginVariable("Include VIP/Reserved Slots List into the whitelist?", typeof(enumBoolYesNo), this.ynbincludeVIPlist));

            lstReturn.Add(new CPluginVariable("Enable Command: !scrambleround", typeof(enumBoolYesNo), this.ynbEnableScrambleRound));
            lstReturn.Add(new CPluginVariable("Enable Command: !scramblenow", typeof(enumBoolYesNo), this.ynbEnableScrambleNow));

            lstReturn.Add(new CPluginVariable("Show Scrambling messages to the server?", typeof(enumBoolYesNo), this.ynbScrambleMessage));
            lstReturn.Add(new CPluginVariable("Yell messages to the server?", this.ynbYellScrambleManuall.GetType(), this.ynbYellScrambleManuall));
            lstReturn.Add(new CPluginVariable("Message when admin requests a scramble at roundend", this.strScrambleRoundMsg.GetType(), this.strScrambleRoundMsg));
            lstReturn.Add(new CPluginVariable("Message when admin requests a scramble now", this.strScrambleNowMsg.GetType(), this.strScrambleNowMsg));
            lstReturn.Add(new CPluginVariable("Message when scrambling is done", this.strScrambleDoneMsg.GetType(), this.strScrambleDoneMsg));

            lstReturn.Add(new CPluginVariable("Yell scramble message at roundend?", typeof(enumBoolYesNo), this.ynbYellScrambleMessage));
            lstReturn.Add(new CPluginVariable("What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
            lstReturn.Add(new CPluginVariable("ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));

            lstReturn.Add(new CPluginVariable("RUSH-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillRUSH));
            lstReturn.Add(new CPluginVariable("RUSH-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByRUSH));

            lstReturn.Add(new CPluginVariable("GM/CTF-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillGM));
            lstReturn.Add(new CPluginVariable("GM/CTF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByGM));

            lstReturn.Add(new CPluginVariable("DF-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillDF));
            lstReturn.Add(new CPluginVariable("DF-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDF));

            lstReturn.Add(new CPluginVariable("CQ-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillCONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapCONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundCONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsCONQUEST.GetType(), this.intwonTicketsCONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Scrambling Message at roundend", this.strScrambleMessageCONQUEST.GetType(), this.strScrambleMessageCONQUEST));
            lstReturn.Add(new CPluginVariable("CQ-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByCONQUEST));

            //lstReturn.Add(new CPluginVariable("OB-Scramble teams on every new map?", typeof(enumBoolYesNo), this.ynbenableSkillOB));
            //lstReturn.Add(new CPluginVariable("OB-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));

            lstReturn.Add(new CPluginVariable("OB-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillOB));
            lstReturn.Add(new CPluginVariable("OB-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapOB));
            lstReturn.Add(new CPluginVariable("OB-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundOB));
            lstReturn.Add(new CPluginVariable("OB-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsOB.GetType(), this.intwonTicketsOB));

            lstReturn.Add(new CPluginVariable("OB-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));
            lstReturn.Add(new CPluginVariable("What to do with squads?", "enum.SquadMode(Keep all Squads|Keep squads with two or more clanmates|Keep no squads)", this.strScrambleMode));
            lstReturn.Add(new CPluginVariable("ClanTag-List: Keep squad, if at least one player uses one of these ClanTags", this.strAClantagWhitelistScrambler.GetType(), this.strAClantagWhitelistScrambler));

            lstReturn.Add(new CPluginVariable("OB-Scrambling Message at roundend", this.strScrambleMessageOB.GetType(), this.strScrambleMessageOB));
            lstReturn.Add(new CPluginVariable("Yell scramble message at roundend?", typeof(enumBoolYesNo), this.ynbYellScrambleMessage));

            lstReturn.Add(new CPluginVariable("TDM-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillTDM));
            lstReturn.Add(new CPluginVariable("TDM-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapTDM));
            lstReturn.Add(new CPluginVariable("TDM-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundTDM));
            lstReturn.Add(new CPluginVariable("TDM-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsTDM.GetType(), this.intwonTicketsTDM));
            lstReturn.Add(new CPluginVariable("TDM-Show Scramble message at roundend when x Tickets are reached", this.intwonTicketsTDM.GetType(), this.intshowTicketsTDM));
            lstReturn.Add(new CPluginVariable("TDM-Scrambling Message at roundend", this.strScrambleMessageTDM.GetType(), this.strScrambleMessageTDM));
            lstReturn.Add(new CPluginVariable("TDM-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByTDM));

            lstReturn.Add(new CPluginVariable("DOM-Enable Skillscrambler?", typeof(enumBoolYesNo), this.ynbenableSkillDOM));
            lstReturn.Add(new CPluginVariable("DOM-Scramble on every new map no matter what score?", typeof(enumBoolYesNo), this.ynbScrambleMapDOM));
            lstReturn.Add(new CPluginVariable("DOM-Check balance on every new Round (else on new Map only)", typeof(enumBoolYesNo), this.ynbScrambleEveryRoundDOM));
            lstReturn.Add(new CPluginVariable("DOM-Scramble, if x % Tickets difference (% of maxTickets)", this.intwonTicketsDOM.GetType(), this.intwonTicketsDOM));
            lstReturn.Add(new CPluginVariable("DOM-Scrambling Message at roundend", this.strScrambleMessageDOM.GetType(), this.strScrambleMessageDOM));
            lstReturn.Add(new CPluginVariable("DOM-Scramble by", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDOM));

            lstReturn.Add(new CPluginVariable("Enable Balancing Guard?", this.ynbBalancingGuard.GetType(), this.ynbBalancingGuard));
            lstReturn.Add(new CPluginVariable("Rush - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByRUSH));
            lstReturn.Add(new CPluginVariable("Conquest - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByCONQUEST));
            lstReturn.Add(new CPluginVariable("TDM - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByTDM));
            lstReturn.Add(new CPluginVariable("GM/CTF - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByGM));

            lstReturn.Add(new CPluginVariable("DF - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByDF));

            lstReturn.Add(new CPluginVariable("Obliteration - Sort by (same as Scramble by)", "enum.ScrambleBy(TB-Value|Rank|Skill|SPM|SPMcombat|K/D)", this.ScrambleByOB));

            lstReturn.Add(new CPluginVariable("Rush - Start sorting at Difference of", this.dblValueDiffRUSH.GetType(), this.dblValueDiffRUSH));
            lstReturn.Add(new CPluginVariable("Conquest - Start sorting at Difference of", this.dblValueDiffCONQUEST.GetType(), this.dblValueDiffCONQUEST));
            lstReturn.Add(new CPluginVariable("TDM - Start sorting at Difference of", this.dblValueDiffTDM.GetType(), this.dblValueDiffTDM));
            lstReturn.Add(new CPluginVariable("GM/CTF - Start sorting at Difference of", this.dblValueDiffGM.GetType(), this.dblValueDiffGM));

            lstReturn.Add(new CPluginVariable("CQ/TDM - Stop winning team switching, when x % TicketDiff (% of maxTickets)", this.intScoreWTS.GetType(), this.intScoreWTS));
            lstReturn.Add(new CPluginVariable("CQ/TDM - Enable Shame Message?", this.ynbShameMessage.GetType(), this.ynbShameMessage));
            lstReturn.Add(new CPluginVariable("CQ/TDM - Shame Message", this.strShameMessage.GetType(), this.strShameMessage));

            //lstReturn.Add(new CPluginVariable("Scramble if won with over x Tickets (Coquest)", this.intwonTickets.GetType(), this.intwonTickets));
            //lstReturn.Add(new CPluginVariable("Scrambling Message at roundend (Coquest)", this.strScrambleMessage.GetType(), this.strScrambleMessage));
            //lstReturn.Add(new CPluginVariable("Scramble on every new map no matter what score(Rush and Conquest)", typeof(enumBoolYesNo), this.ynbScrambleMap));
            //lstReturn.Add(new CPluginVariable("Check balance on every new Round (else on new Map only) (Conquest)", typeof(enumBoolYesNo), this.ynbScrambleEveryRound));

            lstReturn.Add(new CPluginVariable("Show Balancing Moves (PlayerNumber Skill and Guard)", this.showMoves.GetType(), this.showMoves));

            lstReturn.Add(new CPluginVariable("Check for Update?", this.Check4Update.GetType(), this.Check4Update));

            lstReturn.Add(new CPluginVariable("Enable Virtual Mode?", this.ynbVirtualMode.GetType(), this.ynbVirtualMode));

            return lstReturn;
        }

        // Allways be suspicious of strValue's actual value.  A command in the console can
        // by the user can put any kind of data it wants in strValue.
        // use type.TryParse

        public void SetPluginVariable(String strVariable, String strValue)
        {
            if (strVariable.CompareTo("Servertype") == 0)
            {
                this.Servertype = strValue;
            }

            if (strVariable == "Maximum number of players to fetch at each interval")
            {
                Int32 numPlayers = 1;
                Int32.TryParse(strValue, out numPlayers);
                if (numPlayers < 0 || numPlayers > 3)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n ^8^bMaximum number of players to fetch must be between 0 and 3, inclusive!");
                    numPlayers = 1;
                }
                else if (numPlayers == 0)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n ^1^bMaximum number of players to fetch set to 0, stats fetching is disabled!");
                }
                else if (numPlayers != this.intMaxPlayersToFetch)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n ^0^bMaximum number of players to fetch set to " + numPlayers);
                }
                this.intMaxPlayersToFetch = numPlayers;
            }

            if (strVariable.CompareTo("PRoCon - Scramble Teams on Roundend?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleRoundViaPRoCon = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("PRoCon - Scramble Teams on Roundend? Are you sure?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleRoundViaPRoConConf = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.ynbScrambleRoundViaPRoConConf == enumBoolYesNo.Yes)
                {
                    OnScrambleViaPRoCon();
                }
            }
            else if (strVariable.CompareTo("How many warnings?") == 0 && Int32.TryParse(strValue, out this.intWarnings) == true)
            {
                if (this.intWarnings > 0)
                {
                    this.intWarnings = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intWarnings = 1;
                }
            }
            else if (strVariable.CompareTo("Enable Debug Mode for balancing guard") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbDebugModeGuard = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Balancing Guard?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbBalancingGuard = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("CQ/TDM - Enable Shame Message?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbShameMessage = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("CQ/TDM - Shame Message") == 0)
            {
                this.strShameMessage = strValue;
            }
            else if (strVariable.CompareTo("CQ/TDM - Stop winning team switching, when x % TicketDiff (% of maxTickets)") == 0 && Int32.TryParse(strValue, out this.intScoreWTS) == true)
            {
                if (this.intScoreWTS > 0)
                {
                    this.intScoreWTS = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intScoreWTS = 1;
                }
            }
            else if (strVariable.CompareTo("Rush - Start sorting at Difference of") == 0 && Double.TryParse(strValue, out this.dblValueDiffRUSH) == true)
            {
                if (this.dblValueDiffRUSH >= 0.01)
                {
                    this.dblValueDiffRUSH = Convert.ToDouble(strValue);
                }
                else
                {
                    this.dblValueDiffRUSH = 0.01;
                }
            }
            else if (strVariable.CompareTo("GM/CTF - Start sorting at Difference of") == 0 && Double.TryParse(strValue, out this.dblValueDiffGM) == true)
            {
                if (this.dblValueDiffGM >= 0.01)
                {
                    this.dblValueDiffGM = Convert.ToDouble(strValue);
                }
                else
                {
                    this.dblValueDiffGM = 0.01;
                }
            }
            else if (strVariable.CompareTo("Conquest - Start sorting at Difference of") == 0 && Double.TryParse(strValue, out this.dblValueDiffCONQUEST) == true)
            {
                if (this.dblValueDiffCONQUEST >= 0.01)
                {
                    this.dblValueDiffCONQUEST = Convert.ToDouble(strValue);
                }
                else
                {
                    this.dblValueDiffCONQUEST = 0.01;
                }
            }
            else if (strVariable.CompareTo("TDM - Start sorting at Difference of") == 0 && Double.TryParse(strValue, out this.dblValueDiffTDM) == true)
            {
                if (this.dblValueDiffTDM >= 0.01)
                {
                    this.dblValueDiffTDM = Convert.ToDouble(strValue);
                }
                else
                {
                    this.dblValueDiffTDM = 0.01;
                }
            }
            else if (strVariable.CompareTo("Rush - Sort by (same as Scramble by)") == 0)
            {
                this.ScrambleByRUSH = strValue;
            }
            else if (strVariable.CompareTo("GM/CTF - Sort by (same as Scramble by)") == 0)
            {
                this.ScrambleByGM = strValue;
            }
            else if (strVariable.CompareTo("DF - Sort by (same as Scramble by)") == 0)
            {
                this.ScrambleByDF = strValue;
            }
            else if (strVariable.CompareTo("Conquest - Sort by (same as Scramble by)") == 0)
            {
                this.ScrambleByCONQUEST = strValue;
            }
            else if (strVariable.CompareTo("TDM - Sort by (same as Scramble by)") == 0)
            {
                this.ScrambleByTDM = strValue;
            }
            else if (strVariable.CompareTo("Obliteration - Sort by (same as Scramble by)") == 0)
            {
                this.ScrambleByOB = strValue;
            }
            else if (strVariable.CompareTo("Time between Warnings in sec") == 0 && Int32.TryParse(strValue, out this.intInterval) == true)
            {
                if (this.intInterval > 0)
                {
                    this.intInterval = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intInterval = 1;
                }
            }
            else if (strVariable.CompareTo("RUSH-Player Threshold") == 0 && Int32.TryParse(strValue, out this.intTreshRUSH) == true)
            {
                if (this.intTreshRUSH > 0)
                {
                    this.intTreshRUSH = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshRUSH = 0;
                }
            }
            else if (strVariable.CompareTo("RUSH-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif1RUSH) == true)
            {
                if (this.intAllowDif1RUSH > 0)
                {
                    this.intAllowDif1RUSH = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1RUSH = 1;
                }
            }
            else if (strVariable.CompareTo("RUSH-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif2RUSH) == true)
            {
                if (this.intAllowDif2RUSH > 0)
                {
                    this.intAllowDif2RUSH = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2RUSH = 1;
                }
            }
            else if (strVariable.CompareTo("RUSH-Stop balancing, when tickets till end") == 0 && Int32.TryParse(strValue, out this.intminScoreRUSH) == true)
            {
                this.intminScoreRUSH = Convert.ToInt32(strValue);

            }

            else if (strVariable.CompareTo("GM/CTF-Player Threshold") == 0 && Int32.TryParse(strValue, out this.intTreshGM) == true)
            {
                if (this.intTreshGM > 0)
                {
                    this.intTreshGM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshGM = 0;
                }
            }
            else if (strVariable.CompareTo("GM/CTF-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif1GM) == true)
            {
                if (this.intAllowDif1GM > 0)
                {
                    this.intAllowDif1GM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1GM = 1;
                }
            }
            else if (strVariable.CompareTo("GM/CTF-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif2GM) == true)
            {
                if (this.intAllowDif2GM > 0)
                {
                    this.intAllowDif2GM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2GM = 1;
                }
            }

            else if (strVariable.CompareTo("DF-Player Threshold") == 0 && Int32.TryParse(strValue, out this.intTreshDF) == true)
            {
                if (this.intTreshDF > 0)
                {
                    this.intTreshDF = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshDF = 0;
                }
            }
            else if (strVariable.CompareTo("DF-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif1DF) == true)
            {
                if (this.intAllowDif1DF > 0)
                {
                    this.intAllowDif1DF = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1DF = 1;
                }
            }
            else if (strVariable.CompareTo("DF-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif2DF) == true)
            {
                if (this.intAllowDif2DF > 0)
                {
                    this.intAllowDif2DF = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2DF = 1;
                }
            }

            else if (strVariable.CompareTo("CQ-Player Threshold") == 0 && Int32.TryParse(strValue, out this.intTreshCONQUEST) == true)
            {
                if (this.intTreshCONQUEST > 0)
                {
                    this.intTreshCONQUEST = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshCONQUEST = 0;
                }
            }
            else if (strVariable.CompareTo("CQ-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif1CONQUEST) == true)
            {
                if (this.intAllowDif1CONQUEST > 0)
                {
                    this.intAllowDif1CONQUEST = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1CONQUEST = 1;
                }
            }
            else if (strVariable.CompareTo("CQ-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif2CONQUEST) == true)
            {
                if (this.intAllowDif2CONQUEST > 0)
                {
                    this.intAllowDif2CONQUEST = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2CONQUEST = 1;
                }
            }
            else if (strVariable.CompareTo("CQ-Stop balancing, when tickets till end") == 0 && Int32.TryParse(strValue, out this.intminScoreCONQUEST) == true)
            {
                this.intminScoreCONQUEST = Convert.ToInt32(strValue);

            }

            else if (strVariable.CompareTo("DOM-Player Threshold") == 0 && Int32.TryParse(strValue, out this.intTreshDOM) == true)
            {
                if (this.intTreshDOM > 0)
                {
                    this.intTreshDOM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshDOM = 0;
                }
            }
            else if (strVariable.CompareTo("DOM-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif1DOM) == true)
            {
                if (this.intAllowDif1DOM > 0)
                {
                    this.intAllowDif1DOM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1DOM = 1;
                }
            }
            else if (strVariable.CompareTo("DOM-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif2DOM) == true)
            {
                if (this.intAllowDif2DOM > 0)
                {
                    this.intAllowDif2DOM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2DOM = 1;
                }
            }
            else if (strVariable.CompareTo("DOM-Stop balancing, when tickets till end") == 0 && Int32.TryParse(strValue, out this.intminScoreDOM) == true)
            {
                this.intminScoreDOM = Convert.ToInt32(strValue);

            }

            else if (strVariable.CompareTo("TDM-Player Threshold") == 0 && Int32.TryParse(strValue, out this.intTreshTDM) == true)
            {
                if (this.intTreshTDM > 0)
                {
                    this.intTreshTDM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshTDM = 0;
                }
            }
            else if (strVariable.CompareTo("TDM-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif1TDM) == true)
            {
                if (this.intAllowDif1TDM > 0)
                {
                    this.intAllowDif1TDM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1TDM = 1;
                }
            }
            else if (strVariable.CompareTo("TDM-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif2TDM) == true)
            {
                if (this.intAllowDif2TDM > 0)
                {
                    this.intAllowDif2TDM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2TDM = 1;
                }
            }
            else if (strVariable.CompareTo("TDM-Stop balancing, when tickets till end") == 0 && Int32.TryParse(strValue, out this.intminScoreTDM) == true)
            {
                this.intminScoreTDM = Convert.ToInt32(strValue);

            }

            else if (strVariable.CompareTo("OB-Player Threshold") == 0 && Int32.TryParse(strValue, out this.intTreshOB) == true)
            {
                if (this.intTreshOB > 0)
                {
                    this.intTreshOB = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intTreshOB = 0;
                }
            }
            else if (strVariable.CompareTo("OB-Allowing Player Difference below Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif1OB) == true)
            {
                if (this.intAllowDif1OB > 0)
                {
                    this.intAllowDif1OB = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif1OB = 1;
                }
            }
            else if (strVariable.CompareTo("OB-Allowing Player Difference equal to/above Threshold") == 0 && Int32.TryParse(strValue, out this.intAllowDif2OB) == true)
            {
                if (this.intAllowDif2OB > 0)
                {
                    this.intAllowDif2OB = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intAllowDif2OB = 1;
                }
            }
            else if (strVariable.CompareTo("OB-Stop balancing, when tickets till end") == 0 && Int32.TryParse(strValue, out this.intminScoreOB) == true)
            {
                this.intminScoreOB = Convert.ToInt32(strValue);

            }

            else if (strVariable.CompareTo("Show ingame warnings?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbShowWarnings = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Warning Message") == 0)
            {
                this.strWarning = strValue;
            }
            else if (strVariable.CompareTo("Show ingame balancing message?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbShowBallancing = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Balancing Message") == 0)
            {
                this.strLastWarning = strValue;
            }
            else if (strVariable.CompareTo("Show private message to moved player?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbShowPlayermessage = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Message for moved Player") == 0)
            {
                this.strBeenMoved = strValue;
            }

            else if (strVariable.CompareTo("Enable Command: !scrambleround") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbEnableScrambleRound = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Command: !scramblenow") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbEnableScrambleNow = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show Scrambling messages to the server?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleMessage = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Yell messages to the server?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbYellScrambleManuall = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Message when admin requests a scramble at roundend") == 0)
            {
                this.strScrambleRoundMsg = strValue;
            }
            else if (strVariable.CompareTo("Message when admin requests a scramble now") == 0)
            {
                this.strScrambleNowMsg = strValue;
            }
            else if (strVariable.CompareTo("Message when scrambling is done") == 0)
            {
                this.strScrambleDoneMsg = strValue;
            }

            else if (strVariable.CompareTo("Yell scramble message at roundend?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbYellScrambleMessage = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("What to do with squads?") == 0)
            {
                this.strScrambleMode = strValue;
            }
            else if (strVariable.CompareTo("ClanTag-List: Keep squad, if at least one player uses one of these ClanTags") == 0)
            {
                this.strAClantagWhitelistScrambler = CPluginVariable.DecodeStringArray(strValue);
            }

            else if (strVariable.CompareTo("RUSH-Scramble teams on every new map?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillRUSH = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("RUSH-Scramble by") == 0)
            {
                this.ScrambleByRUSH = strValue;
            }

            else if (strVariable.CompareTo("GM/CTF-Scramble teams on every new map?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillGM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("GM/CTF-Scramble by") == 0)
            {
                this.ScrambleByGM = strValue;
            }

            else if (strVariable.CompareTo("DF-Scramble teams on every new map?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillDF = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("DF-Scramble by") == 0)
            {
                this.ScrambleByDF = strValue;
            }

            //else if (strVariable.CompareTo("OB-Scramble teams on every new map?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            //{
            //    this.ynbenableSkillOB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            //}
            //else if (strVariable.CompareTo("OB-Scramble by") == 0)
            //{
            //    this.ScrambleByOB = strValue;
            //}

            else if (strVariable.CompareTo("CQ-Enable Skillscrambler?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillCONQUEST = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("CQ-Scramble on every new map no matter what score?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleMapCONQUEST = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("CQ-Check balance on every new Round (else on new Map only)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleEveryRoundCONQUEST = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("CQ-Scramble, if x % Tickets difference (% of maxTickets)") == 0 && Int32.TryParse(strValue, out this.intwonTicketsCONQUEST) == true)
            {
                if (this.intwonTicketsCONQUEST >= 0)
                {
                    this.intwonTicketsCONQUEST = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intwonTicketsCONQUEST = 0;
                }
            }
            else if (strVariable.CompareTo("CQ-Scrambling Message at roundend") == 0)
            {
                this.strScrambleMessageCONQUEST = strValue;
            }
            else if (strVariable.CompareTo("CQ-Scramble by") == 0)
            {
                this.ScrambleByCONQUEST = strValue;
            }

            else if (strVariable.CompareTo("DOM-Enable Skillscrambler?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillDOM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("DOM-Scramble on every new map no matter what score?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleMapDOM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("DOM-Check balance on every new Round (else on new Map only)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleEveryRoundDOM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("DOM-Scramble, if x % Tickets difference (% of maxTickets)") == 0 && Int32.TryParse(strValue, out this.intwonTicketsDOM) == true)
            {
                if (this.intwonTicketsDOM >= 0)
                {
                    this.intwonTicketsDOM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intwonTicketsDOM = 0;
                }
            }
            else if (strVariable.CompareTo("DOM-Scrambling Message at roundend") == 0)
            {
                this.strScrambleMessageDOM = strValue;
            }
            else if (strVariable.CompareTo("DOM-Scramble by") == 0)
            {
                this.ScrambleByDOM = strValue;
            }

            else if (strVariable.CompareTo("OB-Enable Skillscrambler?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillOB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("OB-Scramble on every new map no matter what score?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleMapOB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("OB-Check balance on every new Round (else on new Map only)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleEveryRoundOB = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("OB-Scramble, if x % Tickets difference (% of maxTickets)") == 0 && Int32.TryParse(strValue, out this.intwonTicketsOB) == true)
            {
                if (this.intwonTicketsOB >= 0)
                {
                    this.intwonTicketsOB = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intwonTicketsOB = 0;
                }
            }
            else if (strVariable.CompareTo("OB-Scrambling Message at roundend") == 0)
            {
                this.strScrambleMessageOB = strValue;
            }
            else if (strVariable.CompareTo("OB-Scramble by") == 0)
            {
                this.ScrambleByOB = strValue;
            }

            else if (strVariable.CompareTo("TDM-Enable Skillscrambler?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbenableSkillTDM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("TDM-Scramble on every new map no matter what score?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleMapTDM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("TDM-Check balance on every new Round (else on new Map only)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbScrambleEveryRoundTDM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            if (strVariable.CompareTo("TDM-Scramble, if x % Tickets difference (% of maxTickets)") == 0 && Int32.TryParse(strValue, out this.intwonTicketsTDM) == true)
            {
                if (this.intwonTicketsTDM >= 0)
                {
                    this.intwonTicketsTDM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intwonTicketsTDM = 0;
                }
            }
            if (strVariable.CompareTo("TDM-Show Scramble message at roundend when x Tickets are reached") == 0 && Int32.TryParse(strValue, out this.intshowTicketsTDM) == true)
            {
                if (this.intshowTicketsTDM >= 0)
                {
                    this.intshowTicketsTDM = Convert.ToInt32(strValue);
                }
                else
                {
                    this.intshowTicketsTDM = 5;
                }
            }
            else if (strVariable.CompareTo("TDM-Scrambling Message at roundend") == 0)
            {
                this.strScrambleMessageTDM = strValue;
            }
            else if (strVariable.CompareTo("TDM-Scramble by") == 0)
            {
                this.ScrambleByTDM = strValue;
            }

            else if (strVariable.CompareTo("Enable Debug Mode for normal balancing") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbDebugMode = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Debug Mode for skill balancing") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbDebugModeSkill = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Whitelist") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbWhitelist = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Whitelist ClanTags") == 0)
            {
                this.strAClantagWhitelist = CPluginVariable.DecodeStringArray(strValue);
            }
            else if (strVariable.CompareTo("Whitelist Names") == 0)
            {
                this.strAWhitelist = CPluginVariable.DecodeStringArray(strValue);
            }
            else if (strVariable.CompareTo("Include VIP/Reserved Slots List into the whitelist?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbincludeVIPlist = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Show Balancing Moves (PlayerNumber Skill and Guard)") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.showMoves = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Check for Update?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.Check4Update = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }

            if (strVariable == "Enable Virtual Mode?" && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ynbVirtualMode = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                this.boolVirtual = (this.ynbVirtualMode == enumBoolYesNo.Yes);
                if (this.boolVirtual)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^b^9TrueBalancer:^n ^1^bVirtual mode is ENABLED!^n^0 No player moves or say/yells will be sent to the game server.");
                }
            }

        }

        private void UnregisterAllCommands()
        {

            List<String> emptyList = new List<String>();

            this.UnregisterCommand(
                new MatchCommand(
                    emptyList,
                    "scramblenow",
                    this.Listify<MatchArgumentFormat>()
                )
            );

            this.UnregisterCommand(
                new MatchCommand(
                    emptyList,
                    "scrambleround",
                    this.Listify<MatchArgumentFormat>()
                )
            );

            this.UnregisterCommand(
                new MatchCommand(
                    emptyList,
                    "tb-move",
                    this.Listify<MatchArgumentFormat>()
                )
            );

            this.UnregisterCommand(
                new MatchCommand(
                    emptyList,
                    "tb-fmove",
                    this.Listify<MatchArgumentFormat>()
                )
            );

        }

        private void RegisterAllCommands()
        {

            if (this.m_isPluginEnabled == true)
            {
                MatchCommand confirmationCommand = new MatchCommand(this.Listify<String>("@", "!", "#"), "yes", this.Listify<MatchArgumentFormat>());

                this.RegisterCommand(
                    new MatchCommand(
                        "TrueBalancer",
                        "OnCommandScrambleNow",
                        this.Listify<String>("@", "!", "#"),
                        "scramblenow",
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Privileges,
                            Privileges.CanMovePlayers,
                            0,
                            confirmationCommand,
                            "You do not have enough privileges to scramble players"),
                        "Scrambling teams now! Use with caution."
                    )
                );

                this.RegisterCommand(
                    new MatchCommand(
                        "TrueBalancer",
                        "OnCommandScrambleRound",
                        this.Listify<String>("@", "!", "#"),
                        "scrambleround",
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Privileges,
                            Privileges.CanMovePlayers,
                            0,
                            confirmationCommand,
                            "You do not have enough privileges to scramble players"),
                        "Scrambling teams next round!"
                    )
                );

                this.RegisterCommand(
                        new MatchCommand(
                            "TrueBalancer",
                            "OnCommandTBMove",
                            this.Listify<String>("@", "!", "#"),
                            "tb-move",
                            this.Listify<MatchArgumentFormat>(
                                new MatchArgumentFormat(
                                    "playername",
                                    GetSoldierNames()
                                )
                            ),
                            new ExecutionRequirements(
                                ExecutionScope.Privileges,
                                Privileges.CanMovePlayers,
                                2,
                                confirmationCommand,
                                "You do not have enough privileges to move players"),
                            "Moves the player to the other team on death!"
                        )
                    );

                this.RegisterCommand(
                        new MatchCommand(
                            "TrueBalancer",
                            "OnCommandTBForceMove",
                            this.Listify<String>("@", "!", "#"),
                            "tb-fmove",
                            this.Listify<MatchArgumentFormat>(
                                new MatchArgumentFormat(
                                    "playername",
                                    GetSoldierNames()
                                )
                            ),
                            new ExecutionRequirements(
                                ExecutionScope.Privileges,
                                Privileges.CanMovePlayers,
                                2,
                                confirmationCommand,
                                "You do not have enough privileges to move players"),
                            "Kills a player and moves the player to the other team."
                        )
                    );
            }

        }

        #endregion
    }
}
