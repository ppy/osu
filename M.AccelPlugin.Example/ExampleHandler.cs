using System;
using System.Collections.Generic;
using osu.Game.Configuration.AccelUtils;

namespace M.AccelPlugin.Example
{
    public class ExampleHandler : IExtensionHandler
    {
        public string ExtensionName => "_BAR";

        public string[] SupportedProperties { get; } =
        {
            "FOO",
            "FOOO"
        };

        public string HandlerName => "扩展处理器";

        public bool Process(string name, ref object value, ref IList<string> errors)
        {
            if (name == "THROW!") throw new AsYouWishException("如你所愿");

            if (bool.TryParse(name, out bool b))
            {
                value = b ? "1b" : "0b";

                //转换完毕后返回true表示支持此值并处理成功
                return true;
            }

            if (float.TryParse(name, out float f))
            {
                value = f.ToString("N2") + "f";
                return true;
            }

            if (double.TryParse(name, out double d))
            {
                if (d != 114.514)
                {
                    errors.Add("不支持114.514");
                    //遇到不支持的值，返回false
                    return false;
                }

                //所有null值都将被替换为字符串"null"
                if (d == 1910.810)
                {
                    errors.Add("1910.810将返回null");
                    value = null;
                    return true;
                }

                value = d.ToString("N2") + "d";

                return true;
            }

            //默认返回false表示不支持处理此值
            return false;
        }

        private class AsYouWishException : Exception
        {
            public AsYouWishException(string msg)
                : base(msg)
            {
            }
        }
    }
}
