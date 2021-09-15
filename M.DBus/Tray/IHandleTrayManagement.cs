using System.Threading.Tasks;

namespace M.DBus.Tray
{
    public interface IHandleTrayManagement
    {
        void AddEntry(SimpleEntry entry);
        void RemoveEntry(SimpleEntry entry);
        Task<bool> ConnectToWatcher();
    }
}
