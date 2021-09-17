using M.DBus.Utils.Canonical.DBusMenuFlags;

namespace M.DBus.Tray
{
    public class SeparatorEntry : SimpleEntry
    {
        public SeparatorEntry()
        {
            Type = EntryType.SSeparator;
        }
    }
}
