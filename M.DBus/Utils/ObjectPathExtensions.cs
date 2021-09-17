using Tmds.DBus;

namespace M.DBus.Utils
{
    public static class ObjectPathExtensions
    {
        public static string ToServiceName(this ObjectPath path)
            => path.ToString().Replace('/', '.').Remove(0, 1);
    }
}
