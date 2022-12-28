using System;
using System.Collections.Generic;

#nullable disable

namespace osu.Game.Configuration.AccelUtils
{
    //https://chimu.moe/zh-CN/docs
    public class ChimuExtensionHandler : IExtensionHandler
    {
        public string ExtensionName => Extensionname;
        public string[] SupportedProperties => null;
        public string HandlerName => Handlername;

        //绕过问题: 无法在非静态上下文中访问静态属性
        public static string Extensionname => "_CHIMU";
        public static string Handlername => "Chimu转换器";

        public bool Process(string name, ref object value, ref IList<string> errors)
        {
            try
            {
                //chimu的
                if (name == "NOVIDEO")
                {
                    value = value != null && (bool)value
                        ? "?n=0"
                        : "?n=1";

                    return true;
                }
            }
            catch (Exception e)
            {
                errors.Add($"转换失败: {e.Message}");
                value = null;
                return false;
            }

            return false;
        }
    }
}
