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
    public partial class TrueBalancer : PRoConPluginAPI, IPRoConPluginInterface
    {
        #region Variables and Constructors

        //Proconvariables
        private string m_strHostName;
        private string m_strPort;
        private string m_strPRoConVersion;

        private string Servertype;

        private DateTime lastupdatecheck = DateTime.Now.AddHours(-4);

        //TrueBalancer Variables    

        BattlelogClient bclient;

        private Dictionary<string, CPlayerJoinInf> dicPlayerCache = new Dictionary<string, CPlayerJoinInf>();
        private Dictionary<int, CPlayerScoreInf> dicPlayerScore = new Dictionary<int, CPlayerScoreInf>();
        private Dictionary<int, CSquadScoreInf> dicSquadScore = new Dictionary<int, CSquadScoreInf>();
        private Dictionary<string, int> dicSquadList = new Dictionary<string, int>();
        private Dictionary<string, bool> OnCommandMove = new Dictionary<string, bool>();
        private List<string> OnCommandMoveDone;

        private string[] strAWhitelist;
        private string[] strAClantagWhitelist;
        private string[] strAClantagWhitelistScrambler;
        private string[] strAWhitelistComplete;
        private List<string> teamswitcher;
        private List<string> BalancedPlayers;
        //private List<string> PlayersOnServer;
        private string strWarning;
        private string strLastWarning;
        private string strBeenMoved;
        private string strMovedPlayer;
        private string strJoinedPlayerName;
        private string strShameMessage;

        private string strScrambleDoneMsg;
        private string strScrambleNowMsg;
        private string strScrambleRoundMsg;

        private string strFinalSquad;
        private string strErrorMsg;
        private string strdeadplayer;

        private int intInterval;
        private int intWarnings;
        private int intTimerWait;
        private int intI;

        private int TeamA;
        private int TeamB;
        private int intPlayerDif;
        private int intToTeam;
        private int intFromTeam;
        private int intNewTeam;
        private int intcountWarnings;
        private int intWaitSeconds;
        private int intScoreTeamA;
        private int intScoreTeamB;
        private int intTicketcount;

        private int bestSquadTeamID;
        private int intTicketsdif;
        private int intCurrentRoundCount;
        private int intMaxSlots;
        private int intScrambledPlayers;
        private int intSquadA;
        private int intSquadB;
        private int intScrambleCount;
        private int intPlayerCache;
        private double dblValueDiffRUSH;
        private double dblValueDiffCONQUEST;
        private double dblValueDiffDOM;
        private double dblValueDiffTDM;
        private double dblValueDiffGM;
        private int intScoreWTS;

        private int intAllowDif;
        private int intminScore;

        private int intminScoreRUSH;
        private int intTreshRUSH;
        private int intAllowDif1RUSH;
        private int intAllowDif2RUSH;

        private int intTreshGM;
        private int intAllowDif1GM;
        private int intAllowDif2GM;

        private int intTreshDF;
        private int intAllowDif1DF;
        private int intAllowDif2DF;

        private int intminScoreCONQUEST;
        private int intTreshCONQUEST;
        private int intAllowDif1CONQUEST;
        private int intAllowDif2CONQUEST;

        private int intminScoreDOM;
        private int intTreshDOM;
        private int intAllowDif1DOM;
        private int intAllowDif2DOM;

        private int intminScoreOB;
        private int intTreshOB;
        private int intAllowDif1OB;
        private int intAllowDif2OB;

        private int intminScoreTDM;
        private int intTreshTDM;
        private int intAllowDif1TDM;
        private int intAllowDif2TDM;

        private double rankA;
        private double rankB;
        private double skillA;
        private double skillB;
        private double spmA;
        private double spmB;
        private double spmcombatA;
        private double spmcombatB;
        private double kdrA;
        private double kdrB;
        private double TBvalueA;
        private double TBvalueB;

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

        private string strScrambleMode;

        private enumBoolYesNo ynbenableSkillRUSH;
        private enumBoolYesNo ynbScrambleMapRUSH;
        private string ScrambleByRUSH;

        private enumBoolYesNo ynbenableSkillGM;
        private enumBoolYesNo ynbScrambleMapGM;
        private string ScrambleByGM;

        private enumBoolYesNo ynbenableSkillDF;
        private enumBoolYesNo ynbScrambleMapDF;
        private string ScrambleByDF;

        private enumBoolYesNo ynbenableSkillCONQUEST;
        private enumBoolYesNo ynbScrambleMapCONQUEST;
        private enumBoolYesNo ynbScrambleEveryRoundCONQUEST;
        private int intwonTicketsCONQUEST;
        private string strScrambleMessageCONQUEST;
        private string ScrambleByCONQUEST;

        private enumBoolYesNo ynbenableSkillDOM;
        private enumBoolYesNo ynbScrambleMapDOM;
        private enumBoolYesNo ynbScrambleEveryRoundDOM;
        private int intwonTicketsDOM;
        private string strScrambleMessageDOM;
        private string ScrambleByDOM;

        //private enumBoolYesNo ynbenableSkillOB;
        //private enumBoolYesNo ynbScrambleMapOB;
        //private string ScrambleByOB;

        private enumBoolYesNo ynbenableSkillOB;
        private enumBoolYesNo ynbScrambleMapOB;
        private enumBoolYesNo ynbScrambleEveryRoundOB;
        private int intwonTicketsOB;
        private string strScrambleMessageOB;
        private string ScrambleByOB;

        private enumBoolYesNo ynbenableSkillTDM;
        private enumBoolYesNo ynbScrambleMapTDM;
        private enumBoolYesNo ynbScrambleEveryRoundTDM;
        private int intwonTicketsTDM;
        private int intshowTicketsTDM;
        private string strScrambleMessageTDM;
        private string ScrambleByTDM;

        private bool m_isPluginEnabled;
        private bool boolplayerexists;
        private bool boolneedbalance;
        private bool booltimer;
        private bool boolstartBalance;
        private bool boolFirstOP;
        private bool boolLevelStart;
        private bool boolLevelLoaded;
        private bool boolRoundOver;
        private bool boolgametype;
        private bool boolafterBalance;
        private bool boolfirstwarningWL;
        private bool boolmanuellchange;
        private bool boolnoplayer;
        private bool boolscrambleActive;
        private bool boolscrambleNow;
        private bool boolTeamsScrambled;
        private bool boolRunOnList;
        private bool boolplayerleft;
        private bool boolticketdif;
        private bool boolwaitfordeath;
        private bool boolwaitdead;
        private bool boolfirstscrambler;
        private bool boolscramblefailed;
        private bool boolmessagesent;
        private bool backswitcher;
        private bool boolbalanced;
        private bool boolscramblebyadminroundend;
        private bool showfirstmove;

        private string strcurrentGametype;
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

        private bool boolVirtual;
        private enumBoolYesNo ynbVirtualMode;
        private int intMaxPlayersToFetch;

        private enumBoolYesNo showMoves;
        private enumBoolYesNo Check4Update;

        public TrueBalancer()
        {

            //lastupdatecheck = DateTime.Now.AddHours(-4);

            this.bclient = new BattlelogClient(this);

            this.dicPlayerCache = new Dictionary<string, CPlayerJoinInf>();
            this.dicPlayerScore = new Dictionary<int, CPlayerScoreInf>();
            this.dicSquadScore = new Dictionary<int, CSquadScoreInf>();
            this.dicSquadList = new Dictionary<string, int>();
            this.OnCommandMove = new Dictionary<string, bool>();
            this.OnCommandMoveDone = new List<string>();

            this.Servertype = "AUTOMATIC";

            //this.strAWhitelist = new string[] { "HRPanter", "Name two", "Name three" };
            //this.strAClantagWhitelist = new string[] { "FoC", "ClanTag2", "ClanTag3" };
            //this.strAClantagWhitelistScrambler = new string[] { "FoC", "FoCr", "ClanTag3" }; ;
            this.strAWhitelist = new string[] { "HRPanter" };
            this.strAClantagWhitelist = new string[] { "FoC" };
            this.strAClantagWhitelistScrambler = new string[] { "FoC", "FoCr" }; ;
            this.strAWhitelistComplete = new string[] { };
            this.teamswitcher = new List<string>();
            this.BalancedPlayers = new List<string>();

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
        private int _teamID = 0;
        private int _playerWL = 0;
        private int _playerSquad = 0;
        private DateTime _Playerjoined;
        private int _score = 0;
        private bool _tobebalanced = false;
        private double _rank = 0;
        private double _skill = 0;
        private double _spm = 0;
        private double _spmcombat = 0;
        private double _kdr = 0;
        private double _TBvalue = 0;
        private double _playerValue = 0;
        private string _tag = "";

        public bool statsFetched = false;

        private bool _IsCommander = false;
        private bool _IsSpectator = false;

        public int teamID
        {
            get { return _teamID; }
            set { _teamID = value; }
        }

        public int playerWL
        {
            get { return _playerWL; }
            set { _playerWL = value; }
        }

        public int playerSquad
        {
            get { return _playerSquad; }
            set { _playerSquad = value; }
        }

        public DateTime Playerjoined
        {
            get { return _Playerjoined; }
            set { _Playerjoined = value; }
        }

        public int score
        {
            get { return _score; }
            set { _score = value; }
        }

        public bool tobebalanced
        {
            get { return _tobebalanced; }
            set { _tobebalanced = value; }
        }

        public double rank
        {
            get { return _rank; }
            set { _rank = value; }
        }

        public double TBvalue
        {
            get { return _TBvalue; }
            set { _TBvalue = value; }
        }

        public double skill
        {
            get { return _skill; }
            set { _skill = value; }
        }

        public double spm
        {
            get { return _spm; }
            set { _spm = value; }
        }

        public double spmcombat
        {
            get { return _spmcombat; }
            set { _spmcombat = value; }
        }

        public double kdr
        {
            get { return _kdr; }
            set { _kdr = value; }
        }

        public double playerValue
        {
            get { return _playerValue; }
            set { _playerValue = value; }
        }

        public string tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public bool IsCommander
        {
            get { return _IsCommander; }
            set { _IsCommander = value; }
        }

        public bool IsSpectator
        {
            get { return _IsSpectator; }
            set { _IsSpectator = value; }
        }

        public CPlayerJoinInf(int teamID, int playerWL, int playerSquad, DateTime Playerjoined, int score, double rank, double skill, double spm, double spmcombat, double kdr, double TBvalue, double playerValue, string tag, bool tobebalanced, bool commander, bool spectator)
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
        private string _playerName = "";
        private int _teamID = 0;
        private int _playerSquad = 0;
        private double _playerValue = 0;
        private bool _balanced = false;
        private bool _scrambled = false;
        private string _tag = String.Empty;

        public string tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public int teamID
        {
            get { return _teamID; }
            set { _teamID = value; }
        }

        public int playerSquad
        {
            get { return _playerSquad; }
            set { _playerSquad = value; }
        }

        public double playerValue
        {
            get { return _playerValue; }
            set { _playerValue = value; }
        }
        public string playerName
        {
            get { return _playerName; }
            set { _playerName = value; }
        }

        public bool balanced
        {
            get { return _balanced; }
            set { _balanced = value; }
        }
        public bool scrambled
        {
            get { return _scrambled; }
            set { _scrambled = value; }
        }

        public CPlayerScoreInf(string playerName, int teamID, int playerSquad, double playerValue, bool balanced, bool scrambled, string tag)
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
        private int _teamID = 0;
        private int _squadID = 0;
        private double _squadScore = 0;
        private int _squadsize = 0;
        private bool _assigned = false;

        public bool assigned
        {
            get { return _assigned; }
            set { _assigned = value; }
        }

        public int teamID
        {
            get { return _teamID; }
            set { _teamID = value; }
        }

        public int squadsize
        {
            get { return _squadsize; }
            set { _squadsize = value; }
        }

        public int squadID
        {
            get { return _squadID; }
            set { _squadID = value; }
        }

        public double squadScore
        {
            get { return _squadScore; }
            set { _squadScore = value; }
        }

        public CSquadScoreInf(int teamID, int squadID, int squadsize, double squadScore, bool assigned)
        {
            _assigned = assigned;
            _teamID = teamID;
            _squadID = squadID;
            _squadScore = squadScore;
            _squadsize = squadsize;

        }
    }

    #endregion

#endregion

}
