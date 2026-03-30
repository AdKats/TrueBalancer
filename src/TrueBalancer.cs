/*  Copyright 2011 Panther

    This plugin is made for PRoCon.

    BFBC2 PRoCon is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    BF3 PRoCon is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with BF3 PRoCon.  If not, see <http://www.gnu.org/licenses/>.
    
    Use this script at your own risk!
 */

/* support: 
 * https://forum.myrcon.com/showthread.php?7169-TrueBalancer-BF3-BF4-0-5-RC
 * 
 * grizzlybeer
 * https://forum.myrcon.com/member.php?13930-grizzlybeer
 * 
 * TODO:
 * - remove tb-move cmds (use procons !move)
 * ok - exclude commanders, spectators
 * ok - persona id fix
 * 
 * 
 * 
 */

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
    public partial class TrueBalancer : PRoConPluginAPI, IPRoConPluginInterface
    {
        #region Variables and Constructors

        //Proconvariables
        private String m_strHostName;
        private String m_strPort;
        private String m_strPRoConVersion;

        private String Servertype;

        private DateTime lastupdatecheck = DateTime.Now.AddHours(-4);

        //TrueBalancer Variables    

        BattlelogClient bclient;

        private Dictionary<String, CPlayerJoinInf> dicPlayerCache = new Dictionary<String, CPlayerJoinInf>();
        private Dictionary<Int32, CPlayerScoreInf> dicPlayerScore = new Dictionary<Int32, CPlayerScoreInf>();
        private Dictionary<Int32, CSquadScoreInf> dicSquadScore = new Dictionary<Int32, CSquadScoreInf>();
        private Dictionary<String, Int32> dicSquadList = new Dictionary<String, Int32>();
        private Dictionary<String, Boolean> OnCommandMove = new Dictionary<String, Boolean>();
        private List<String> OnCommandMoveDone;

        private String[] strAWhitelist;
        private String[] strAClantagWhitelist;
        private String[] strAClantagWhitelistScrambler;
        private String[] strAWhitelistComplete;
        private List<String> teamswitcher;
        private List<String> BalancedPlayers;
        //private List<string> PlayersOnServer;
        private String strWarning;
        private String strLastWarning;
        private String strBeenMoved;
        private String strMovedPlayer;
        private String strJoinedPlayerName;
        private String strShameMessage;

        private String strScrambleDoneMsg;
        private String strScrambleNowMsg;
        private String strScrambleRoundMsg;

        private String strFinalSquad;
        private String strErrorMsg;
        private String strdeadplayer;

        private Int32 intInterval;
        private Int32 intWarnings;
        private Int32 intTimerWait;
        private Int32 intI;

        private Int32 TeamA;
        private Int32 TeamB;
        private Int32 intPlayerDif;
        private Int32 intToTeam;
        private Int32 intFromTeam;
        private Int32 intNewTeam;
        private Int32 intcountWarnings;
        private Int32 intWaitSeconds;
        private Int32 intScoreTeamA;
        private Int32 intScoreTeamB;
        private Int32 intTicketcount;

        private Int32 bestSquadTeamID;
        private Int32 intTicketsdif;
        private Int32 intCurrentRoundCount;
        private Int32 intMaxSlots;
        private Int32 intScrambledPlayers;
        private Int32 intSquadA;
        private Int32 intSquadB;
        private Int32 intScrambleCount;
        private Int32 intPlayerCache;
        private Double dblValueDiffRUSH;
        private Double dblValueDiffCONQUEST;
        private Double dblValueDiffDOM;
        private Double dblValueDiffTDM;
        private Double dblValueDiffGM;
        private Int32 intScoreWTS;

        private Int32 intAllowDif;
        private Int32 intminScore;

        private Int32 intminScoreRUSH;
        private Int32 intTreshRUSH;
        private Int32 intAllowDif1RUSH;
        private Int32 intAllowDif2RUSH;

        private Int32 intTreshGM;
        private Int32 intAllowDif1GM;
        private Int32 intAllowDif2GM;

        private Int32 intTreshDF;
        private Int32 intAllowDif1DF;
        private Int32 intAllowDif2DF;

        private Int32 intminScoreCONQUEST;
        private Int32 intTreshCONQUEST;
        private Int32 intAllowDif1CONQUEST;
        private Int32 intAllowDif2CONQUEST;

        private Int32 intminScoreDOM;
        private Int32 intTreshDOM;
        private Int32 intAllowDif1DOM;
        private Int32 intAllowDif2DOM;

        private Int32 intminScoreOB;
        private Int32 intTreshOB;
        private Int32 intAllowDif1OB;
        private Int32 intAllowDif2OB;

        private Int32 intminScoreTDM;
        private Int32 intTreshTDM;
        private Int32 intAllowDif1TDM;
        private Int32 intAllowDif2TDM;

        private Double rankA;
        private Double rankB;
        private Double skillA;
        private Double skillB;
        private Double spmA;
        private Double spmB;
        private Double spmcombatA;
        private Double spmcombatB;
        private Double kdrA;
        private Double kdrB;
        private Double TBvalueA;
        private Double TBvalueB;

        private enumBoolYesNo ynbDebugMode;
        private enumBoolYesNo ynbDebugModeSkill;
        private enumBoolYesNo ynbDebugModeGuard;
        private enumBoolYesNo ynbWhitelist;
        private enumBoolYesNo ynbShowWarnings;
        private enumBoolYesNo ynbShowBallancing;
        private enumBoolYesNo ynbShowPlayermessage;
        private enumBoolYesNo ynbLoneWolf;
        private enumBoolYesNo ynbincludeVIPlist;
        private enumBoolYesNo ynbBalancingGuard;
        private enumBoolYesNo ynbShameMessage;
        private enumBoolYesNo ynbScrambleMessage;
        private enumBoolYesNo ynbEnableScrambleNow;
        private enumBoolYesNo ynbEnableScrambleRound;
        private enumBoolYesNo ynbYellScrambleManuall;
        private enumBoolYesNo ynbYellScrambleMessage;

        private enumBoolYesNo ynbScrambleRoundViaPRoCon;
        private enumBoolYesNo ynbScrambleRoundViaPRoConConf;

        private String strScrambleMode;

        private enumBoolYesNo ynbenableSkillRUSH;
        private enumBoolYesNo ynbScrambleMapRUSH;
        private String ScrambleByRUSH;

        private enumBoolYesNo ynbenableSkillGM;
        private enumBoolYesNo ynbScrambleMapGM;
        private String ScrambleByGM;

        private enumBoolYesNo ynbenableSkillDF;
        private enumBoolYesNo ynbScrambleMapDF;
        private String ScrambleByDF;

        private enumBoolYesNo ynbenableSkillCONQUEST;
        private enumBoolYesNo ynbScrambleMapCONQUEST;
        private enumBoolYesNo ynbScrambleEveryRoundCONQUEST;
        private Int32 intwonTicketsCONQUEST;
        private String strScrambleMessageCONQUEST;
        private String ScrambleByCONQUEST;

        private enumBoolYesNo ynbenableSkillDOM;
        private enumBoolYesNo ynbScrambleMapDOM;
        private enumBoolYesNo ynbScrambleEveryRoundDOM;
        private Int32 intwonTicketsDOM;
        private String strScrambleMessageDOM;
        private String ScrambleByDOM;

        //private enumBoolYesNo ynbenableSkillOB;
        //private enumBoolYesNo ynbScrambleMapOB;
        //private string ScrambleByOB;

        private enumBoolYesNo ynbenableSkillOB;
        private enumBoolYesNo ynbScrambleMapOB;
        private enumBoolYesNo ynbScrambleEveryRoundOB;
        private Int32 intwonTicketsOB;
        private String strScrambleMessageOB;
        private String ScrambleByOB;

        private enumBoolYesNo ynbenableSkillTDM;
        private enumBoolYesNo ynbScrambleMapTDM;
        private enumBoolYesNo ynbScrambleEveryRoundTDM;
        private Int32 intwonTicketsTDM;
        private Int32 intshowTicketsTDM;
        private String strScrambleMessageTDM;
        private String ScrambleByTDM;

        private Boolean m_isPluginEnabled;
        private Boolean boolplayerexists;
        private Boolean boolneedbalance;
        private Boolean booltimer;
        private Boolean boolstartBalance;
        private Boolean boolFirstOP;
        private Boolean boolLevelStart;
        private Boolean boolLevelLoaded;
        private Boolean boolRoundOver;
        private Boolean boolgametype;
        private Boolean boolafterBalance;
        private Boolean boolfirstwarningWL;
        private Boolean boolmanuellchange;
        private Boolean boolnoplayer;
        private Boolean boolscrambleActive;
        private Boolean boolscrambleNow;
        private Boolean boolTeamsScrambled;
        private Boolean boolRunOnList;
        private Boolean boolplayerleft;
        private Boolean boolticketdif;
        private Boolean boolwaitfordeath;
        private Boolean boolwaitdead;
        private Boolean boolfirstscrambler;
        private Boolean boolscramblefailed;
        private Boolean boolmessagesent;
        private Boolean backswitcher;
        private Boolean boolbalanced;
        private Boolean boolscramblebyadminroundend;
        private Boolean showfirstmove;

        private String strcurrentGametype;
        private TimeSpan TSWait;
        private DateTime DTScramblestarted;

        private DateTime DTLevelStart;
        private TimeSpan TSLevelStartWait;

        private TimeSpan TSForceMove;
        private DateTime DTForceMove;

        private TimeSpan EndRoundSeconds;
        private DateTime EndRoundTime;
        private DateTime DTLevelLoaded;

        //private Timer myTimer = new Timer();
        //private TimeSpan TSWaitforOP;
        // private bool boolOnLogin;        
        //private DateTime DTRoundOver;        

        private Boolean boolVirtual;
        private enumBoolYesNo ynbVirtualMode;
        private Int32 intMaxPlayersToFetch;

        private enumBoolYesNo showMoves;
        private enumBoolYesNo Check4Update;

        public TrueBalancer()
        {

            //lastupdatecheck = DateTime.Now.AddHours(-4);

            this.bclient = new BattlelogClient(this);

            this.dicPlayerCache = new Dictionary<String, CPlayerJoinInf>();
            this.dicPlayerScore = new Dictionary<Int32, CPlayerScoreInf>();
            this.dicSquadScore = new Dictionary<Int32, CSquadScoreInf>();
            this.dicSquadList = new Dictionary<String, Int32>();
            this.OnCommandMove = new Dictionary<String, Boolean>();
            this.OnCommandMoveDone = new List<String>();

            this.Servertype = "AUTOMATIC";

            //this.strAWhitelist = new string[] { "HRPanter", "Name two", "Name three" };
            //this.strAClantagWhitelist = new string[] { "FoC", "ClanTag2", "ClanTag3" };
            //this.strAClantagWhitelistScrambler = new string[] { "FoC", "FoCr", "ClanTag3" }; ;
            this.strAWhitelist = new String[] { "HRPanter" };
            this.strAClantagWhitelist = new String[] { "FoC" };
            this.strAClantagWhitelistScrambler = new String[] { "FoC", "FoCr" }; ;
            this.strAWhitelistComplete = new String[] { };
            this.teamswitcher = new List<String>();
            this.BalancedPlayers = new List<String>();

            //this.PlayersOnServer = new List<string>(new string[] {"Z1", "Z2", "HRPanter"}); //panter
            this.strWarning = "EVEN TEAMS! Autobalancing teams shortly.";
            this.strLastWarning = "AUTOBALANCING [Player: %MovedPlayer%]";
            this.strBeenMoved = "You got balanced, because you have a low score and died.";
            this.strMovedPlayer = "";
            this.strJoinedPlayerName = "";
            this.strShameMessage = "%TeamSwitcher% tried to switch into the winning team. SHAME ON YOU!";

            this.strScrambleDoneMsg = "Teams are scrambled now. Good luck all!";
            this.strScrambleNowMsg = "Teams are going to be scrambled now. This may take up to 20 seconds!";
            this.strScrambleRoundMsg = "Teams are going to be scrambled on next round!";

            this.strFinalSquad = "";
            this.strErrorMsg = "";
            this.strdeadplayer = "";

            this.intInterval = 15;
            this.intWarnings = 1;
            this.intTimerWait = 15;
            this.intI = 0;
            this.TeamA = 0;
            this.TeamB = 0;
            this.intPlayerDif = 0;
            this.intToTeam = 0;
            this.intFromTeam = 0;
            this.intNewTeam = 0;
            this.intcountWarnings = 0;
            this.intWaitSeconds = 0;
            this.intScoreTeamA = 0;
            this.intScoreTeamB = 0;
            this.bestSquadTeamID = 0;
            this.intCurrentRoundCount = 100;
            this.intTicketsdif = -1;
            this.intMaxSlots = 0;
            this.intScrambledPlayers = 0;
            this.intSquadA = 0;
            this.intSquadB = 0;
            this.intScrambleCount = 0;
            this.intPlayerCache = 15;
            this.dblValueDiffRUSH = 10;
            this.dblValueDiffCONQUEST = 10;
            this.dblValueDiffDOM = 10;
            this.dblValueDiffTDM = 10;
            this.dblValueDiffGM = 10;
            this.intScoreWTS = 50;
            this.intTicketcount = 123987123;

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

            this.ynbDebugMode = enumBoolYesNo.No;
            this.ynbDebugModeSkill = enumBoolYesNo.No;
            this.ynbDebugModeGuard = enumBoolYesNo.No;
            //this.ynbWhitelist = enumBoolYesNo.No;
            this.ynbWhitelist = enumBoolYesNo.Yes;
            this.ynbShowWarnings = enumBoolYesNo.Yes;
            this.ynbShowBallancing = enumBoolYesNo.Yes;
            this.ynbShowPlayermessage = enumBoolYesNo.Yes;
            this.ynbLoneWolf = enumBoolYesNo.Yes;
            //this.ynbincludeVIPlist = enumBoolYesNo.No;
            this.ynbincludeVIPlist = enumBoolYesNo.Yes;
            //this.ynbShameMessage = enumBoolYesNo.Yes;
            //this.ynbBalancingGuard = enumBoolYesNo.No;
            this.ynbShameMessage = enumBoolYesNo.No;
            this.ynbBalancingGuard = enumBoolYesNo.Yes;
            this.ynbScrambleMessage = enumBoolYesNo.Yes;
            this.ynbEnableScrambleNow = enumBoolYesNo.No;
            this.ynbEnableScrambleRound = enumBoolYesNo.Yes;
            this.ynbYellScrambleManuall = enumBoolYesNo.Yes;
            this.ynbYellScrambleMessage = enumBoolYesNo.Yes;

            this.ynbScrambleRoundViaPRoCon = enumBoolYesNo.No;
            this.ynbScrambleRoundViaPRoConConf = enumBoolYesNo.No;

            this.backswitcher = false;
            this.boolplayerleft = false;
            this.m_isPluginEnabled = false;
            this.boolplayerexists = false;
            this.boolneedbalance = false;
            this.booltimer = false;
            this.boolstartBalance = false;
            this.boolLevelStart = false;
            this.boolLevelLoaded = true;
            this.boolRoundOver = false;
            this.boolgametype = false;
            this.boolafterBalance = false;
            this.boolFirstOP = false;
            this.boolfirstwarningWL = false;
            this.boolmanuellchange = false;
            this.boolnoplayer = false;
            this.boolscrambleActive = false;
            this.boolscrambleNow = false;
            this.boolTeamsScrambled = false;
            this.DTScramblestarted = new DateTime();
            this.boolRunOnList = false;
            this.boolticketdif = false;
            this.boolwaitfordeath = false;
            this.boolwaitdead = false;
            this.boolfirstscrambler = false;
            this.boolscramblefailed = false;
            this.boolmessagesent = false;
            this.boolbalanced = true;
            this.boolscramblebyadminroundend = false;
            this.showfirstmove = true;

            this.TSLevelStartWait = new TimeSpan(0);
            this.DTLevelStart = new DateTime();
            this.DTLevelLoaded = new DateTime();

            this.intAllowDif = 500;
            this.intminScore = 500;

            this.intminScoreRUSH = 15;
            this.intTreshRUSH = 24;
            this.intAllowDif1RUSH = 1;
            this.intAllowDif2RUSH = 2;

            this.intTreshGM = 16;
            this.intAllowDif1GM = 1;
            this.intAllowDif2GM = 2;

            this.intTreshDF = 16;
            this.intAllowDif1DF = 1;
            this.intAllowDif2DF = 2;

            this.intminScoreCONQUEST = 100;
            this.intTreshCONQUEST = 24;
            this.intAllowDif1CONQUEST = 1;
            this.intAllowDif2CONQUEST = 2;

            this.intminScoreDOM = 100;
            this.intTreshDOM = 24;
            this.intAllowDif1DOM = 1;
            this.intAllowDif2DOM = 2;

            this.intminScoreOB = 1;
            this.intTreshOB = 24;
            this.intAllowDif1OB = 1;
            this.intAllowDif2OB = 2;

            this.intminScoreTDM = 25;
            this.intTreshTDM = 24;
            this.intAllowDif1TDM = 1;
            this.intAllowDif2TDM = 2;

            //this.strScrambleMode = "Keep squads with two or more clanmates";
            this.strScrambleMode = "Keep all Squads";

            this.ynbenableSkillRUSH = enumBoolYesNo.No;
            this.ynbScrambleMapRUSH = enumBoolYesNo.No;
            this.ScrambleByRUSH = "TB-Value";

            this.ynbenableSkillGM = enumBoolYesNo.No;
            this.ynbScrambleMapGM = enumBoolYesNo.No;
            this.ScrambleByGM = "TB-Value";

            this.ynbenableSkillDF = enumBoolYesNo.No;
            this.ynbScrambleMapDF = enumBoolYesNo.No;
            this.ScrambleByDF = "TB-Value";

            //this.ynbenableSkillOB = enumBoolYesNo.No;
            //this.ynbScrambleMapOB = enumBoolYesNo.No;
            //this.ScrambleByOB = "TB-Value";

            //this.ynbenableSkillCONQUEST = enumBoolYesNo.No;
            this.ynbenableSkillCONQUEST = enumBoolYesNo.Yes;
            this.ynbScrambleMapCONQUEST = enumBoolYesNo.No;
            this.ynbScrambleEveryRoundCONQUEST = enumBoolYesNo.No;
            this.intwonTicketsCONQUEST = 50;
            this.strScrambleMessageCONQUEST = "SCRAMBLING teams next round. Ticketdifference too big. Squads will be kept together.";
            this.ScrambleByCONQUEST = "TB-Value";

            this.ynbenableSkillDOM = enumBoolYesNo.Yes;
            this.ynbScrambleMapDOM = enumBoolYesNo.No;
            this.ynbScrambleEveryRoundDOM = enumBoolYesNo.No;
            this.intwonTicketsDOM = 50;
            this.strScrambleMessageDOM = "SCRAMBLING teams next round. Ticketdifference too big. Squads will be kept together.";
            this.ScrambleByDOM = "TB-Value";

            //this.ynbenableSkillOB = enumBoolYesNo.No;
            this.ynbenableSkillOB = enumBoolYesNo.Yes;
            this.ynbScrambleMapOB = enumBoolYesNo.No;
            this.ynbScrambleEveryRoundOB = enumBoolYesNo.No;
            this.intwonTicketsOB = 50;
            this.strScrambleMessageOB = "SCRAMBLING teams next round. Ticketdifference too big. Squads will be kept together.";
            this.ScrambleByOB = "TB-Value";

            //this.ynbenableSkillTDM = enumBoolYesNo.No;
            this.ynbenableSkillTDM = enumBoolYesNo.Yes;
            this.ynbScrambleMapTDM = enumBoolYesNo.No;
            this.ynbScrambleEveryRoundTDM = enumBoolYesNo.No;
            this.intwonTicketsTDM = 50;
            this.intshowTicketsTDM = 50;
            this.strScrambleMessageTDM = "SCRAMBLING teams next round. Ticketdifference too big. Squads will be kept together.";
            this.ScrambleByTDM = "TB-Value";

            this.TSForceMove = new TimeSpan(0);
            this.DTForceMove = new DateTime();

            this.EndRoundSeconds = new TimeSpan(0);
            this.EndRoundTime = new DateTime();

            this.boolVirtual = false;
            this.ynbVirtualMode = enumBoolYesNo.No;
            this.intMaxPlayersToFetch = 2;

            showMoves = enumBoolYesNo.No;
            Check4Update = enumBoolYesNo.Yes;
        }

        #endregion
    }

    #region Classes

    class CPlayerJoinInf
    {
        private Int32 _teamID = 0;
        private Int32 _playerWL = 0;
        private Int32 _playerSquad = 0;
        private DateTime _Playerjoined;
        private Int32 _score = 0;
        private Boolean _tobebalanced = false;
        private Double _rank = 0;
        private Double _skill = 0;
        private Double _spm = 0;
        private Double _spmcombat = 0;
        private Double _kdr = 0;
        private Double _TBvalue = 0;
        private Double _playerValue = 0;
        private String _tag = "";

        public Boolean statsFetched = false;

        private Boolean _IsCommander = false;
        private Boolean _IsSpectator = false;

        public Int32 teamID
        {
            get { return _teamID; }
            set { _teamID = value; }
        }

        public Int32 playerWL
        {
            get { return _playerWL; }
            set { _playerWL = value; }
        }

        public Int32 playerSquad
        {
            get { return _playerSquad; }
            set { _playerSquad = value; }
        }

        public DateTime Playerjoined
        {
            get { return _Playerjoined; }
            set { _Playerjoined = value; }
        }

        public Int32 score
        {
            get { return _score; }
            set { _score = value; }
        }

        public Boolean tobebalanced
        {
            get { return _tobebalanced; }
            set { _tobebalanced = value; }
        }

        public Double rank
        {
            get { return _rank; }
            set { _rank = value; }
        }

        public Double TBvalue
        {
            get { return _TBvalue; }
            set { _TBvalue = value; }
        }

        public Double skill
        {
            get { return _skill; }
            set { _skill = value; }
        }

        public Double spm
        {
            get { return _spm; }
            set { _spm = value; }
        }

        public Double spmcombat
        {
            get { return _spmcombat; }
            set { _spmcombat = value; }
        }

        public Double kdr
        {
            get { return _kdr; }
            set { _kdr = value; }
        }

        public Double playerValue
        {
            get { return _playerValue; }
            set { _playerValue = value; }
        }

        public String tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public Boolean IsCommander
        {
            get { return _IsCommander; }
            set { _IsCommander = value; }
        }

        public Boolean IsSpectator
        {
            get { return _IsSpectator; }
            set { _IsSpectator = value; }
        }

        public CPlayerJoinInf(Int32 teamID, Int32 playerWL, Int32 playerSquad, DateTime Playerjoined, Int32 score, Double rank, Double skill, Double spm, Double spmcombat, Double kdr, Double TBvalue, Double playerValue, String tag, Boolean tobebalanced, Boolean commander, Boolean spectator)
        {
            _TBvalue = TBvalue;
            _teamID = teamID;
            _playerWL = playerWL;
            _playerSquad = playerSquad;
            _Playerjoined = Playerjoined;
            _score = score;
            _rank = rank;
            _skill = skill;
            _spm = spm;
            _spmcombat = spmcombat;
            _kdr = kdr;
            _tag = tag;
            _tobebalanced = tobebalanced;
            _playerValue = playerValue;
            _IsCommander = commander;
            _IsSpectator = spectator;
        }
    }

    class CPlayerScoreInf
    {
        private String _playerName = "";
        private Int32 _teamID = 0;
        private Int32 _playerSquad = 0;
        private Double _playerValue = 0;
        private Boolean _balanced = false;
        private Boolean _scrambled = false;
        private String _tag = String.Empty;

        public String tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public Int32 teamID
        {
            get { return _teamID; }
            set { _teamID = value; }
        }

        public Int32 playerSquad
        {
            get { return _playerSquad; }
            set { _playerSquad = value; }
        }

        public Double playerValue
        {
            get { return _playerValue; }
            set { _playerValue = value; }
        }
        public String playerName
        {
            get { return _playerName; }
            set { _playerName = value; }
        }

        public Boolean balanced
        {
            get { return _balanced; }
            set { _balanced = value; }
        }
        public Boolean scrambled
        {
            get { return _scrambled; }
            set { _scrambled = value; }
        }

        public CPlayerScoreInf(String playerName, Int32 teamID, Int32 playerSquad, Double playerValue, Boolean balanced, Boolean scrambled, String tag)
        {
            _tag = tag;
            _playerName = playerName;
            _teamID = teamID;
            _playerSquad = playerSquad;
            _playerValue = playerValue;
            _balanced = balanced;
            _scrambled = scrambled;

        }
    }

    class CSquadScoreInf
    {
        private Int32 _teamID = 0;
        private Int32 _squadID = 0;
        private Double _squadScore = 0;
        private Int32 _squadsize = 0;
        private Boolean _assigned = false;

        public Boolean assigned
        {
            get { return _assigned; }
            set { _assigned = value; }
        }

        public Int32 teamID
        {
            get { return _teamID; }
            set { _teamID = value; }
        }

        public Int32 squadsize
        {
            get { return _squadsize; }
            set { _squadsize = value; }
        }

        public Int32 squadID
        {
            get { return _squadID; }
            set { _squadID = value; }
        }

        public Double squadScore
        {
            get { return _squadScore; }
            set { _squadScore = value; }
        }

        public CSquadScoreInf(Int32 teamID, Int32 squadID, Int32 squadsize, Double squadScore, Boolean assigned)
        {
            _assigned = assigned;
            _teamID = teamID;
            _squadID = squadID;
            _squadScore = squadScore;
            _squadsize = squadsize;

        }
    }

    #endregion

}
