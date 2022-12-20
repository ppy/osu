using osu.Framework.Graphics;
using Tmds.DBus;

namespace osu.Desktop.DBus
{
    public abstract partial class DBusComponent : Drawable, IDBusObject
    {
        public abstract ObjectPath ObjectPath { get; }
    }
}
