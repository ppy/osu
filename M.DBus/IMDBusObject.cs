using Tmds.DBus;

namespace M.DBus
{
    public interface IMDBusObject : IDBusObject
    {
        /// <summary>
        /// Service name
        /// </summary>
        public string? CustomRegisterName { get; }

        public bool IsService { get; }
    }
}
