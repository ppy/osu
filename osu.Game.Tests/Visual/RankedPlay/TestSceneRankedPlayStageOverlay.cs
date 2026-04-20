// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayStageOverlay : RankedPlayTestScene
    {
        private Container content = null!;
        protected override Container<Drawable> Content => content;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create components", () => base.Content.Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new RankedPlayBackground
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                }
            });
        }

        [Test]
        public void TestBasic()
        {
            double multiplier = 1.0;

            AddSliderStep<double>("set multiplier", 1, 5, 2, value => multiplier = value);
            AddStep("create", () => Child = new RankedPlayStageOverlay("Pick Phase", RankedPlayColourScheme.BLUE)
            {
                PickingUser = new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                },
                Multiplier = multiplier,
            });
        }

        [Test]
        public void TestLongUsername()
        {
            AddStep("create", () => Child = new RankedPlayStageOverlay("Pick Phase", RankedPlayColourScheme.BLUE)
            {
                PickingUser = new APIUser
                {
                    Id = 226597,
                    Username = "WWWWWWWWWWWWWWWWWWWW",
                },
                Multiplier = 2,
            });
        }

        [Test]
        public void TestColourScheme()
        {
            AddStep("create blue", () => Child = new RankedPlayStageOverlay("Pick Phase", RankedPlayColourScheme.BLUE)
            {
                PickingUser = new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                },
                Multiplier = 2,
            });
            AddStep("create red", () => Child = new RankedPlayStageOverlay("Pick Phase", RankedPlayColourScheme.RED)
            {
                PickingUser = new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                },
                Multiplier = 2,
            });
        }
    }
}
