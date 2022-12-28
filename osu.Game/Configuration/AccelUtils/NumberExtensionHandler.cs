using System.Collections.Generic;

#nullable disable

namespace osu.Game.Configuration.AccelUtils
{
    public class NumberExtensionHandler : IExtensionHandler
    {
        public string ExtensionName => Extensionname;
        public string[] SupportedProperties => null;
        public string HandlerName => Handlername;

        //绕过问题: 无法在非静态上下文中访问静态属性
        public static string Extensionname => "_NUMBER";
        public static string Handlername => "Bool转Int";

        public bool Process(string name, ref object value, ref IList<string> errors)
        {
            if (value is bool)
            {
                value = (bool)value ? 1 : 0;
                return true;
            }

            errors.Add($"与{name}对应的值不是True或False");
            return false;
        }
    }
}
