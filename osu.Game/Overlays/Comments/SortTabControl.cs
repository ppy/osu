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
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Overlays.Comments
{
    public class SortTabControl : OsuTabControl<CommentsSortCriteria>
    {
        protected override Dropdown<CommentsSortCriteria> CreateDropdown() => null;

        protected override TabItem<CommentsSortCriteria> CreateTabItem(CommentsSortCriteria value) => new SortTabItem(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(5, 0),
        };

        public SortTabControl(OverlayColourScheme colourScheme)
        {
            AutoSizeAxes = Axes.Both;

            TabContainer.ForEach(tab => ((SortTabItem)tab).ColourScheme = colourScheme);
        }

        private class SortTabItem : TabItem<CommentsSortCriteria>
        {
            public OverlayColourScheme ColourScheme { get; set; }

            private readonly CommentsSortCriteria value;

            public SortTabItem(CommentsSortCriteria value)
                : base(value)
            {
                this.value = value;

                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Child = new TabButton(value, ColourScheme) { Active = { BindTarget = Active } };
            }

            protected override void OnActivated()
            {
            }

            protected override void OnDeactivated()
            {
            }

            private class TabButton : HeaderButton
            {
                public readonly BindableBool Active = new BindableBool();

                [Resolved]
                private OsuColour colours { get; set; }

                private readonly SpriteText text;

                public TabButton(CommentsSortCriteria value, OverlayColourScheme colourScheme)
                    : base(colourScheme)
                {
                    Add(text = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 14),
                        Text = value.ToString()
                    });
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    Active.BindValueChanged(active =>
                    {
                        updateBackgroundState();

                        text.Font = text.Font.With(weight: active.NewValue ? FontWeight.Bold : FontWeight.Medium);
                        text.Colour = active.NewValue ? colours.ForOverlayElement(ColourScheme, 0.4f, 0.8f) : Color4.White;
                    }, true);
                }

                protected override bool OnHover(HoverEvent e)
                {
                    updateBackgroundState();
                    return true;
                }

                protected override void OnHoverLost(HoverLostEvent e) => updateBackgroundState();

                private void updateBackgroundState()
                {
                    if (Active.Value || IsHovered)
                        ShowBackground();
                    else
                        HideBackground();
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
