using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        #region 歌词处理

        private List<Lyric> parse(LyricResponseRoot lyricResponseRoot)
        {
            var result = new List<Lyric>();

            if (lyricResponseRoot?.Lyrics == null) return result;

            //先处理原始歌词信息
            foreach (var s in lyricResponseRoot.Lyrics)
            {
                //忽略目前暂时不会处理的信息
                if (skipProcess(s)) continue;

                //创建currentLrc
                var currentLrc = new Lyric();

                var ss = splitString(s);

                //获取歌词
                //设置原始歌词和歌词时间
                currentLrc.Content = ss[1];

                currentLrc.Time = toMs(ss[0]);

                //通过歌词时间查找翻译
                result.Add(currentLrc);
            }

            //再处理翻译歌词
            if (lyricResponseRoot.Tlyrics != null)
            {
                foreach (var tlyric in lyricResponseRoot.Tlyrics)
                {
                    if (skipProcess(tlyric)) continue;

                    var str = splitString(tlyric);
                    var tms = toMs(str[0]);

                    foreach (var lrc in result.FindAll(l => l.Time == tms))
                        lrc.TranslatedString = str[1];
                }
            }

            return result;
        }

        /// <param name="src">要处理的lrc信息</param>
        /// <returns>
        /// [0]: 时间信息"xx.xx.xx"<br/>
        /// [1]: 歌词</returns>
        private string[] splitString(string src)
        {
            string[] result = { "", "" };

            //时间
            result[0] = src.Split(']')
                           .First() //通过分割第一个向左中括号可以得到时间"[xx:xx.xx"
                           .Replace("[", "") //将"["去除
                           .Replace(":", "."); //替换":"为"."，节省后续时间

            //内容
            result[1] = src.Replace(
                src.Split(']').First() + ']',
                string.Empty);

            return result;
        }

        private int toMs(string src)
        {
            int result;
            var source = src.Split('.');

            try
            {
                result = int.Parse(source[0]) * 60000
                         + int.Parse(source[1]) * 1000
                         + int.Parse(source[2]);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"解析歌词行\"{src}\"失败");
                result = 0;
            }

            return result;
        }

        private readonly string[] noProcessStrings =
        {
            "by",
            "ti",
            "ar",
            "al",
            "offset"
        };

        private bool skipProcess(string s)
        {
            foreach (var nps in noProcessStrings)
            {
                if (s.StartsWith($"[{nps}", StringComparison.Ordinal)) return true;
            }

            if (string.IsNullOrEmpty(s)) return true;

            return false;
        }

        #endregion

        #region 歌词获取

        private OsuJsonWebRequest<ResponseRoot> currentSearchRequest;
        private OsuJsonWebRequest<LyricResponseRoot> currentLyricRequest;

        private CancellationTokenSource cancellationTokenSource;

        private UrlEncoder encoder;

        public void StartFetchByBeatmap(
            WorkingBeatmap beatmap,
            bool noLocalFile,
            Action<List<Lyric>> onFinish,
            Action<string> onFail)
        {
            if (!noLocalFile)
            {
                try
                {
                    if (storage.Exists($"custom/lyrics/beatmap-{beatmap.BeatmapSetInfo.ID}.json"))
                    {
                        onFinish?.Invoke(GetLyricFrom(beatmap));
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
            var title = beatmap.Metadata.TitleUnicode ?? beatmap.Metadata.Title;
            var artist = beatmap.Metadata.ArtistUnicode ?? beatmap.Metadata.Artist;
            var target = encoder.Encode($"{artist} {title}");

            var req = new OsuJsonWebRequest<ResponseRoot>(
                $"https://music.163.com/api/search/get/web?hlpretag=&hlposttag=&s={target}&type=1&total=true&limit=1");

            req.Finished += () => onRequestFinish(req.ResponseObject, onFinish, onFail);
            req.Failed += e =>
            {
                Logger.Error(e, "查询歌曲失败");
                onFail?.Invoke(e.ToString());
            };
            req.PerformAsync(cancellationTokenSource.Token);

            currentSearchRequest = req;
        }

        public void StartFetchById(int id, Action<List<Lyric>> onFinish, Action<string> onFail)
        {
            //处理之前的请求
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            var fakeResponse = new ResponseRoot
            {
                Result = new ResultInfo
                {
                    SongCount = 1,
                    Songs = new List<SongInfo>
                    {
                        new SongInfo
                        {
                            ID = id
                        }
                    }
                }
            };

            onRequestFinish(fakeResponse, onFinish, onFail);
        }

        private void onRequestFinish(ResponseRoot responseRoot, Action<List<Lyric>> onFinish, Action<string> onFail)
        {
            if (responseRoot.Result.SongCount <= 0)
            {
                onFail?.Invoke("未搜索到对应歌曲!");
                return;
            }

            var id = responseRoot.Result.Songs.First().ID;
            var target = $"https://music.163.com/api/song/lyric?os=pc&id={id}&lv=-1&kv=-1&tv=-1";
            var req = new OsuJsonWebRequest<LyricResponseRoot>(target);
            req.Finished += () => onFinish?.Invoke(parse(req.ResponseObject));
            req.Failed += e => Logger.Error(e, "获取歌词失败");
            req.PerformAsync(cancellationTokenSource.Token);

            currentLyricRequest = req;
        }

        #endregion

        #region 歌词读取、写入

        [Resolved]
        private Storage storage { get; set; }

        public List<Lyric> GetLyricFrom(WorkingBeatmap working)
        {
            try
            {
                var target = $"custom/lyrics/beatmap-{working.BeatmapSetInfo.ID}.json";

                var content = File.ReadAllText(storage.GetFullPath(target, true));

                if (string.IsNullOrEmpty(content))
                {
                    return new List<Lyric>();
                }

                var obj = JsonConvert.DeserializeObject<LyricResponseRoot>(content);

                return parse(obj);
            }
            catch (Exception e)
            {
                Logger.Error(e, "从本地获取歌词时发生了错误");
                throw;
            }
        }

        public void WriteLrcToFile(List<Lyric> lyrics, WorkingBeatmap working)
        {
            try
            {
                var target = $"custom/lyrics/beatmap-{working.BeatmapSetInfo.ID}.json";

                var lrc = new LyricInfo();
                var tLrc = new LyricInfo();

                foreach (var l in lyrics)
                {
                    var time = "[" + TimeSpan.FromMilliseconds(l.Time).ToString("mm\\:ss\\.fff") + "]";
                    lrc.RawLyric +=
                        time
                        + l.Content
                        + "\n";

                    tLrc.RawLyric +=
                        time
                        + l.TranslatedString
                        + "\n";
                }

                var serializeObject = JsonConvert.SerializeObject(new LyricResponseRoot
                {
                    RawLyric = lrc,
                    RawTLyric = tLrc
                });

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
