using OpenForestUI.Common;
using OpenForestUI.Ingame.Data.Replay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OpenForestUI.Ingame.Data.Provider
{
    class ReplayDataProvider
    {
        private static readonly HttpClient webClient;

        // Every member here is used STATICALLY (DetectPlaybackMode -> IsReplayActive -> GETGameAsync,
        // the replay clock override in DoTick, the seek/render POSTs). webClient used to be created
        // only in an INSTANCE constructor that is never called — nothing does `new ReplayDataProvider()`
        // (only IngameDataProvider is instantiated, as LoLDataProvider). So webClient stayed null, every
        // /replay/* call threw NullReferenceException -> caught -> returned null, and the client was
        // NEVER recognized as a replay (always misdetected as Live — which also disables the replay clock
        // and seek/rewind handling). A static constructor initializes it once, on first static access.
        static ReplayDataProvider()
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

        // True when the client is currently playing a .rofl replay.
        // /replay/game returns 200 only inside replay playback; in live games
        // (including spectator) it returns 404 / connection refused.
        public static async Task<bool> IsReplayActive()
        {
            return await GETGameAsync() != null;
        }

        // Convenience alias matching the planning doc; same semantics as GETPlaybackAsync.
        public static Task<Playback> GetPlaybackInfo()
        {
            return GETPlaybackAsync();
        }

        public static async Task<Game> GETGameAsync()
        {
            try
            {
                var response = webClient.GetAsync("https://127.0.0.1:2999/replay/game").Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Game>(result);
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

        public static async Task<Playback> GETPlaybackAsync()
        {
            try
            {
                var response = webClient.GetAsync("https://127.0.0.1:2999/replay/playback").Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Playback>(result);
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

        public static async Task<bool> POSTPlaybackAsync(Playback playback)
        {
            var content = new StringContent(JsonConvert.SerializeObject(playback), Encoding.UTF8, "application/json");
            var result = await webClient.PostAsync("https://127.0.0.1:2999/replay/playback", content);
            return result.IsSuccessStatusCode;

        }

        public static async Task<Render> GETRenderAsync()
        {
            try
            {
                var response = webClient.GetAsync("https://127.0.0.1:2999/replay/render").Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Log.Info(result);
                    return JsonConvert.DeserializeObject<Render>(result);
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

        public static async Task<bool> POSTRenderAsync(Render render)
        {
            var c = JsonConvert.SerializeObject(render);
            Log.Info(c);
            var content = new StringContent(c, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = await webClient.PostAsync("https://127.0.0.1:2999/replay/render", content);
            Log.Info("Render Post Result: " + result.StatusCode.ToString());
            return result.IsSuccessStatusCode;
        }
    }
}
