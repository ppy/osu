//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Framework;
using OpenTK.Graphics;

namespace osu.Framework.Desktop.OS.Linux
{
    public class LinuxGameWindow : DesktopGameWindow
    {
        public LinuxGameWindow(GraphicsContextFlags flags)
            : base(flags)
        {
        }

        protected override BasicGameForm CreateGameForm(GraphicsContextFlags flags)
        {
            return new LinuxGameForm(flags);
        }
    }
}
