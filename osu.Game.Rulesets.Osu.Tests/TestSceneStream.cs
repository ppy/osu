// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public partial class TestSceneStream : OsuSkinnableTestScene
    {
        [Test]
        public void TestHits()
        {
            AddStep("Hit Big Stream", () => SetContents(_ => testStream(2, true)));
            AddStep("Hit Medium Stream", () => SetContents(_ => testStream(5, true)));
            AddStep("Hit Small Stream", () => SetContents(_ => testStream(7, true)));
        }

        private Drawable testStream(float circleSize, bool auto = false, double hitOffset = 0)
        {
            var playfield = new TestOsuPlayfield();

            Vector2 pos = new Vector2(0, 0);

            playfield.Add(createSingle(circleSize, auto, 0, pos, hitOffset));

            return playfield;
        }

        private DrawableStream createSingle(float circleSize, bool auto, double timeOffset, Vector2? positionOffset, double hitOffset = 0)
        {
            positionOffset ??= Vector2.Zero;

            var stream = new Stream
            {
                StartTime = Time.Current + 1000 + timeOffset,
                Position = new Vector2(64, 96) + positionOffset.Value,
                StreamPath = new StreamPath(new[]
                {
                    new PathControlPoint(Vector2.Zero, PathType.Linear),
                    new PathControlPoint(new Vector2(64, 200), PathType.PerfectCurve),
                    new PathControlPoint(new Vector2(128, 100)),
                    new PathControlPoint(new Vector2(128, 0))
                }, new[]
                {
                    new StreamControlPoint(),
                    new StreamControlPoint(100, 4, 1.5),
                    new StreamControlPoint(200, 8, 1)
                })
            };

            stream.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = circleSize });

            var drawable = CreateDrawableStream(stream);

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObject>())
                mod.ApplyToDrawableHitObject(drawable);
            return drawable;
        }

        protected virtual DrawableStream CreateDrawableStream(Stream stream) => new DrawableStream(stream);

        protected partial class TestOsuPlayfield : OsuPlayfield
        {
            public TestOsuPlayfield()
            {
                RelativeSizeAxes = Axes.Both;
            }
        }
    }
}
