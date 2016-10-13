//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.Chat.Display;
using osu.Framework;
using osu.Game.GameModes.Play;

namespace osu.Desktop.Tests
{
    class TestCasePlaySongSelect : TestCase
    {
        public override string Name => @"Song Select";
        public override string Description => @"Testing song selection UI";
        
        public override void Reset()
        {
            base.Reset();
            Add(new PlaySongSelect());
        }
    }
}
