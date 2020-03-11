// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public abstract class OverlayStreamItem<T> : TabItem<T>
    {
        public readonly Bindable<T> SelectedItem = new Bindable<T>();

        private bool userHoveringArea;

        public bool UserHoveringArea
        {
            set
            {
                if (value == userHoveringArea)
                    return;

                userHoveringArea = value;
                updateState();
            }
        }

        private FillFlowContainer<SpriteText> text;
        private ExpandingBar expandingBar;

        protected OverlayStreamItem(T value)
            : base(value)
        {
            Height = 60;
            Width = 100;
            Padding = new MarginPadding(5);
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            AddRange(new Drawable[]
            {
                text = new FillFlowContainer<SpriteText>
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding { Top = 6 },
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Text = MainText,
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Black),
                        },
                        new OsuSpriteText
                        {
                            Text = AdditionalText,
                            Font = OsuFont.GetFont(size: 16, weight: FontWeight.Regular),
                        },
                        new OsuSpriteText
                        {
                            Text = InfoText,
                            Font = OsuFont.GetFont(size: 10),
                            Colour = colourProvider.Foreground1
                        },
                    }
                },
                expandingBar = new ExpandingBar
                {
                    Anchor = Anchor.TopCentre,
                    Colour = GetBarColour(colours),
                    ExpandedSize = 4,
                    CollapsedSize = 2,
                    Expanded = true
                },
                new HoverClickSounds()
            });

            SelectedItem.BindValueChanged(_ => updateState(), true);
        }

        protected abstract string MainText { get; }

        protected abstract string AdditionalText { get; }

        protected virtual string InfoText => string.Empty;

        protected abstract Color4 GetBarColour(OsuColour colours);

        protected override void OnActivated() => updateState();

        protected override void OnDeactivated() => updateState();

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        private void updateState()
        {
            // highlighted regardless if we are hovered
            bool textHighlighted = IsHovered;
            bool barExpanded = IsHovered;

            if (SelectedItem.Value == null)
            {
                // at listing, all badges are highlighted when user is not hovering any badge.
                textHighlighted |= !userHoveringArea;
                barExpanded |= !userHoveringArea;
            }
            else
            {
                // bar is always expanded when active
                barExpanded |= Active.Value;

                // text is highlighted only when hovered or active (but not if in selection mode)
                textHighlighted |= Active.Value && !userHoveringArea;
            }

            expandingBar.Expanded = barExpanded;
            text.FadeTo(textHighlighted ? 1 : 0.5f, 100, Easing.OutQuint);
        }
    }
}
