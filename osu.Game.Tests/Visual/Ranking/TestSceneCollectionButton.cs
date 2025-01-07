// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Ranking;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneCollectionButton : OsuManualInputManagerTestScene
    {
        private CollectionButton? collectionButton;
        private readonly BeatmapInfo beatmapInfo = new BeatmapInfo { OnlineID = 88 };

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create button", () => Child = new PopoverContainer
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = collectionButton = new CollectionButton(beatmapInfo)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            });
        }

        [Test]
        public void TestCollectionButton()
        {
            AddStep("click collection button", () =>
            {
                InputManager.MoveMouseTo(collectionButton!);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("collection popover is visible", () => this.ChildrenOfType<CollectionPopover>().Single().State.Value == Visibility.Visible);

            AddStep("click outside popover", () =>
            {
                InputManager.MoveMouseTo(ScreenSpaceDrawQuad.TopLeft);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("collection popover is hidden", () => this.ChildrenOfType<CollectionPopover>().Single().State.Value == Visibility.Hidden);

            AddStep("click collection button", () =>
            {
                InputManager.MoveMouseTo(collectionButton!);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("press escape", () => InputManager.Key(Key.Escape));

            AddAssert("collection popover is hidden", () => this.ChildrenOfType<CollectionPopover>().Single().State.Value == Visibility.Hidden);
        }
    }
}
