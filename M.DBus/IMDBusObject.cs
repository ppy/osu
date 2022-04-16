using Tmds.DBus;

namespace M.DBus
{
    public interface IMDBusObject : IDBusObject
    {
        public string CustomRegisterName { get; }
    }
}
