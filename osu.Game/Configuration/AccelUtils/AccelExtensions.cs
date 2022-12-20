using System;
using System.Collections.Generic;
using osu.Framework.Logging;
using osu.Game.Beatmaps;

namespace osu.Game.Configuration.AccelUtils
{
    public static class AccelExtensionsUtil
    {
        public static IDictionary<string, IExtensionHandler> ExtensionHandlers = new Dictionary<string, IExtensionHandler>
        {
            [ChimuExtensionHandler.Extensionname] = new ChimuExtensionHandler(),
            [SayoExtensionHandler.Extensionname] = new SayoExtensionHandler(),
            [NumberExtensionHandler.Extensionname] = new NumberExtensionHandler()
        };

        /// <summary>
        /// 所有已知的属性
        /// </summary>
        private static readonly IList<string> vaild_properties = new List<string>
        {
            "BID",
            "NOVIDEO"
        };

        /// <summary>
        /// 尝试将给定的Url解析成加速地址
        /// </summary>
        /// <param name="url">要解析的地址</param>
        /// <param name="data">一个字典，其中的键必须能在 <see cref="vaild_properties"/> 中查找到 </param>
        /// <param name="result">解析结果</param>
        /// <param name="errors">错误/警告列表</param>
        /// <param name="noSuggestion">禁用用户警告</param>
        /// <returns>解析是否成功</returns>
        /// <exception cref="ArgumentNullException">地址为空</exception>
        public static bool TryParseAccelUrl(this string url,
                                            IDictionary<string, object> data,
                                            out string result,
                                            out IList<string> errors,
                                            bool noSuggestion = false)
        {
            //0: 不在检测中
            //1: 属性名
            //2: 属性扩展类型
            int propertyDetectState = 0;
            (string name, string extensionName) propertyInfo = (string.Empty, string.Empty);

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
                if (c == '[' && propertyDetectState == 0)
                {
                    propertyDetectState = 1;
                    continue;
                }

                //如果检测到']'，那么退出属性检测并处理结果
                if (c == ']' && propertyDetectState >= 1)
                {
                    propertyDetectState = 0;

                    //替换值的时候使用
                    string propertyNameInput = propertyInfo.name;
                    propertyInfo.name = propertyInfo.name.ToUpperInvariant();

                    if (!vaild_properties.Contains(propertyInfo.name) && !noSuggestion)
                    {
                        string suggestion = "输入建议: ";
                        bool haveSuggestion = false;

                        foreach (string propertyName in vaild_properties)
                        {
                            if (propertyName.StartsWith(propertyInfo.name, StringComparison.Ordinal))
                            {
                                haveSuggestion = true;
                                suggestion += $" {propertyName}";
                            }
                        }

                        errors.Add(haveSuggestion ? suggestion : $"未知的属性: {propertyInfo.name}");
                    }

                    //查询值
                    object value;
                    data.TryGetValue(propertyInfo.name, out value!);

                    //如果有扩展名，则将值交由其对应的处理器处理
                    if (!string.IsNullOrEmpty(propertyInfo.extensionName))
                    {
                        //查询支持的处理器
                        IExtensionHandler? handler;
                        ExtensionHandlers.TryGetValue(propertyInfo.extensionName, out handler);

                        //如果没查到，则转换到大写再试一次
                        if (handler == null)
                            ExtensionHandlers.TryGetValue(propertyInfo.extensionName.ToUpperInvariant(), out handler);

                        //如果还是没有
                        if (!noSuggestion)
                        {
                            string suggestion = "输入建议: ";
                            bool haveSuggestion = false;

                            foreach (var hdlr in ExtensionHandlers.Values)
                            {
                                if (hdlr.ExtensionName.StartsWith(propertyInfo.extensionName, StringComparison.Ordinal))
                                {
                                    haveSuggestion = true;
                                    suggestion += $" {hdlr.ExtensionName}({hdlr.HandlerName})";
                                }
                            }

                            errors.Add(haveSuggestion ? suggestion : $"未找到与 {propertyInfo.extensionName} 对应的处理器, 它将保持原始状态");
                        }

                        if (handler != null)
                        {
                            try
                            {
                                if (!handler.Process(propertyInfo.name, ref value, ref errors))
                                    errors.Add($"处理器\"{handler.HandlerName}\"不支持属性{propertyInfo.name}");
                            }
                            catch (Exception e)
                            {
                                errors.Add($"处理器 \"{handler.HandlerName}\" 产生了异常: {e.Message}");

                                Logger.Log($"处理器 \"{handler.HandlerName}\" 产生了异常: {e.Message}");
                                Logger.Log(e.StackTrace);
                                return false;
                            }
                        }
                    }

                    //如果value是null，则将其赋值为"null"
                    value ??= "null";
                    result = result.Replace($"[{propertyNameInput}{propertyInfo.extensionName}]", value.ToString());

                    //清空属性名称
                    propertyInfo.name = string.Empty;
                    propertyInfo.extensionName = string.Empty;

                    //继续
                    continue;
                }

                //如果检测到'_’，进入类型检测
                if (c == '_' && propertyDetectState == 1)
                    propertyDetectState = 2;

                //添加字符到名称
                if (propertyDetectState == 1) propertyInfo.name = propertyInfo.name + c;
                if (propertyDetectState == 2) propertyInfo.extensionName = propertyInfo.extensionName + c;
            }

