// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.Handlers;
using osu.Game.Beatmaps;
using osu.Game.Modes.Mods;
using osu.Game.Modes.Osu.Mods;
using osu.Game.Screens.Play;
using System;
using System.IO;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseReplay : TestCasePlayer
    {
        private WorkingBeatmap beatmap;

        private InputHandler replay;

        private Func<Stream> getReplayStream;

        public override string Description => @"Testing replay playback.";

        protected override Player CreatePlayer(WorkingBeatmap beatmap)
        {
            beatmap.Mods.Value = new Mod[] { new OsuModAutoplay() };

            return base.CreatePlayer(beatmap);
        }
    }
}
