using System.Collections.Generic;

namespace osu.Game.Configuration.AccelUtils
{
    public class SayoExtensionHandler : IExtensionHandler
    {
        public string ExtensionName => Extensionname;
        public string HandlerName => Handlername;

        //绕过问题: 无法在非静态上下文中访问静态属性
        public static string Extensionname => "_SAYO";
        public static string Handlername => "Sayo转换器";

        public bool Process(string name, ref object value, ref IList<string> errors)
        {
            if (name == "NOVIDEO")
            {
                value = value != null && (bool)value
                    ? "novideo"
                    : "full";

                return true;
            }

            return false;
        }
    }
}
