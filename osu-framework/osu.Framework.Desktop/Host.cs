//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Desktop.OS.Linux;
using osu.Framework.Desktop.OS.Windows;
using osu.Framework.Framework;
using OpenTK.Graphics;

namespace osu.Framework.Desktop
{
    public static class Host
    {
        public static BasicGameHost GetSuitableHost()
        {
            BasicGameHost host = null;

            GraphicsContextFlags flags = GraphicsContextFlags.Default;
            if (RuntimeInfo.IsUnix)
                host = new LinuxGameHost(flags);
            else
                host = new WindowsGameHost(flags);

            return host;
        }
    }
}
