using System.Threading.Tasks;

namespace M.DBus.Tray
{
    public interface IHandleTrayManagement
    {
        void AddEntry(SimpleEntry entry);
        void AddEntryRange(SimpleEntry[] entries);
        void RemoveEntry(SimpleEntry entry);
        void RemoveEntryRange(SimpleEntry[] entries);
        Task<bool> ConnectToWatcher();
    }
}
