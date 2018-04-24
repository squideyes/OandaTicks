// Copyright 2017 Louis S. Berman.
//
// This file is part of OandaTicks.
//
// OandaTicks is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OandaTicks is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OandaTicks.  If not, see <http://www.gnu.org/licenses/>.

using Newtonsoft.Json;
using Nito.AsyncEx;
using NLog;
using OandaTicks.Common.Helpers;
using OandaTicks.Common.Trading;
using OandaTicks.Library;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OandaTicks.WinService
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            try
            {
                AuthInfo authInfo;
                bool showHelp;

                (authInfo, showHelp) = ParseAndHandleArgs(args);

                if (showHelp)
                    ShowHelp();
                else if (authInfo != null)
                    AsyncContext.Run(() => DoWork(authInfo));

                Environment.ExitCode = 1;
            }
            catch (Exception error)
            {
                logger.Error(error);

                Environment.ExitCode = -1;
            }
        }

        private static void ShowHelp()
        {
            const string HEADER = "Saves the OANDA pricing stream as CSV-format tick files to the local system in one-minute chunks for later upload to Azure.";
            const string AUTHINFO = "The UNC path of an existing AuthInfo.json file.  If an AuthInfo.json file was previously saved it will be overwritten.  In any case, the file will be persisted to local storage using DPAPI security.";
            const string DELETEAUTHINFO = "Indicates that any previously saved AuthInfo.json data should be permanently deleted.";

            var sb = new StringBuilder();

            foreach (var line in HEADER.Wrap(76))
                sb.AppendLine(line);

            sb.AppendLine();
            sb.AppendLine("OANDATICKS [<AUTHINFO PATH>] [/DELETEAUTHINFO]");
            sb.AppendLine();
            sb.Append(GetWrapped("<AUTHINFO PATH>", AUTHINFO));
            sb.AppendLine();
            sb.Append(GetWrapped("/DELETEAUTHINFO", DELETEAUTHINFO));
            sb.AppendLine();

            Console.WriteLine();
            Console.WriteLine(sb.ToString());
        }

        private static string GetWrapped(string prefix, string text)
        {
            var sb = new StringBuilder();

            foreach (var line in text.Wrap(60))
            {
                if (sb.Length == 0)
                    sb.Append(prefix.PadRight(17));
                else
                    sb.Append(new string(' ', 17));

                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        private static async Task DoWork(AuthInfo authInfo)
        {
            var context = new Context(
                authInfo.Environ, authInfo.AccountId, authInfo.AccessToken);

            var client = new OandaClient(
                context, Properties.Settings.Default.TimeoutSeconds);

            client.OnBidAsk += (s, e) => logger.Trace(e.BidAsk);

            client.OnChunk += (s, e) =>
            {
                var savedTo = e.Data.Save(WellKnown.AppInfo.GetLocalAppDataPath());

                logger.Info($"CHUNK ({e.Data.Symbol}): {e.Data.Count} Bid/Asks saved to \"{savedTo}\"");
            };

            client.OnBadPrice += (s, e) =>
            {
                // TODO: Handle multiple bad prices within window

                logger.Warn($"BAD PRICE: {e.Json}");
            };

            client.OnHeartbeat += (s, e) => logger.Trace(
                $"HEARTBEAT ({e.Kind}): {e.DateTimeUtc.ToText()}");

            client.OnStatus += (s, e) =>
            {
                switch(e.Status)
                {
                    case StreamStatus.Connecting:
                    case StreamStatus.Disconnecting:
                        logger.Debug($"STATUS ({e.Kind}): {e.Status}");
                        break;
                    case StreamStatus.Reconnecting:
                    case StreamStatus.TimedOut:
                    case StreamStatus.WebError:
                        logger.Warn($"STATUS ({e.Kind}): {e.Status}");
                        break;
                    default:
                        logger.Info($"STATUS ({e.Kind}): {e.Status}");
                        break;
                }
            };

            client.OnWebError += (s, e) => logger.Error(
                $"ERROR ({e.Kind}, {e.Status}): {e.Message}");

            await client.UpdateAssetLimitsAsync();

            logger.Info("Updated Asset Limits");

            await client.ConnectAsync(new EnumList<Symbol>().ToList());
        }

        private static (AuthInfo, bool) ParseAndHandleArgs(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    var json = new SafeConfig()
                        .WithFileName(WellKnown.SettingsFileName)
                        .WithLocalMachineScope()
                        .Load()
                        .Get<string>(WellKnown.AuthInfoName);

                    var authInfo = JsonConvert.DeserializeObject<AuthInfo>(json);

                    logger.Debug($"Read \"{WellKnown.SettingsFileName}\" using DPAPI");

                    return (authInfo, false);
                }

                if (args.Length != 1)
                    return (null, true);

                if (File.Exists(args[0]))
                {
                    using (var reader = new StreamReader(args[0]))
                    {
                        var json = reader.ReadToEnd();

                        var authInfo = JsonConvert.DeserializeObject<AuthInfo>(json);

                        new SafeConfig()
                            .WithFileName(WellKnown.SettingsFileName)
                            .WithLocalMachineScope()
                            .Set(WellKnown.AuthInfoName, json)
                            .Save();
                    }

                    Console.WriteLine($"Saved \"{WellKnown.SettingsFileName}\" using DPAPI");

                    return (null, false);
                }
                else if (args[0].Equals(
                    "/DELETEAUTHINFO", StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(WellKnown.SettingsFileName);

                    Console.WriteLine($"Deleted \"{WellKnown.SettingsFileName}\"");

                    return (null, false);
                }
            }
            catch
            {
            }

            return (null, true);
        }
    }
}