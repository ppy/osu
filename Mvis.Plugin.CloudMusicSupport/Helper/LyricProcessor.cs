using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading;
using Mvis.Plugin.CloudMusicSupport.Misc;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Online.API;

namespace Mvis.Plugin.CloudMusicSupport.Helper
{
    public class LyricProcessor : Component
    {
        #region 歌词获取

        private OsuJsonWebRequest<APISearchResponseRoot> currentSearchRequest;
        private OsuJsonWebRequest<APILyricResponseRoot> currentLyricRequest;

        private CancellationTokenSource cancellationTokenSource;

        private UrlEncoder encoder;

        public void StartFetchByBeatmap(
            WorkingBeatmap beatmap,
            bool noLocalFile,
            Action<APILyricResponseRoot> onFinish,
            Action<string> onFail)
        {
            if (!noLocalFile)
            {
                try
                {
                    string filePath = $"custom/lyrics/beatmap-{beatmap.BeatmapSetInfo.ID}.json";

                    string content = File.ReadAllText(storage.GetFullPath(filePath, true));

                    var deserializeObject = JsonConvert.DeserializeObject<APILyricResponseRoot>(content);

                    if (deserializeObject != null)
                    {
                        onFinish?.Invoke(deserializeObject);
                        return;
                    }
                }
                catch
                {
                    //忽略异常
                }
            }

            encoder ??= UrlEncoder.Default;

            //处理之前的请求
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            currentSearchRequest?.Dispose();
            currentLyricRequest?.Dispose();

            //处理要搜索的歌名: "艺术家 标题"
            string title = beatmap.Metadata.TitleUnicode;
            string artist = beatmap.Metadata.ArtistUnicode;
            string target = encoder.Encode($"{artist} {title}");

            var req = new OsuJsonWebRequest<APISearchResponseRoot>(
                $"https://music.163.com/api/search/get/web?hlpretag=&hlposttag=&s={target}&type=1&total=true&limit=1");

            req.Finished += () => onRequestFinish(req.ResponseObject, onFinish, onFail);
            req.Failed += e =>
            {
                string message = "查询歌曲失败";

                if (e is HttpRequestException)
                    message += ", 未能送达http请求, 请检查当前网络以及代理";

                Logger.Error(e, message);
                onFail?.Invoke(e.ToString());
            };
            req.PerformAsync(cancellationTokenSource.Token);

            currentSearchRequest = req;
        }

        public void StartFetchById(int id, Action<APILyricResponseRoot> onFinish, Action<string> onFail)
        {
            //处理之前的请求
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            var fakeResponse = new APISearchResponseRoot
            {
                Result = new APISearchResultInfo
                {
                    SongCount = 1,
                    Songs = new List<APISongInfo>
                    {
                        new APISongInfo
                        {
                            ID = id
                        }
                    }
                }
            };

            onRequestFinish(fakeResponse, onFinish, onFail);
        }

        private void onRequestFinish(APISearchResponseRoot responseRoot, Action<APILyricResponseRoot> onFinish, Action<string> onFail)
        {
            if ((responseRoot.Result?.SongCount ?? 0) <= 0)
            {
                onFail?.Invoke("未搜索到对应歌曲!");
                return;
            }

            int id = responseRoot.Result.Songs.First().ID;
            string target = $"https://music.163.com/api/song/lyric?os=pc&id={id}&lv=-1&kv=-1&tv=-1";
            var req = new OsuJsonWebRequest<APILyricResponseRoot>(target);
            req.Finished += () => onFinish?.Invoke(req.ResponseObject);
            req.Failed += e => Logger.Error(e, "获取歌词失败");
            req.PerformAsync(cancellationTokenSource.Token);

            currentLyricRequest = req;
        }

        #endregion

        #region 歌词读取、写入

        [Resolved]
        private Storage storage { get; set; }

        public void WriteLrcToFile(APILyricResponseRoot responseRoot, WorkingBeatmap working)
        {
            try
            {
                string target = $"custom/lyrics/beatmap-{working.BeatmapSetInfo.ID}.json";

                string serializeObject = JsonConvert.SerializeObject(responseRoot);

                File.WriteAllText(storage.GetFullPath(target, true), serializeObject);
            }
            catch (Exception e)
            {
                Logger.Error(e, "写入歌词时发生了错误");
            }
        }

        #endregion
    }
}
