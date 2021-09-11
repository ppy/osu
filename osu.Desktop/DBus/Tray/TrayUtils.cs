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

        public enum EntryEvent
        {
            Unknown,
            Opened,
            Closed,
            Clicked,
        }
    }
}
