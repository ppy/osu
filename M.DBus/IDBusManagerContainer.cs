using M.DBus.Services.Notifications;
using M.DBus.Tray;
using Tmds.DBus;

namespace M.DBus
{
    public interface IDBusManagerContainer<T>
        where T : IDBusObject
    {
        void Add(T obj);
        void Remove(T obj);
        void AddRange(T[] objects);
        void RemoveRange(T[] objects);
        void PostSystemNotification(SystemNotification notification);
        void AddTrayEntry(SimpleEntry entry);
        void RemoveTrayEntry(SimpleEntry entry);
    }
}
