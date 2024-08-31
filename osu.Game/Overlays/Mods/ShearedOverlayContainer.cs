// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Footer;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// A sheared overlay which provides a header and basic animations.
    /// Exposes <see cref="TopLevelContent"/> and <see cref="MainAreaContent"/> as valid targets for content.
    /// </summary>
    public abstract partial class ShearedOverlayContainer : OsuFocusedOverlayContainer
    {
        public const float PADDING = 14;

        [Cached]
        public readonly OverlayColourProvider ColourProvider;

        /// <summary>
        /// The overlay's header.
        /// </summary>
        protected ShearedOverlayHeader Header { get; private set; } = null!;

        /// <summary>
        /// The overlay's footer.
        /// </summary>
        protected Container Footer { get; private set; } = null!;

        [Resolved]
        private ScreenFooter? footer { get; set; }

        /// <summary>
        /// A container containing all content, including the header and footer.
        /// May be used for overlay-wide animations.
        /// </summary>
        protected Container TopLevelContent { get; private set; } = null!;

        /// <summary>
        /// A container for content that is to be displayed between the header and footer.
        /// </summary>
        protected Container MainAreaContent { get; private set; } = null!;

        /// <summary>
        /// A container for content that is to be displayed inside the footer.
        /// </summary>
        protected Container FooterContent { get; private set; } = null!;

        protected override bool StartHidden => true;

        protected override bool BlockNonPositionalInput => true;

        // ShearedOverlayContainers are placed at a layer within the screen container as they rely on ScreenFooter which must be placed there.
        // Therefore, dimming must be managed locally, since DimMainContent dims the entire screen layer.
        protected sealed override bool DimMainContent => false;

        protected ShearedOverlayContainer(OverlayColourScheme colourScheme)
        {
            RelativeSizeAxes = Axes.Both;

            ColourProvider = new OverlayColourProvider(colourScheme);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = TopLevelContent = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourProvider.Background6.Opacity(0.75f),
                    },
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
                            Bottom = ScreenFooter.HEIGHT + PADDING,
                        }
                    },
                }
            };
        }

        public VisibilityContainer? DisplayedFooterContent { get; private set; }

        /// <summary>
        /// Creates content to be displayed on the game-wide footer.
        /// </summary>
        public virtual VisibilityContainer? CreateFooterContent() => null;

        /// <summary>
        /// Invoked when the back button in the footer is pressed.
        /// </summary>
        /// <returns>Whether the back button should not close the overlay.</returns>
        public virtual bool OnBackButton() => false;

        protected override bool OnClick(ClickEvent e)
        {
            if (State.Value == Visibility.Visible)
            {
                Hide();
                return true;
            }

            return base.OnClick(e);
        }

        private IDisposable? activeOverlayRegistration;
        private bool hideFooterOnPopOut;

        protected override void PopIn()
        {
            const double fade_in_duration = 400;

            this.FadeIn(fade_in_duration, Easing.OutQuint);

            Header.MoveToY(0, fade_in_duration, Easing.OutQuint);

            if (footer != null)
            {
                activeOverlayRegistration = footer.RegisterActiveOverlayContainer(this, out var footerContent);
                DisplayedFooterContent = footerContent;

                if (footer.State.Value == Visibility.Hidden)
                {
                    footer.Show();
                    hideFooterOnPopOut = true;
                }
            }
        }

        protected override void PopOut()
        {
            const double fade_out_duration = 500;

            base.PopOut();
            this.FadeOut(fade_out_duration, Easing.OutQuint);

            Header.MoveToY(-Header.DrawHeight, fade_out_duration, Easing.OutQuint);

            if (footer != null)
            {
                activeOverlayRegistration?.Dispose();
                activeOverlayRegistration = null;
                DisplayedFooterContent = null;

                if (hideFooterOnPopOut)
                {
                    footer.Hide();
                    hideFooterOnPopOut = false;
                }
            }
        }
    }
}
