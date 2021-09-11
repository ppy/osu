namespace M.DBus.Tray
{
    public interface IHandleTrayManagement
    {
        void AddEntry(SimpleEntry entry);
        void RemoveEntry(SimpleEntry entry);
    }
}
