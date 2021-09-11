using System.Collections.Generic;
using M.DBus.Tray;

namespace osu.Desktop.DBus.Tray
{
    public class TrayUtils
    {
        public static EntryEvent GetEntryEventType(string message)
        {
            switch (message)
            {
                case "opened":
                    return EntryEvent.Opened;

                case "closed":
                    return EntryEvent.Closed;

                case "clicked":
                    return EntryEvent.Clicked;

                default:
                    return EntryEvent.Unknown;
            }
        }

        public static IDictionary<string, object> ToDictionary(SimpleEntry entry)
        {
            var dict = new Dictionary<string, object>
            {
                ["label"] = entry.Name,
                ["enabled"] = entry.Enabled,
                ["visible"] = entry.Visible,
                ["toggle-state"] = entry.ToggleState,
                ["visible"] = entry.Visible
            };

            entry.AfterCast(dict);
            return dict;
        }

        public enum EntryEvent
        {
            Unknown,
            Opened,
            Closed,
            Clicked,
        }
    }
}
