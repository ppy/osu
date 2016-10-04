//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Desktop;
using osu.Framework.OS;
using osu.Game;

namespace osu.Desktop
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            BasicGameHost host = Host.GetSuitableHost("osu");
            host.Load(new OsuGame());
            host.Run();
        }
    }
}
