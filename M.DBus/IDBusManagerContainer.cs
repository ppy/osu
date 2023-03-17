using System.Collections.Generic;
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
        void AddRange(IEnumerable<T> objects);
        void RemoveRange(IEnumerable<T> objects);
        void PostSystemNotification(SystemNotification notification);
        void AddTrayEntry(SimpleEntry entry);
        void RemoveTrayEntry(SimpleEntry entry);
    }
}
