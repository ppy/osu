//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.IO;
using System.Reflection;

namespace osu.Framework.Resources
{
    public class DllResourceStore : IResourceStore<byte[]>
    {
        private Assembly assembly;
        private string space;

        public DllResourceStore(string dllName)
        {
            assembly = Assembly.LoadFrom(dllName);
            space = Path.GetFileNameWithoutExtension(dllName);
        }

        public byte[] Get(string name)
        {
            using (Stream input = GetStream(name))
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public Stream GetStream(string name)
        {
            return assembly?.GetManifestResourceStream($@"{space}.{name.Replace('/', '.')}");
        }
    }
}
