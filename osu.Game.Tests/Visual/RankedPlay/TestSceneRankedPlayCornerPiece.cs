// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayCornerPiece : MultiplayerTestScene
    {
        private readonly Bindable<Visibility> visibility = new Bindable<Visibility>(Visibility.Visible);

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add children", () =>
            {
                Children =
                [
                    new RankedPlayCornerPiece(RankedPlayColourScheme.Blue, Anchor.BottomLeft)
                    {
                        State = { BindTarget = visibility },
                        Child = new RankedPlayUserDisplay(2, Anchor.BottomLeft, RankedPlayColourScheme.Blue)
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    },
                    new RankedPlayCornerPiece(RankedPlayColourScheme.Red, Anchor.TopRight)
                    {
                        State = { BindTarget = visibility },
                        Child = new RankedPlayUserDisplay(2, Anchor.TopRight, RankedPlayColourScheme.Red)
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    },
                ];
            });
        }

        [Test]
        public void TestCornerPieces()
        {
            AddStep("show", () => visibility.Value = Visibility.Visible);
            AddStep("hide", () => visibility.Value = Visibility.Hidden);
            AddSliderStep("health", 0, 1_000_000, 1_000_000, value =>
            {
                foreach (var d in this.ChildrenOfType<RankedPlayUserDisplay>())
                {
                    d.Health.Value = value;
                }
            });
        }
    }
}
