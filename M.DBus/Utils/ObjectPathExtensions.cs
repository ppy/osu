using Tmds.DBus;

namespace M.DBus.Utils
{
    public static class ObjectPathExtensions
    {
        public static string ToServiceName(this ObjectPath path)
        {
            string str = path.ToString().Replace('/', '.').Remove(0, 1);

            if (str.EndsWith('.'))
                str = str.Remove(str.Length - 1, 1);

            return str;
        }
    }
}
