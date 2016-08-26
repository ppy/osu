//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT License - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Desktop;
using osu.Framework.Framework;
using osu.Game;

namespace osu.Desktop
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            BasicGameHost host = Host.GetSuitableHost();
            host.Load(new OsuGame());
            host.Run();
        }
    }
}
