using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using M.DBus;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Online.API;

namespace osu.Game.Database
{
    public class AccelDownloadBeatmapSetRequest : ArchiveDownloadRequest<IBeatmapSetInfo>
    {
        private readonly bool minimiseDownloadSize;

        private readonly MConfigManager config;

        public AccelDownloadBeatmapSetRequest(IBeatmapSetInfo set, bool minimiseDownloadSize)
            : base(set)
        {
            this.minimiseDownloadSize = minimiseDownloadSize;
            config = MConfigManager.GetInstance();
        }

        private string getTarget() => $@"{(minimiseDownloadSize ? "novideo" : "full")}/{Model.OnlineID}";

        private string selectUri()
        {
            string result;

            var dict = new Dictionary<string, object>
            {
                ["BID"] = Model.OnlineID,
                ["NOVIDEO"] = (minimiseDownloadSize ? "novideo" : "full"),
                ["TARGET"] = getTarget()
            };

            if (!config.Get<string>(MSetting.AccelSource).TryParse(dict, out result, out _))
                throw new ParseFailedException("加速地址解析失败, 请检查您的设置。");

            return result;
        }

        protected override string Target => getTarget();

        protected override string Uri => selectUri();

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Timeout = 60000;
            return req;
        }

        protected override string FileExtension => ".osz";
    }

    public static class AccelExtensions
    {
        public static readonly string[] VAILD_PROPERTIES =
        {
            "BID",
            "NOVIDEO",
            "TARGET"
        };

        //尝试将给定的Url解析成加速地址
        public static bool TryParse(this string url, IDictionary<string, object> data, out string result, out List<string> errors)
        {
            bool propertyDetected = false;
            string propertyName = string.Empty;

            errors = new List<string>();
            result = url;

            if (string.IsNullOrEmpty(url))
            {
                errors.Add("地址为空");
                return false;
            }

            foreach (char c in url)
            {
                //如果检测到'['，那么进入属性检测
                if (c == '[' && !propertyDetected)
                {
                    propertyDetected = true;
                    continue;
                }

                //如果检测到']'，那么退出属性检测并处理结果
                if (c == ']' && propertyDetected)
                {
                    propertyDetected = false;

                    //如果属性名未知，跳过并添加错误信息
                    if (!VAILD_PROPERTIES.Contains(propertyName))
                    {
                        errors.Add($"未知的属性: {propertyName}");
                        propertyName = string.Empty;
                        continue;
                    }

                    //查询值
                    object value;
                    data.TryGetValue(propertyName, out value);

                    if (value == null)
                    {
                        throw new ArgumentNullException(
                            $"加速地址解析失败: 与 {propertyName} 对应的值是 null, 请将此错误报告给MATRIX-feather");
                    }

                    //替换字符串
                    result = result.Replace($"[{propertyName}]", value.ToString());

                    //清空属性名称
                    propertyName = string.Empty;

                    //继续
                    continue;
                }

                //添加字符到名称
                if (propertyDetected) propertyName = propertyName + c;
            }

            //检测未闭合的括号
            if (propertyDetected)
            {
                errors.Add("存在未闭合的括号");
                return false;
            }

            return true;
        }

        /// <summary>
        /// From <see cref="ServiceUtils.GetValueFor"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private static object getValueFor(string name, IBeatmapSetInfo info)
        {
            var members = info.GetType().GetMembers();

            try
            {
                object value = null;

                var targetMember = (MemberInfo)members.FirstOrDefault(m => m.Name == name);

                if (targetMember != null && targetMember is PropertyInfo propertyInfo)
                    value = propertyInfo.GetValue(info);

                return value;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"未能在{info}中查找 {name}: {e.Message}");
            }

            return "errno";
        }
    }

    public class ParseFailedException : Exception
    {
        public ParseFailedException(string s)
            : base(s)
        {
        }
    }
}
