// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public class TestScenePathControlPointVisualiser : OsuTestScene
    {
        private Slider slider;
        private PathControlPointVisualiser visualiser;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            slider = new Slider();
            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
        });

        [Test]
        public void TestAddOverlappingControlPoints()
        {
            createVisualiser(true);

            addControlPointStep(new Vector2(200));
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(300));
            addControlPointStep(new Vector2(500, 300));

            AddAssert("last connection displayed", () =>
            {
                var lastConnection = visualiser.Connections.Last(c => c.ControlPoint.Position.Value == new Vector2(300));
                return lastConnection.DrawWidth > 50;
            });
        }

        private void createVisualiser(bool allowSelection) => AddStep("create visualiser", () => Child = visualiser = new PathControlPointVisualiser(slider, allowSelection)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        });

        private void addControlPointStep(Vector2 position) => AddStep($"add control point {position}", () => slider.Path.ControlPoints.Add(new PathControlPoint(position)));
    }
}
