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
using osu.Game.Overlays.Comments;
using JetBrains.Annotations;

namespace osu.Game.Overlays
{
    public class OverlaySortTabControl<T> : CompositeDrawable, IHasCurrentValue<T>
    {
        private readonly BindableWithCurrent<T> current = new BindableWithCurrent<T>();

        public Bindable<T> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public OverlaySortTabControl()
        {
            AutoSizeAxes = Axes.Both;
            AddInternal(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(size: 12),
                        Text = @"Sort by"
                    },
                    CreateControl().With(c =>
                    {
                        c.Anchor = Anchor.CentreLeft;
                        c.Origin = Anchor.CentreLeft;
                        c.Current = current;
                    })
                }
            });
        }

        [NotNull]
        protected virtual SortTabControl CreateControl() => new SortTabControl();

        protected class SortTabControl : OsuTabControl<T>
        {
            protected override Dropdown<T> CreateDropdown() => null;

            protected override TabItem<T> CreateTabItem(T value) => new SortTabItem(value);

            protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5, 0),
            };

            public SortTabControl()
            {
                AutoSizeAxes = Axes.Both;
            }

            protected class SortTabItem : TabItem<T>
            {
                public SortTabItem(T value)
                    : base(value)
                {
                    AutoSizeAxes = Axes.Both;
                    Child = new TabButton(value) { Active = { BindTarget = Active } };
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
                    private OverlayColourProvider colourProvider { get; set; }

                    private readonly SpriteText text;

                    public TabButton(T value)
                    {
                        Add(text = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12),
                            Text = value.ToString()
                        });
                    }

                    protected override void LoadComplete()
                    {
                        base.LoadComplete();
                        Active.BindValueChanged(_ => updateState(), true);
                    }

                    protected override bool OnHover(HoverEvent e)
                    {
                        updateHoverState();
                        return true;
                    }

                    protected override void OnHoverLost(HoverLostEvent e) => updateHoverState();

                    private void updateState()
                    {
                        updateHoverState();
                        text.Font = text.Font.With(weight: Active.Value ? FontWeight.Bold : FontWeight.Medium);
                    }

                    private void updateHoverState()
                    {
                        if (Active.Value || IsHovered)
                            ShowBackground();
                        else
                            HideBackground();

                        text.Colour = Active.Value && !IsHovered ? colourProvider.Light1 : Color4.White;
                    }
                }
            }
        }
    }
}
