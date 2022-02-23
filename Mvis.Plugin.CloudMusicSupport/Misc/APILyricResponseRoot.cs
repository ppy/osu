using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Markdig.Helpers;
using Newtonsoft.Json;

namespace Mvis.Plugin.CloudMusicSupport.Misc
{
    public class APILyricResponseRoot : IDisposable
    {
        [JsonProperty("lrc")]
        public LyricInfo LyricInfo;

        /// <summary>
        /// 网易云中歌词和翻译歌词是分开的，存储和下载歌词是用TLyricInfo获取翻译
        /// </summary>
        [JsonProperty("tlyric")]
        public LyricInfo TLyricInfo;

        [JsonProperty("localOffset")]
        public double LocalOffset;

        [CanBeNull]
        [JsonIgnore]
        public List<string> Lyrics => LyricInfo?.RawLyric?.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();

        [CanBeNull]
        [JsonIgnore]
        public List<string> Tlyrics => TLyricInfo?.RawLyric?.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();

        public List<Lyric> ToLyricList()
        {
            var result = new List<Lyric>();

            if (Lyrics == null) return result;

            //蠢办法，但起码比之前有用(
            //先处理原始歌词信息
            foreach (string lyricString in Lyrics)
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
                                Time = timeProperty.toMS()
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
            if (Tlyrics != null)
            {
                foreach (string tlyricString in Tlyrics)
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
                                times.Add(timeProperty.toMS());
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
