// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select.Filter;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneTabControl : OsuManualInputManagerTestScene
    {
        private OsuSpriteText text = null!;
        private OsuTabControl<GroupMode> filter = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                filter = new OsuTabControl<GroupMode>
                {
                    Margin = new MarginPadding(4),
                    Size = new Vector2(229, 24),
                    AutoSort = true
                },
                text = new OsuSpriteText
                {
                    Text = "None",
                    Margin = new MarginPadding(4),
                    Position = new Vector2(275, 5)
                }
            };

            filter.PinItem(GroupMode.All);
            filter.PinItem(GroupMode.RecentlyPlayed);

            filter.Current.ValueChanged += grouping =>
            {
                text.Text = "Currently Selected: " + grouping.NewValue.ToString();
            };
        });

        [Test]
        public void TestSelection()
        {
            AddStep("select by user click", () =>
            {
                filter.Current.SetDefault();
                InputManager.MoveMouseTo(filter.ChildrenOfType<OsuTabControl<GroupMode>.OsuTabItem>().Single(t => t.Value == GroupMode.RecentlyPlayed));
                InputManager.Click(MouseButton.Left);
            });

            AddStep("select programmatically with feedback", () =>
            {
                filter.Current.SetDefault();
                filter.SelectTabWithSound(GroupMode.RecentlyPlayed);
            });

            AddStep("select programmatically by bindable", () =>
            {
                filter.Current.SetDefault();
                filter.Current.Value = GroupMode.RecentlyPlayed;
            });
        }
    }
}
