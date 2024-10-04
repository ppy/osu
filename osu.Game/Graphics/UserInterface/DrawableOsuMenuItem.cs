// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public partial class DrawableOsuMenuItem : Menu.DrawableMenuItem
    {
        public const int MARGIN_HORIZONTAL = 10;
        public const int MARGIN_VERTICAL = 4;
        public const int TEXT_SIZE = 17;
        public const int TRANSITION_LENGTH = 80;

        public BindableBool ShowCheckbox { get; } = new BindableBool();

        private TextContainer text;
        private HotkeyDisplay hotkey;
        private HoverClickSounds hoverClickSounds;

        public DrawableOsuMenuItem(MenuItem item)
            : base(item)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = Color4.Transparent;
            BackgroundColourHover = Color4Extensions.FromHex(@"172023");

            AddInternal(hotkey = new HotkeyDisplay
            {
                Alpha = 0,
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Margin = new MarginPadding { Right = 10, Top = 1 },
            });
            AddInternal(hoverClickSounds = new HoverClickSounds());

            updateText();

            if (showChevron)
            {
                AddInternal(new SpriteIcon
                {
                    Margin = new MarginPadding { Horizontal = 10, },
                    Size = new Vector2(8),
                    Icon = FontAwesome.Solid.ChevronRight,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                });
            }
        }

        // Only add right chevron if direction of menu items is vertical (i.e. width is relative size, see `DrawableMenuItem.SetFlowDirection()`).
        private bool showChevron => Item.Items.Any() && RelativeSizeAxes == Axes.X;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ShowCheckbox.BindValueChanged(_ => updateState());
            Item.Action.BindDisabledChanged(_ => updateState(), true);
            FinishTransforms();
        }

        private void updateText()
        {
            var osuMenuItem = Item as OsuMenuItem;

            switch (osuMenuItem?.Type)
            {
                default:
                case MenuItemType.Standard:
                    text.Colour = Color4.White;
                    break;

                case MenuItemType.Destructive:
                    text.Colour = Color4.Red;
                    break;

                case MenuItemType.Highlighted:
                    text.Colour = Color4Extensions.FromHex(@"ffcc22");
                    break;
            }

            hotkey.Hotkey = osuMenuItem?.Hotkey ?? default;
            hotkey.Alpha = EqualityComparer<Hotkey>.Default.Equals(hotkey.Hotkey, default) ? 0 : 1;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // this hack ensures that the menu can auto-size while leaving enough space for the hotkey display.
            // the gist of it is that while the hotkey display is not in the text / "content" that determines sizing
            // (because it cannot be, because we want the hotkey display to align to the *right* and not the left),
            // enough padding to fit the hotkey with _its_ spacing is added as padding of the text to compensate.
            text.Padding = new MarginPadding { Right = hotkey.Alpha > 0 || showChevron ? hotkey.DrawWidth + 15 : 0 };
        }

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
            hoverClickSounds.Enabled.Value = IsActionable;
            Alpha = IsActionable ? 1 : 0.2f;

            if (IsHovered && IsActionable)
            {
                text.BoldText.FadeIn(TRANSITION_LENGTH, Easing.OutQuint);
                text.NormalText.FadeOut(TRANSITION_LENGTH, Easing.OutQuint);
            }
            else
            {
                text.BoldText.FadeOut(TRANSITION_LENGTH, Easing.OutQuint);
                text.NormalText.FadeIn(TRANSITION_LENGTH, Easing.OutQuint);
            }

            text.CheckboxContainer.Alpha = ShowCheckbox.Value ? 1 : 0;
        }

        protected sealed override Drawable CreateContent() => text = CreateTextContainer();
        protected virtual TextContainer CreateTextContainer() => new TextContainer();

        protected partial class TextContainer : Container, IHasText
        {
            public LocalisableString Text
            {
                get => NormalText.Text;
                set
                {
                    NormalText.Text = value;
                    BoldText.Text = value;
                }
            }

            public readonly SpriteText NormalText;
            public readonly SpriteText BoldText;
            public readonly Container CheckboxContainer;

            public TextContainer()
            {
                AutoSizeAxes = Axes.Both;

                Child = new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,

                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(10),
                    Direction = FillDirection.Horizontal,
                    Padding = new MarginPadding { Horizontal = MARGIN_HORIZONTAL, Vertical = MARGIN_VERTICAL, },

                    Children = new Drawable[]
                    {
                        CheckboxContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = MARGIN_HORIZONTAL,
                        },
                        new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                NormalText = new OsuSpriteText
                                {
                                    AlwaysPresent = true, // ensures that the menu item does not change width when switching between normal and bold text.
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = OsuFont.GetFont(size: TEXT_SIZE),
                                },
                                BoldText = new OsuSpriteText
                                {
                                    AlwaysPresent = true, // ensures that the menu item does not change width when switching between normal and bold text.
                                    Alpha = 0,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold),
                                }
                            }
                        },
                    }
                };
            }
        }
    }
}
