// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// A sheared overlay which provides a header and footer and basic animations.
    /// Exposes <see cref="TopLevelContent"/>, <see cref="MainAreaContent"/> and <see cref="Footer"/> as valid targets for content.
    /// </summary>
    public abstract class ShearedOverlayContainer : OsuFocusedOverlayContainer
    {
        [Cached]
        protected readonly OverlayColourProvider ColourProvider;

        /// <summary>
        /// The overlay's header.
        /// </summary>
        protected PopupScreenTitle Header { get; private set; }

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

        protected abstract OverlayColourScheme ColourScheme { get; }

        protected override bool StartHidden => true;

        protected override bool BlockNonPositionalInput => true;

        protected ShearedOverlayContainer()
        {
            RelativeSizeAxes = Axes.Both;

            ColourProvider = new OverlayColourProvider(ColourScheme);
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
                    Header = new PopupScreenTitle
                    {
                        Anchor = Anchor.TopCentre,
                        Depth = float.MinValue,
                        Origin = Anchor.TopCentre,
                        Title = "Mod Select",
                        Description = "Mods provide different ways to enjoy gameplay. Some have an effect on the score you can achieve during ranked play. Others are just for fun.",
                        Close = Hide
                    },
                    MainAreaContent = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Top = PopupScreenTitle.HEIGHT,
                            Bottom = footer_height,
                        }
                    },
                    Footer = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Depth = float.MinValue,
                        Height = footer_height,
                        Margin = new MarginPadding { Top = 10 },
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

        protected override void PopIn()
        {
            const double fade_in_duration = 400;

            base.PopIn();
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
