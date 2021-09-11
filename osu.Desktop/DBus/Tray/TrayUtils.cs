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
            => new Dictionary<string, object>
            {
                ["type"] = entry.Type,
                ["label"] = entry.Label,
                ["enabled"] = entry.Enabled,
                ["visible"] = entry.Visible,
                ["icon-name"] = entry.IconName,
                ["icon-data"] = entry.IconData,
                ["shortcuts"] = entry.Shortcuts,
                ["toggle-type"] = entry.ToggleType,
                ["toggle-state"] = entry.ToggleState,
                ["children-display"] = entry.ChildrenDisplay,
            };

        public enum EntryEvent
        {
            Unknown,
            Opened,
            Closed,
            Clicked,
        }
    }
}
