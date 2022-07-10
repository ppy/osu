using System;
using System.IO;
using System.Linq;
using Mvis.Plugin.CloudMusicSupport.Config;
using Mvis.Plugin.CloudMusicSupport.Misc.Mapping;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Online.API;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Mvis.Plugin.CloudMusicSupport.Helper
{
    public class UserDefinitionHelper : Component
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

        #region 更新定义

        private WebRequest currentRequest;

        private string filePath = "custom/lyrics/definition.json";

        public void UpdateDefinition(string url = null, Action onComplete = null, Action<Exception> onFail = null)
        {
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
            };

            req.Failed += onFail;

            currentRequest = req;
            req.PerformAsync().ConfigureAwait(false);
        }

        #endregion

        public bool HaveDefinition(int onlineID, out int neteaseID)
        {
            neteaseID = -1;

            if (onlineID == -1 || mappingRoot == null)
                return false;

            var result = mappingRoot.Data.FirstOrDefault(d => d.Beatmaps.Contains(onlineID));

            if (result != null) neteaseID = result.TargetNeteaseID;
            return result != null;
        }

        internal void Debug()
        {
            foreach (var mapping in mappingRoot.Data)
            {
                Logger.Log($"{mapping}包含的数据：");
                Logger.Log($"|------对应网易云ID：{mapping.TargetNeteaseID}");
                Logger.Log($"|------谱面：");

                foreach (var oid in mapping.Beatmaps)
                {
                    Logger.Log($"|------------{oid}");
                }
            }
        }
    }
}
