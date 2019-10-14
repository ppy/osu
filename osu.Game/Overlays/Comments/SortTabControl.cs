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
using osu.Framework.Allocation;
using osuTK.Graphics;

namespace osu.Game.Overlays.Comments
{
    public class SortTabControl : OsuTabControl<CommentsSortCriteria>
    {
        private const int spacing = 5;

        protected override Dropdown<CommentsSortCriteria> CreateDropdown() => null;

        protected override TabItem<CommentsSortCriteria> CreateTabItem(CommentsSortCriteria value) => new SortTabItem(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(spacing, 0),
        };

        public SortTabControl()
        {
            AutoSizeAxes = Axes.Both;
        }

        private class SortTabItem : TabItem<CommentsSortCriteria>
        {
            private readonly TabContent content;

            public SortTabItem(CommentsSortCriteria value)
                : base(value)
            {
                AutoSizeAxes = Axes.Both;
                Child = content = new TabContent(value)
                {
                    Active = { BindTarget = Active }
                };
            }

            protected override void OnActivated() => content.Activate();

            protected override void OnDeactivated() => content.Deactivate();

            private class TabContent : HeaderButton
            {
                private const int text_size = 14;

                public readonly BindableBool Active = new BindableBool();

                [Resolved]
                private OsuColour colours { get; set; }

                private readonly SpriteText text;

                public TabContent(CommentsSortCriteria value)
                {
                    Add(text = new SpriteText
                    {
                        Font = OsuFont.GetFont(size: text_size),
                        Text = value.ToString()
                    });
                }

                public void Activate()
                {
                    ShowBackground();
                    text.Font = text.Font.With(weight: FontWeight.Bold);
                    text.Colour = colours.BlueLighter;
                }

                public void Deactivate()
                {
                    if (!IsHovered)
                        HideBackground();

                    text.Font = text.Font.With(weight: FontWeight.Medium);
                    text.Colour = Color4.White;
                }

                protected override void OnHoverLost(HoverLostEvent e)
                {
                    if (!Active.Value) base.OnHoverLost(e);
                }
            }
        }
    }

    public enum CommentsSortCriteria
    {
        New,
        Old,
        Top
    }
}
