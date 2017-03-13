// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Framework.Input.Handlers;
using osu.Game.Beatmaps;
using osu.Game.Modes;
using osu.Game.Screens.Play;

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
            var player = base.CreatePlayer(beatmap);
            player.ReplayInputHandler = Ruleset.GetRuleset(beatmap.PlayMode).CreateAutoplayScore(beatmap.Beatmap)?.Replay?.GetInputHandler();
            return player;
        }
    }
}
