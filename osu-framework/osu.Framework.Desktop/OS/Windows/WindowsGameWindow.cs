//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Framework;
using OpenTK.Graphics;

namespace osu.Framework.Desktop.OS.Windows
{
    public class WindowsGameWindow : DesktopGameWindow
    {
        public WindowsGameWindow(GraphicsContextFlags flags)
            : base(flags)
        {
        }

        protected override BasicGameForm CreateGameForm(GraphicsContextFlags flags)
        {
            return new WindowsGameForm(flags);
        }
    }
}
