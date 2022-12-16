using System;
using System.IO;
using System.Linq;
using Mvis.Plugin.CloudMusicSupport.Config;
using Mvis.Plugin.CloudMusicSupport.Misc.Mapping;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Screens.LLin;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Mvis.Plugin.CloudMusicSupport.Helper
{
    public partial class UserDefinitionHelper : Component
    {
        private APIMappingRoot mappingRoot;

        #region 依赖

        [Resolved]
        private LyricConfigManager config { get; set; }

        [Resolved]
        private Storage storage { get; set; }

        #endregion

        [BackgroundDependencyLoader]
        private void load()
        {
            UpdateDefinition();
        }

        [Resolved]
        private IImplementLLin llin { get; set; }

        [Resolved]
        private LyricPlugin plugin { get; set; }

        #region 更新定义

        private WebRequest currentRequest;

        private string filePath = "custom/lyrics/definition.json";

        public void UpdateDefinition(string url = null, Action onComplete = null, Action<Exception> onFail = null)
        {
            void onRefreshComplete()
            {
                if (plugin.IsContentLoaded)
                {
                    this.Schedule(() =>
                    {
                        plugin.RefreshLyric();
                        llin.PostNotification(plugin, FontAwesome.Regular.CheckCircle, "用户定义更新完成");
                    });
                }
            }

            //检查本地
            if (storage.Exists(filePath))
            {
                var deserializedObject = JsonConvert.DeserializeObject<APIMappingRoot>(File.ReadAllText(storage.GetFullPath(filePath)));

                //检查日期
                //如果日期那里是-1，那么直接跳过时间检查
                if (deserializedObject != null && deserializedObject.LastUpdate != -1)
                {
                    int lastUpdate = TimeSpan.FromMilliseconds(deserializedObject.LastUpdate).Days;
                    int now = TimeSpan.FromMilliseconds(DateTime.Now.Millisecond).Days;

                    //如果在一周内更新或者LastUpdate是-2(调试模式)，那么跳过
                    if (now - lastUpdate < 7 || deserializedObject.LastUpdate == -1)
                    {
                        mappingRoot = deserializedObject;
                        onRefreshComplete();
                        return;
                    }
                }
            }

            //如果没有指定URL，那么从配置读取（自动更新）
            if (string.IsNullOrEmpty(url)) url = config.Get<string>(LyricSettings.UserDefinitionURL);

            //从网络上下载定义
            currentRequest?.Abort();

            var req = new OsuJsonWebRequest<APIMappingRoot>(url);

            req.Finished += () =>
            {
                if (currentRequest == req) currentRequest = null;

                mappingRoot = req.ResponseObject;

                File.WriteAllText(storage.GetFullPath(filePath), req.GetResponseString());

                onComplete?.Invoke();

                onRefreshComplete();
            };

            req.Failed += onFail;

            currentRequest = req;
            req.PerformAsync().ConfigureAwait(false);
        }

        #endregion

        public bool OnlineIDHaveDefinition(int onlineID, out int neteaseID)
        {
            neteaseID = -1;

            if (onlineID == -1 || mappingRoot == null)
                return false;

            var result = mappingRoot.Data.FirstOrDefault(d => d.Beatmaps.Contains(onlineID));

            if (result != null) neteaseID = result.TargetNeteaseID;
            return result != null;
        }

        public bool BeatmapMetaHaveDefinition(BeatmapInfo bi, out int neteaseID)
        {
            neteaseID = -1;

            if (mappingRoot == null || mappingRoot.Data.Length == 0) return false;

            var metadata = bi.Metadata;
            var result = mappingRoot.Data.FirstOrDefault(m =>
                (m.ArtistMatchMode == MatchingMode.Contains
                    ? m.MatchingArtist.Any(s => metadata.Artist.Contains(s) || (metadata.ArtistUnicode?.Contains(s) ?? false))
                    : m.MatchingArtist.Any(s => metadata.Artist.Equals(s) || (metadata.ArtistUnicode?.Equals(s) ?? false)))
                &&
                (m.TitleMatchMode == MatchingMode.Contains
                    ? m.MatchingTitle.Any(s => metadata.Title.Contains(s) || (metadata.TitleUnicode?.Contains(s) ?? false))
                    : m.MatchingTitle.Any(s => metadata.Title.Equals(s) || (metadata.TitleUnicode?.Equals(s) ?? false)))
            );

            if (result != null) neteaseID = result.TargetNeteaseID;

            return result != null;
        }

        internal void Debug()
        {
            foreach (var mapping in mappingRoot.Data)
            {
                string titleString = "";
                string artistString = "";

                foreach (string t in mapping.MatchingTitle)
                    titleString += $"\"{t}\", ";

                foreach (string a in mapping.MatchingArtist)
                    artistString += $"\"{a}\", ";

                Logger.Log($"{mapping}包含的数据：");
                Logger.Log($"|------对应网易云ID：{mapping.TargetNeteaseID}");
                Logger.Log($"|------对应标题：{titleString}");
                Logger.Log($"|------标题匹配模式：{mapping.TitleMatchMode}");
                Logger.Log($"|------艺术家：{artistString}");
                Logger.Log($"|------艺术家匹配模式：{mapping.ArtistMatchMode}");
                Logger.Log($"|------谱面：");

                foreach (int oid in mapping.Beatmaps)
                    Logger.Log($"|------------{oid}");
            }
        }
    }
}
