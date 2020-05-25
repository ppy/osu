// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneSpinnerSpunOut : OsuTestScene
    {
        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            SelectedMods.Value = new[] { new OsuModSpunOut() };
        });

        [Test]
        public void TestSpunOut()
        {
            DrawableSpinner spinner = null;

            AddStep("create spinner", () => spinner = createSpinner());

            AddUntilStep("wait for end", () => Time.Current > spinner.LifetimeEnd);

            AddAssert("spinner is completed", () => spinner.Progress >= 1);
        }

        private DrawableSpinner createSpinner()
        {
            var spinner = new Spinner
            {
                StartTime = Time.Current + 500,
                EndTime = Time.Current + 2500
            };
            spinner.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            var drawableSpinner = new DrawableSpinner(spinner)
            {
                Anchor = Anchor.Centre
            };

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(new[] { drawableSpinner });

            Add(drawableSpinner);
            return drawableSpinner;
        }
    }
}
