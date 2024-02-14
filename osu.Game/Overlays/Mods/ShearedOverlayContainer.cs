// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// A sheared overlay which provides a header and footer and basic animations.
    /// Exposes <see cref="TopLevelContent"/>, <see cref="MainAreaContent"/> and <see cref="Footer"/> as valid targets for content.
    /// </summary>
    public abstract partial class ShearedOverlayContainer : OsuFocusedOverlayContainer
    {
        protected const float PADDING = 14;

        public const float SHEAR = 0.2f;

        [Cached]
        protected readonly OverlayColourProvider ColourProvider;

        /// <summary>
        /// The overlay's header.
        /// </summary>
        protected ShearedOverlayHeader Header { get; private set; }

        /// <summary>
        /// The overlay's footer.
        /// </summary>
        protected Container Footer { get; private set; }

        /// <summary>
        /// A container containing all content, including the header and footer.
        /// May be used for overlay-wide animations.
        /// </summary>
        protected Container TopLevelContent { get; private set; }

        /// <summary>
        /// A container for content that is to be displayed between the header and footer.
        /// </summary>
        protected Container MainAreaContent { get; private set; }

        /// <summary>
        /// A container for content that is to be displayed inside the footer.
        /// </summary>
        protected Container FooterContent { get; private set; }

        protected override bool StartHidden => true;

        protected override bool BlockNonPositionalInput => true;

        protected ShearedOverlayContainer(OverlayColourScheme colourScheme)
        {
            RelativeSizeAxes = Axes.Both;

            ColourProvider = new OverlayColourProvider(colourScheme);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float footer_height = 50;

            Child = TopLevelContent = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    Header = new ShearedOverlayHeader
                    {
                        Anchor = Anchor.TopCentre,
                        Depth = float.MinValue,
                        Origin = Anchor.TopCentre,
                        Close = Hide
                    },
                    MainAreaContent = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Top = ShearedOverlayHeader.HEIGHT,
                            Bottom = footer_height + PADDING,
                        }
                    },
                    Footer = new InputBlockingContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Depth = float.MinValue,
                        Height = footer_height,
                        Margin = new MarginPadding { Top = PADDING },
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourProvider.Background5
                            },
                            FooterContent = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    }
                }
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (State.Value == Visibility.Visible)
            {
                Hide();
                return true;
            }

            return base.OnClick(e);
        }

        protected override void PopIn()
        {
            const double fade_in_duration = 400;

            this.FadeIn(fade_in_duration, Easing.OutQuint);

            Header.MoveToY(0, fade_in_duration, Easing.OutQuint);
            Footer.MoveToY(0, fade_in_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            const double fade_out_duration = 500;

            base.PopOut();
            this.FadeOut(fade_out_duration, Easing.OutQuint);

            Header.MoveToY(-Header.DrawHeight, fade_out_duration, Easing.OutQuint);
            Footer.MoveToY(Footer.DrawHeight, fade_out_duration, Easing.OutQuint);
        }
    }
}
