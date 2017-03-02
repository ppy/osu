// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Screens.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using OpenTK;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.IO;
using osu.Game.Database;
using osu.Game.Input.Handlers;
using osu.Game.IO.Legacy;
using osu.Game.Modes;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Screens.Play;
using OpenTK.Graphics;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseReplay : TestCasePlayer
    {
        private WorkingBeatmap beatmap;

        private LegacyReplayInputHandler replay;

        public override string Name => @"Replay";

        public override string Description => @"Testing replay playback.";

        public override void Reset()
        {
            base.Reset();

            var list = new List<LegacyReplayInputHandler.LegacyReplayFrame>();

            float lastTime = 0;
            foreach (var l in File.ReadAllText(@"C:\Users\Dean\Desktop\2157025197").Split(','))
            {
                var split = l.Split('|');

                if (split.Length < 4 || float.Parse(split[0]) < 0) continue;

                lastTime += float.Parse(split[0]);

                list.Add(new LegacyReplayInputHandler.LegacyReplayFrame(
                    lastTime,
                    float.Parse(split[1]),
                    384 - float.Parse(split[2]),
                    (LegacyReplayInputHandler.LegacyButtonState)int.Parse(split[3])
                    ));
            }


            replay = new LegacyReplayInputHandler(list);

            //var data = File.ReadAllBytes(@"C:\Users\Dean\.osu\Replays\Tao - O2i3 - Ooi [Game Edit] [Advanced] (2016-08-08) Osu.osr");
            //using (MemoryStream dataStream = new MemoryStream(data))
            //{
            //    var obj = SerializationReader.DynamicDeserializer.Deserialize(dataStream);


            //    Console.WriteLine(obj);
            //}
        }

        protected override Player CreatePlayer(WorkingBeatmap beatmap)
        {
            var player = base.CreatePlayer(beatmap);
            player.ReplayInputHandler = replay;
            return player;
        }
    }
}
