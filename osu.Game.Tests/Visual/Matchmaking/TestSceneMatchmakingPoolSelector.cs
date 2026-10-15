// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Online.Matchmaking;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneMatchmakingPoolSelector : OsuManualInputManagerTestScene
    {
        private PoolSelector selector = null!;

        private readonly Bindable<bool> enabled = new Bindable<bool>(true);

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("add selector", () => Child = new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 100,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.5f),
                    },
                    selector = new PoolSelector
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Enabled = { BindTarget = enabled },
                        AvailablePools = { Value = pools },
                        SelectedPool = { Value = pools.First() },
                    }
                },
            });
        }

        [Test]
        public void TestEnabledState()
        {
            AddStep("enable", () => enabled.Value = true);

            AddStep("attempt change to second pool", () =>
            {
                var drawable = this.ChildrenOfType<IHasText>().Single(d => d.Text == "osu!taiko");
                InputManager.MoveMouseTo(drawable.ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("selected pool changed", () => selector.SelectedPool.Value, () => Is.EqualTo(pools[1]));
            AddStep("disable", () => enabled.Value = false);

            AddStep("attempt change to third pool", () =>
            {
                var drawable = this.ChildrenOfType<IHasText>().Single(d => d.Text == "osu!catch");
                InputManager.MoveMouseTo(drawable.ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("selected pool did not change", () => selector.SelectedPool.Value, () => Is.EqualTo(pools[1]));
        }

        private static readonly MatchmakingPool[] pools =
        [
            new MatchmakingPool { Id = 0, RulesetId = 0 },
            new MatchmakingPool { Id = 1, RulesetId = 1 },
            new MatchmakingPool { Id = 2, RulesetId = 2 },
            new MatchmakingPool { Id = 3, RulesetId = 3, Variant = 4 },
            new MatchmakingPool { Id = 4, RulesetId = 3, Variant = 7 },
        ];
    }
}
