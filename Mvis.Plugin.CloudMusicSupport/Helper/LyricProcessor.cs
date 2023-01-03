using System;
using System.Collections.Generic;
using System.IO;
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

namespace Mvis.Plugin.CloudMusicSupport.Helper
{
    public partial class LyricProcessor : Component
    {
        #region 歌词获取

        private APISearchRequest? currentSearchRequest;
        private APILyricRequest? currentLyricRequest;

        private CancellationTokenSource cancellationTokenSource = null!;

        private UrlEncoder? encoder;

        public void Search(SearchOption searchOption)
        {
            var beatmap = searchOption.Beatmap;

            if (beatmap == null) return;

            var onFinish = searchOption.OnFinish;
            var onFail = searchOption.OnFail;

            if (!searchOption.NoLocalFile)
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

            //处理要搜索的歌名: "标题 艺术家"
            string title = beatmap.Metadata.GetTitle();
            string artist = searchOption.NoArtist ? string.Empty : $" {beatmap.Metadata.GetArtist()}";
            string target = encoder.Encode($"{title}{artist}");

            var req = new APISearchRequest(target);

            req.Finished += () =>
            {
                var meta = RequestFinishMeta.From(req.ResponseObject, beatmap, onFinish, onFail, searchOption.TitleSimiliarThreshold);
                meta.NoRetry = searchOption.NoRetry;

                onRequestFinish(meta);
            };

            req.Failed += e =>
            {
                string message = "查询歌曲失败";

                if (e is HttpRequestException)
                    message += ", 未能送达http请求, 请检查当前网络以及代理";

                Logger.Error(e, message);
                onFail?.Invoke(e.ToString());
            };
            req.PerformAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            currentSearchRequest = req;
        }

        public void SearchByNeteaseID(int id, Action<APILyricResponseRoot> onFinish, Action<string> onFail, float titleThreshold)
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

            onRequestFinish(RequestFinishMeta.From(fakeResponse, null, onFinish, onFail, titleThreshold));
        }

        private void onRequestFinish(RequestFinishMeta meta)
        {
            if (!meta.Success)
            {
                //如果没成功，尝试使用标题重搜
                if (meta.SourceBeatmap != null && !meta.NoRetry)
                {
                    var searchMeta = SearchOption.FromRequestFinishMeta(meta);
                    searchMeta.NoArtist = true;
                    searchMeta.NoRetry = true;
                    searchMeta.NoLocalFile = true;

                    Logger.Log("精准搜索失败, 将尝试只搜索标题...", level: LogLevel.Important);
                    Search(searchMeta);
                }
                else
                    meta.OnFail?.Invoke("未搜索到对应歌曲!");

                return;
            }

            float similiarPrecentage = meta.GetSimiliarPrecentage();

            Logger.Log($"Beatmap: '{meta.SourceBeatmap?.Metadata.GetTitle() ?? "???"}' <-> '{meta.GetNeteaseTitle()}' -> {similiarPrecentage} < {meta.TitleSimiliarThreshold}");

            if (similiarPrecentage >= meta.TitleSimiliarThreshold)
            {
                var req = new APILyricRequest(meta.SongID);
                req.Finished += () => meta.OnFinish?.Invoke(req.ResponseObject);
                req.Failed += e => Logger.Error(e, "获取歌词失败");
                req.PerformAsync(cancellationTokenSource.Token).ConfigureAwait(false);

                currentLyricRequest = req;
            }
            else
            {
                Logger.Log("标题匹配失败, 将不会继续搜索歌词...", level: LogLevel.Important);

                Logger.Log($"对 {meta.SourceBeatmap?.Metadata.GetTitle() ?? "未知谱面"} 的标题匹配失败：");
                Logger.Log($"Beatmap: '{meta.SourceBeatmap?.Metadata.GetTitle() ?? "???"}' <-> '{meta.GetNeteaseTitle()}' -> {similiarPrecentage} < {meta.TitleSimiliarThreshold}");

                meta.OnFail?.Invoke("标题匹配失败, 将不会继续搜索歌词...");
            }
        }

        #endregion

        #region 歌词读取、写入

        [Resolved]
        private Storage storage { get; set; } = null!;

        public void WriteLrcToFile(APILyricResponseRoot? responseRoot, WorkingBeatmap working)
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
