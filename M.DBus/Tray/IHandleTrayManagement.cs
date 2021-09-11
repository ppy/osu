namespace M.DBus.Tray
{
    public interface IHandleTrayManagement
    {
        void AddEntry(SingleEntry entry);
        void RemoveEntry(SingleEntry entry);
    }
}
