using OpenForestUI.Common;
using OpenForestUI.Ingame.Data.RIOT;
using OpenForestUI.Ingame.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenForestUI.Ingame.Data.Provider
{
    public enum PlaybackMode
    {
        // Live ranked/normal play. Vanguard-compatible fork has no path here —
        // /playerlist only exposes the local player's data, so the overlay is unusable.
        Live,
        // Live spectator (Custom Game spectator slot). All 10 players visible.
        Spectator,
        // .rofl replay playback. /replay/* endpoints are reachable.
        Replay,
    }

    class IngameDataProvider
    {
        public static HttpClient webClient;

        public IngameDataProvider()
        {
            Init();
        }
        private void Init()
        {
            var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                }
            };

            webClient = new HttpClient(handler);
            webClient.DefaultRequestHeaders.ExpectContinue = true;
            webClient.DefaultRequestHeaders.Add("User-Agent", "OpenForestUI");
            webClient.Timeout = TimeSpan.FromSeconds(0.25);
        }

        public async Task<GameMetaData> GetGameData()
        {
            try
            {
                var response = webClient.GetAsync("https://127.0.0.1:2999/liveclientdata/gamestats").Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<GameMetaData>(result);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }

        }

        public async Task<List<Player>> GetPlayerData()
        {
            HttpResponseMessage response = await webClient.GetAsync("https://127.0.0.1:2999/liveclientdata/playerlist");
            if (!response.IsSuccessStatusCode)
            {
                //Game Not Running
                return null;
            }
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Player>>(result).ToList();
        }

        public async Task<List<RiotEvent>> GetEventData()
        {
            HttpResponseMessage response = await webClient.GetAsync("https://127.0.0.1:2999/liveclientdata/eventdata");
            if (!response.IsSuccessStatusCode)
            {
                //Game Not Running
                return null;
            }

            var result = await response.Content.ReadAsStringAsync();

            var events = JsonConvert.DeserializeObject<RiotEventList>(result).Events;
            return events;
        }

        // Decide which kind of game this is by probing the local Riot endpoints.
        // Order matters: replay should win over spectator because /replay/game also
        // implies an active client, and we want playback time to take over gameTime.
        public async Task<PlaybackMode> DetectPlaybackMode()
        {
            if (await ReplayDataProvider.IsReplayActive())
            {
                Log.Info("Detected playback mode: Replay");
                return PlaybackMode.Replay;
            }

            if (await IsSpectatorGame())
            {
                Log.Info("Detected playback mode: Spectator");
                return PlaybackMode.Spectator;
            }

            Log.Info("Detected playback mode: Live (overlay features will be limited without memory reader)");
            return PlaybackMode.Live;
        }

        public async Task<bool> IsSpectatorGame(int tries = 0)
        {
            if(tries == 10)
            {
                Log.Warn("Could not determine if active game is spectator game. Defaulting to no to protect against usage in live games");
                return false;
            }
            try
            {
                Log.Verbose("Checking spectate endpoint to determine game type");
                HttpResponseMessage response = await webClient.GetAsync("https://127.0.0.1:2999/liveclientdata/activeplayername");
                string res = await response.Content.ReadAsStringAsync();
                if (res.Trim() == "\"\"")
                {
                    Log.Verbose("Found spectate game");
                    return true;
                }
                Log.Verbose("Found live game");
                return false;
            } catch (Exception e)
            {
                Log.Verbose($"Spectate endpoint connection error: {e.Message} \n Attempting again");
                return await IsSpectatorGame(tries++);
            }
            
        }
    }
}
