// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Screens.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using OpenTK;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.MathUtils;
using osu.Framework.Platform;
using osu.Game.Beatmaps.IO;
using osu.Game.Database;
using osu.Game.Input.Handlers;
using osu.Game.IO.Legacy;
using osu.Game.Modes;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Screens.Play;
using OpenTK.Graphics;
using OpenTK.Input;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Compressors.LZMA;
using SharpCompress.Readers;
using KeyboardState = osu.Framework.Input.KeyboardState;
using MouseState = osu.Framework.Input.MouseState;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseReplay : TestCasePlayer
    {
        private WorkingBeatmap beatmap;

        private InputHandler replay;

        private Func<Stream> getReplayStream;
        private ScoreDatabase scoreDatabase;

        public override string Description => @"Testing replay playback.";

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            scoreDatabase = new ScoreDatabase(storage);
        }

        protected override Player CreatePlayer(WorkingBeatmap beatmap)
        {
            var player = base.CreatePlayer(beatmap);
            player.ReplayInputHandler = Ruleset.GetRuleset(beatmap.PlayMode).CreateAutoplayReplay(beatmap.Beatmap)?.Replay?.GetInputHandler();
            return player;
        }
    }
}
