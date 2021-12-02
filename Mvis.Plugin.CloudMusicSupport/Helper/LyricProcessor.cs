using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading;
using Markdig.Helpers;
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

            //蠢办法，但起码比之前有用(
            //先处理原始歌词信息
            foreach (string lyricString in lyricResponseRoot.Lyrics)
            {
                //创建currentLrc
                //可能存在一行歌词多个时间，所以先创建列表
                List<Lyric> lyrics = new List<Lyric>();

                //Logger.Log($"处理歌词: {lyricString}");

                bool propertyDetected = false;
                string propertyName = string.Empty;
                string lyricContent = string.Empty;

                //处理属性
                foreach (char c in lyricString)
                {
                    if (c == '[')
                    {
                        propertyDetected = true;
                        continue;
                    }

                    //如果检测到']'，那么退出属性检测并处理结果
                    if (c == ']' && propertyDetected)
                    {
                        propertyDetected = false;

                        //处理属性

                        //时间
                        string timeProperty = propertyName.Replace(":", ".");

                        //如果是时间属性
                        if (timeProperty[0].IsDigit())
                        {
                            lyrics.Add(new Lyric
                            {
                                Time = toMs(timeProperty)
                            });
                        }

                        //todo: 在此放置对其他属性的处理逻辑

                        //清空属性名称
                        propertyName = string.Empty;

                        //继续
                        continue;
                    }

                    //如果是属性，那么添加字符到propertyName，反之则是lyricContent
                    if (propertyDetected) propertyName = propertyName + c;
                    else lyricContent = lyricContent + c;

                    //Logger.Log($"原始歌词: propertyName: {propertyName} | lyricContent: {lyricContent}");
                }

                //最后，设置歌词内容并添加到result
                foreach (var lyric in lyrics)
                {
                    lyric.Content = lyricContent;
                    //Logger.Log($"添加歌词: {lyric}");

                    result.Add(lyric);
                }
            }

            //再处理翻译歌词
            if (lyricResponseRoot.Tlyrics != null)
            {
                foreach (string tlyricString in lyricResponseRoot.Tlyrics)
                {
                    bool propertyDetected = false;
                    string propertyName = string.Empty;
                    string lyricContent = string.Empty;

                    IList<int> times = new List<int>();

                    //Logger.Log($"处理翻译歌词: {tlyricString}");

                    //处理属性
                    foreach (char c in tlyricString)
                    {
                        if (c == '[')
                        {
                            propertyDetected = true;
                            continue;
                        }

                        //如果检测到']'，那么退出属性检测并处理结果
                        if (c == ']' && propertyDetected)
                        {
                            propertyDetected = false;

                            //处理属性

                            //时间
                            string timeProperty = propertyName.Replace(":", ".");

                            //如果是时间属性
                            if (timeProperty[0].IsDigit())
                            {
                                //添加当前时间到times
                                times.Add(toMs(timeProperty));
                            }

                            //todo: 在此放置对其他属性的处理逻辑

                            //清空属性名称
                            propertyName = string.Empty;

                            //继续
                            continue;
                        }

                        //如果是属性，那么添加字符到propertyName，反之则是lyricContent
                        if (propertyDetected) propertyName = propertyName + c;
                        else lyricContent = lyricContent + c;
                    }

                    foreach (int time in times)
                    {
                        foreach (var lrc in result.FindAll(l => l.Time == time))
                        {
                            lrc.TranslatedString = lyricContent;
                            //Logger.Log($"设置歌词歌词: {lrc}");
                        }
                    }
                }
            }

            result.Sort((l1, l2) => l2.CompareTo(l1));

            return result;
        }

        private int toMs(string src)
        {
            int result;
            string[] source = src.Split('.');

            try
            {
                result = int.Parse(source.ElementAtOrDefault(0) ?? "0") * 60000
                         + int.Parse(source.ElementAtOrDefault(1) ?? "0") * 1000
                         + int.Parse(source.ElementAtOrDefault(2) ?? "0");
            }
            catch (Exception e)
            {
                string reason = e.Message;

                if (e is FormatException)
                    reason = "格式有误, 请检查原歌词是否正确";

                Logger.Error(e, $"无法将\"{src}\"转换为歌词时间: {reason}");
                result = int.MaxValue;
            }

            return result;
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
            string title = beatmap.Metadata.TitleUnicode;
            string artist = beatmap.Metadata.ArtistUnicode;
            string target = encoder.Encode($"{artist} {title}");

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
            if ((responseRoot.Result?.SongCount ?? 0) <= 0)
            {
                onFail?.Invoke("未搜索到对应歌曲!");
                return;
            }

            int id = responseRoot.Result.Songs.First().ID;
            string target = $"https://music.163.com/api/song/lyric?os=pc&id={id}&lv=-1&kv=-1&tv=-1";
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
                string target = $"custom/lyrics/beatmap-{working.BeatmapSetInfo.ID}.json";

                string content = File.ReadAllText(storage.GetFullPath(target, true));

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
                string target = $"custom/lyrics/beatmap-{working.BeatmapSetInfo.ID}.json";

                var lrc = new LyricInfo();
                var tLrc = new LyricInfo();

                foreach (var l in lyrics)
                {
                    string time = "[" + TimeSpan.FromMilliseconds(l.Time).ToString("mm\\:ss\\.fff") + "]";
                    lrc.RawLyric +=
                        time
                        + l.Content
                        + "\n";

                    tLrc.RawLyric +=
                        time
                        + l.TranslatedString
                        + "\n";
                }

                string serializeObject = JsonConvert.SerializeObject(new LyricResponseRoot
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
