// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Allocation;
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
        public const int MARGIN_HORIZONTAL = 17;
        public const int MARGIN_VERTICAL = 4;
        private const int text_size = 17;
        private const int transition_length = 80;

        private TextContainer text;
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

            AddInternal(hoverClickSounds = new HoverClickSounds());

            updateTextColour();

            bool hasSubmenu = Item.Items.Any();

            // Only add right chevron if direction of menu items is vertical (i.e. width is relative size, see `DrawableMenuItem.SetFlowDirection()`).
            if (hasSubmenu && RelativeSizeAxes == Axes.X)
            {
                AddInternal(new SpriteIcon
                {
                    Margin = new MarginPadding(6),
                    Size = new Vector2(8),
                    Icon = FontAwesome.Solid.ChevronRight,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                });

                text.Padding = new MarginPadding
                {
                    // Add some padding for the chevron above.
                    Right = 5,
                };
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Item.Action.BindDisabledChanged(_ => updateState(), true);
            FinishTransforms();
        }

        private void updateTextColour()
        {
            switch ((Item as OsuMenuItem)?.Type)
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
                text.BoldText.FadeIn(transition_length, Easing.OutQuint);
                text.NormalText.FadeOut(transition_length, Easing.OutQuint);
            }
            else
            {
                text.BoldText.FadeOut(transition_length, Easing.OutQuint);
                text.NormalText.FadeIn(transition_length, Easing.OutQuint);
            }
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

            public TextContainer()
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;

                AutoSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    NormalText = new OsuSpriteText
                    {
                        AlwaysPresent = true, // ensures that the menu item does not change width when switching between normal and bold text.
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(size: text_size),
                        Margin = new MarginPadding { Horizontal = MARGIN_HORIZONTAL, Vertical = MARGIN_VERTICAL },
                    },
                    BoldText = new OsuSpriteText
                    {
                        AlwaysPresent = true, // ensures that the menu item does not change width when switching between normal and bold text.
                        Alpha = 0,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold),
                        Margin = new MarginPadding { Horizontal = MARGIN_HORIZONTAL, Vertical = MARGIN_VERTICAL },
                    }
                };
            }
        }
    }
}