            //检测未闭合的括号
            if (propertyDetectState != 0)
            {
                errors.Add("存在未闭合的括号");
                return false;
            }

            if (errors.Count > 5)
                errors.Add("错误/警告有点多, 您确定这是正确的地址吗?");

            return true;
        }

        /// <summary>
        /// 尝试将给定的Url解析成加速地址
        /// </summary>
        /// <param name="url">要解析的地址</param>
        /// <param name="beatmapInfo">谱面信息，传入此值将自动生成字典</param>
        /// <param name="result">解析结果</param>
        /// <param name="errors">错误/警告列表</param>
        /// <param name="overrides">覆盖内容，将用于替换或补全自动生成字典中的值</param>
        /// <param name="noSuggestion">禁用用户警告</param>
        /// <returns>解析是否成功</returns>
        /// <exception cref="ArgumentNullException">地址为空</exception>
        public static bool TryParseAccelUrl(this string url,
                                            IBeatmapInfo beatmapInfo,
                                            out string result,
                                            out IList<string> errors,
                                            IDictionary<string, object> overrides = null,
                                            bool noSuggestion = false)
        {
            var dict = new Dictionary<string, object>
            {
                ["BID"] = beatmapInfo.OnlineID,
                ["NOVIDEO"] = OsuConfigManager.Instance.Get<bool>(OsuSetting.PreferNoVideo)
            };

            if (overrides != null)
            {
                foreach (var kvp in overrides)
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }

            return url.TryParseAccelUrl(dict, out result, out errors, noSuggestion);
        }

        /// <summary>
        /// 尝试将给定的Url解析成加速地址
        /// </summary>
        /// <param name="url">要解析的地址</param>
        /// <param name="beatmapSetInfo">谱面信息，传入此值将自动生成字典</param>
        /// <param name="result">解析结果</param>
        /// <param name="errors">错误/警告列表</param>
        /// <param name="overrides">覆盖内容，将用于替换或补全自动生成字典中的值</param>
        /// <param name="noSuggestion">禁用用户警告</param>
        /// <returns>解析是否成功</returns>
        /// <exception cref="ArgumentNullException">地址为空</exception>
        public static bool TryParseAccelUrl(this string url,
                                            IBeatmapSetInfo beatmapSetInfo,
                                            out string result,
                                            out IList<string> errors,
                                            IDictionary<string, object> overrides = null,
                                            bool noSuggestion = false)
        {
            var dict = new Dictionary<string, object>
            {
                ["BID"] = beatmapSetInfo.OnlineID,
                ["NOVIDEO"] = OsuConfigManager.Instance.Get<bool>(OsuSetting.PreferNoVideo)
            };

            if (overrides != null)
            {
                foreach (var kvp in overrides)
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }

            return url.TryParseAccelUrl(dict, out result, out errors, noSuggestion);
        }

        public static bool AddHandler(IExtensionHandler handler)
        {
            if (ExtensionHandlers.ContainsKey(handler.ExtensionName)) return false;

            ExtensionHandlers.Add(handler.ExtensionName, handler);

            foreach (string propertyName in handler.SupportedProperties)
            {
                if (!vaild_properties.Contains(propertyName))
                    vaild_properties.Add(propertyName);
            }

            return true;
        }
    }
}
