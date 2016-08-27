//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using osu.Framework.Desktop;
using osu.Framework.Framework;

namespace osu.Desktop
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            BasicGameHost host = Host.GetSuitableHost();
            host.Load(new SampleGame());
            host.Run();
        }
    }
}
