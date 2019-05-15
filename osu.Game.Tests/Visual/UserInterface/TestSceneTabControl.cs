// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select.Filter;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    [Description("SongSelect filter control")]
    public class TestSceneTabControl : OsuTestScene
    {
        public TestSceneTabControl()
        {
            OsuSpriteText text;
            OsuTabControl<GroupMode> filter;
            Add(filter = new OsuTabControl<GroupMode>
            {
                Margin = new MarginPadding(4),
                Size = new Vector2(229, 24),
                AutoSort = true
            });
            Add(text = new OsuSpriteText
            {
                Text = "None",
                Margin = new MarginPadding(4),
                Position = new Vector2(275, 5)
            });

            filter.PinItem(GroupMode.All);
            filter.PinItem(GroupMode.RecentlyPlayed);

            filter.Current.ValueChanged += grouping =>
            {
                text.Text = "Currently Selected: " + grouping.NewValue.ToString();
            };
        }
    }
}
