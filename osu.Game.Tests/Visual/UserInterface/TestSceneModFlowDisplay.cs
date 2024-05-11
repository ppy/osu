// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneModFlowDisplay : OsuTestScene
    {
        private ModFlowDisplay modFlow;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = modFlow = new ModFlowDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.None,
                Width = 200,
                Current =
                {
                    Value = new OsuRuleset().CreateAllMods().ToArray(),
                }
            };
        });

        [Test]
        public void TestWrapping()
        {
            AddSliderStep("icon size", 0.1f, 2, 1, val =>
            {
                if (modFlow != null)
                    modFlow.IconScale = val;
            });

            AddSliderStep("flow width", 100, 500, 200, val =>
            {
                if (modFlow != null)
                    modFlow.Width = val;
            });
        }
    }
}
