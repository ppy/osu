﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Screens.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using OpenTK;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.IO;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Screens.Play;
using OpenTK.Graphics;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCasePlayer : TestCase
    {
        protected Player Player;

        public override string Description => @"Showing everything to play the game.";

        private BeatmapDatabase db;
        private PlayMode mode;

        [BackgroundDependencyLoader]
        private void load(BeatmapDatabase db)
        {
            this.db = db;
        }

        public override void Reset()
        {
            base.Reset();

            WorkingBeatmap beatmap = null;

            var beatmapInfo = db.Query<BeatmapInfo>().FirstOrDefault(b => b.Mode == mode);
            if (beatmapInfo != null)
                beatmap = db.GetWorkingBeatmap(beatmapInfo);

            if (beatmap?.Track == null)
            {
                var objects = new List<HitObject>();

                int time = 1500;
                for (int i = 0; i < 50; i++)
                {
                    objects.Add(new HitCircle
                    {
                        StartTime = time,
                        Position = new Vector2(i % 4 == 0 || i % 4 == 2 ? 0 : 512,
                        i % 4 < 2 ? 0 : 384),
                        NewCombo = i % 4 == 0
                    });

                    time += 500;
                }

                var decoder = new ConstructableBeatmapDecoder();

                Beatmap b = new Beatmap
                {
                    HitObjects = objects,
                    BeatmapInfo = new BeatmapInfo
                    {
                        Mode = mode,
                        BaseDifficulty = new BaseDifficulty
                        {
                            ApproachRate = 5,
                            CircleSize = 5,
                            DrainRate = 5,
                            OverallDifficulty = 5,
                            SliderMultiplier = 1.4,
                            SliderTickRate = 1
                        },
                        Metadata = new BeatmapMetadata
                        {
                            Artist = @"Unknown",
                            Title = @"Sample Beatmap",
                            Author = @"peppy",
                        }
                    }
                };

                decoder.Process(b);

                beatmap = new TestWorkingBeatmap(b);
            }

            Add(new Box
            {
                RelativeSizeAxes = Framework.Graphics.Axes.Both,
                Colour = Color4.Black,
            });

            Add(new PlayerLoader(Player = CreatePlayer(beatmap))
            {
                Beatmap = beatmap
            });

            AddButton("osu!", () =>
            {
                mode = PlayMode.Osu;
                Reset();
            });

            AddButton("osu!taiko", () =>
            {
                mode = PlayMode.Taiko;
                Reset();
            });
        }

        protected virtual Player CreatePlayer(WorkingBeatmap beatmap)
        {
            return new Player
            {
                Beatmap = beatmap
            };
        }

        private class TestWorkingBeatmap : WorkingBeatmap
        {
            public TestWorkingBeatmap(Beatmap beatmap)
                : base(beatmap.BeatmapInfo, beatmap.BeatmapInfo.BeatmapSet)
            {
                Beatmap = beatmap;
            }

            protected override ArchiveReader GetReader() => null;
        }
    }
}
