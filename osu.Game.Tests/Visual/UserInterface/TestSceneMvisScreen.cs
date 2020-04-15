// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Mvis.UI.Objects;
using osu.Game.Screens.Mvis.Buttons;
using osu.Game.Overlays;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneMvisScreen : ScreenTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MvisScreen),
            typeof(BottomBar)
        };
        [Cached]
        private MusicController musicController = new MusicController();

        [Test]
        public void Mvis()
        {
            OsuScreenStack stack;

            AddStep("Run test", () =>
            {
                Child = stack = new OsuScreenStack
                {
                    RelativeSizeAxes = Axes.Both
                };

                stack.Push( new MvisScreen() );
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            Add(musicController);
        }
    }
}
