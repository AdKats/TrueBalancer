using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
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
        #region Battlelog

        public class BattlelogClient
        {

            private TrueBalancer plugin = null;

            public BattlelogClient(TrueBalancer plugin)
            {
                this.plugin = plugin;
            }

            //private HttpWebRequest req = null;

            WebClient client = null;

            private void fetchWebPage(ref String html_data, String url)
            {
                try
                {

                    // Create a request for the URL.        
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    // Set Timeout
                    //plugin.DebugInfoSkill("Default timeout: " + request.Timeout);
                    request.Timeout = 12 * 1000; // 12 seconds
                    request.ReadWriteTimeout = 2 * 1000; // 2 seconds
                    request.KeepAlive = false;
                    /*
                    String h = "Headers: ";
                    for (int k = 0; k < request.Headers.Count; k++) {
                        h = h + request.Headers.GetKey(k) + ":" + request.Headers.Get(k) + ";";
                    }
                    plugin.DebugInfoSkill(h);
                    */
                    //plugin.DebugInfoSkill("New timeout: " + request.Timeout);
                    // Get the response.
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    // Display the status.
                    //plugin.DebugInfoSkill("HTTP Response: " + response.StatusDescription);
                    // Get the stream containing content returned by the server.
                    Stream dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    html_data = reader.ReadToEnd();
                    // Cleanup the streams and the response.
                    reader.Close();
                    dataStream.Close();
                    response.Close();

                    /*
                        if (client == null)
                            client = new WebClient();

                        html_data = client.DownloadString(url);
                        //return html_data;
                    */

                }
                catch (WebException e)
                {
                    if (e.Status.Equals(WebExceptionStatus.Timeout))
                        throw new Exception("HTTP request timed-out");
                    else
                        throw;

                }
            }

            public enum ServerType { BF3, BF4 };

            public class StatsException : Exception
            {
                public StatsException(String message)
                    : base(message)
                {
                }
            }

            public PlayerStats getPlayerStats(String player, ServerType st)
            {
                try
                {
                    /* First fetch the player's main page to get the persona id */
                    String result = "";

                    if (st == ServerType.BF3)
                    {
                        fetchWebPage(ref result, "http://battlelog.battlefield.com/bf3/user/" + player);
                    }
                    if (st == ServerType.BF4)
                    {
                        fetchWebPage(ref result, "http://battlelog.battlefield.com/bf4/user/" + player);
                    }

                    /* Extract the persona id */
                    MatchCollection pid = Regex.Matches(result, @"/soldier/" + player + @"/stats/(\d+)(/\w*)?/", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    String personaId = "";

                    foreach (Match m in pid)
                    {
                        if (m.Success && m.Groups[2].Value.Trim() != "/ps3" && m.Groups[2].Value.Trim() != "/xbox" && m.Groups[2].Value.Trim() != "/xbox360" && m.Groups[2].Value.Trim() != "/xboxone" && m.Groups[2].Value.Trim() != "/ps4")
                        {
                            personaId = m.Groups[1].Value.Trim();
                        }
                    }

                    if (personaId == "")
                        throw new Exception("could not find persona-id for ^b" + player);

                    PlayerStats ps = new PlayerStats();
                    ps.tag = extractClanTag(result, player);
                    if (st == ServerType.BF3)
                    {
                        fetchWebPage(ref result, "http://battlelog.battlefield.com/bf3/overviewPopulateStats/" + personaId + "/bf3-us-engineer/1/");
                    }
                    if (st == ServerType.BF4)
                    {
                        fetchWebPage(ref result, "http://battlelog.battlefield.com/bf4/warsawoverviewpopulate/" + personaId + "/1/");
                    }

                    Hashtable json = (Hashtable)JSON.JsonDecode(result);

                    // check we got a valid response
                    if (!(json.ContainsKey("type") && json.ContainsKey("message")))
                        throw new Exception("JSON response does not contain \"type\" or \"message\" fields");

                    String type = (String)json["type"];
                    String message = (String)json["message"];

                    /* verify we got a success message */
                    if (!(type.StartsWith("success") && message.StartsWith("OK")))
                        throw new Exception("JSON response was type=" + type + ", message=" + message);

                    /* verify there is data structure */
                    Hashtable data = null;
                    if (!json.ContainsKey("data") || (data = (Hashtable)json["data"]) == null)
                        throw new Exception("JSON response was does not contain a data field");

                    Hashtable stats = null;
                    if (!data.ContainsKey("overviewStats") || (stats = (Hashtable)data["overviewStats"]) == null)
                        throw new StatsException("^1^bERROR^0^n: JSON response ^bdata^n does not contain ^boverviewStats^n");

                    // get the data fields

                    if (st == ServerType.BF3)
                    {
                        if (stats.ContainsKey("rank"))
                            Double.TryParse(stats["rank"].ToString(), out ps.rank);

                        if (stats.ContainsKey("elo"))
                            Double.TryParse(stats["elo"].ToString(), out ps.skill);

                        Double combatScore = 0;
                        Double score = 0;
                        Double timePlayed = 0;
                        Double kills = 0;
                        Double deaths = 0;

                        if (stats.ContainsKey("combatScore"))
                            Double.TryParse(stats["combatScore"].ToString(), out combatScore);

                        if (stats.ContainsKey("score"))
                            Double.TryParse(stats["score"].ToString(), out score);

                        if (stats.ContainsKey("timePlayed"))
                            Double.TryParse(stats["timePlayed"].ToString(), out timePlayed);

                        if (stats.ContainsKey("kills"))
                            Double.TryParse(stats["kills"].ToString(), out kills);

                        if (stats.ContainsKey("deaths"))
                            Double.TryParse(stats["deaths"].ToString(), out deaths);

                        if (combatScore != 0 && timePlayed != 0)
                            ps.spmcombat = Math.Round(combatScore / (timePlayed / 60), 2);
                        else
                            ps.spmcombat = 0;

                        if (score != 0 && timePlayed != 0)
                            ps.spm = Math.Round(score / (timePlayed / 60), 2);
                        else
                            ps.spm = 0;

                        if (kills != 0 && deaths != 0)
                            ps.kdr = Math.Round(kills / deaths, 2);
                        else
                            ps.kdr = 0;
                    }
                    if (st == ServerType.BF4)
                    {
                        if (stats.ContainsKey("rank"))
                            Double.TryParse(stats["rank"].ToString(), out ps.rank);

                        if (stats.ContainsKey("skill"))
                            Double.TryParse(stats["skill"].ToString(), out ps.skill);

                        Double combatScore = 0;
                        Double score = 0;
                        Double timePlayed = 0;
                        Double kills = 0;
                        Double deaths = 0;

                        if (stats.ContainsKey("combatScore"))
                            Double.TryParse(stats["combatScore"].ToString(), out combatScore);

                        if (stats.ContainsKey("score"))
                            Double.TryParse(stats["score"].ToString(), out score);

                        if (stats.ContainsKey("timePlayed"))
                            Double.TryParse(stats["timePlayed"].ToString(), out timePlayed);

                        if (stats.ContainsKey("kills"))
                            Double.TryParse(stats["kills"].ToString(), out kills);

                        if (stats.ContainsKey("deaths"))
                            Double.TryParse(stats["deaths"].ToString(), out deaths);

                        if (combatScore != 0 && timePlayed != 0)
                            ps.spmcombat = Math.Round(combatScore / (timePlayed / 60), 2);
                        else
                            ps.spmcombat = 0;

                        if (score != 0 && timePlayed != 0)
                            ps.spm = Math.Round(score / (timePlayed / 60), 2);
                        else
                            ps.spm = 0;

                        if (kills != 0 && deaths != 0)
                            ps.kdr = Math.Round(kills / deaths, 2);
                        else
                            ps.kdr = 0;
                    }
                    //ps.statsFetched = true;
                    return ps;
                }
                catch (StatsException e)
                {
                    this.plugin.DebugInfoSkill("^8 StatsException (^b" + player + "^n): " + e.Message);
                }
                catch (Exception e)
                {
                    this.plugin.DebugInfoSkill("^8 Exception (^b" + player + "^n): " + e.Message);
                }

                return new PlayerStats();
            }

            public String extractClanTag(String result, String player)
            {
                /* Extract the player tag */
                Match tag = Regex.Match(result, @"\[\s*([a-zA-Z0-9]+)\s*\]\s*" + player, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (tag.Success)
                    return tag.Groups[1].Value;
                return String.Empty;
            }

        }

        public class PlayerStats
        {
            public Double rank;
            public Double skill;
            public Double spm;
            public Double spmcombat;
            public Double kdr;
            public String tag = String.Empty;
            public Boolean statsFetched = false;

            public void reset()
            {
                rank = 0;
                skill = 0;
                spm = 0;
                spmcombat = 0;
                kdr = 0;
                tag = String.Empty;

            }

        }

        #endregion
    }
}
