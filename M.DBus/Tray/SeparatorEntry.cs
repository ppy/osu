using System.Collections.Generic;

namespace M.DBus.Tray
{
    public class SeparatorEntry : SimpleEntry
    {
        public override void AfterCast(IDictionary<string, object> dictionary)
        {
            dictionary["type"] = "separator";
            base.AfterCast(dictionary);
        }
    }
}
