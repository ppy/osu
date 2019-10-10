// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Bindables;

namespace osu.Game.Overlays.Comments
{
    public class SortSelector : OsuTabControl<SortCommentsBy>
    {
        private const int spacing = 5;

        protected override Dropdown<SortCommentsBy> CreateDropdown() => null;

        protected override TabItem<SortCommentsBy> CreateTabItem(SortCommentsBy value) => new SortTabItem(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(spacing, 0),
        };

        public SortSelector()
        {
            AutoSizeAxes = Axes.Both;
        }

        private class SortTabItem : TabItem<SortCommentsBy>
        {
            private readonly TabContent content;

            public SortTabItem(SortCommentsBy value)
                : base(value)
            {
                AutoSizeAxes = Axes.Both;
                Child = content = new TabContent(value)
                { Active = { BindTarget = Active } };
            }

            protected override void OnActivated() => content.FadeInBackground();

            protected override void OnDeactivated() => content.FadeOutBackground();

            private class TabContent : HeaderButton
            {
                private const int text_size = 14;

                public readonly BindableBool Active = new BindableBool();

                public TabContent(SortCommentsBy value)
                {
                    Add(new SpriteText
                    {
                        Font = OsuFont.GetFont(size: text_size),
                        Text = value.ToString()
                    });
                }

                protected override void OnHoverLost(HoverLostEvent e)
                {
                    if (!Active.Value) base.OnHoverLost(e);
                }
            }
        }
    }

    public enum SortCommentsBy
    {
        New,
        Old,
        Top
    }
}
