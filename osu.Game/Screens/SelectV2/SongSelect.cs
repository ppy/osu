// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// This screen is intended to house all components introduced in the new song select design to add transitions and examine the overall look.
    /// This will be gradually built upon and ultimately replace <see cref="Select.SongSelect"/> once everything is in place.
    /// </summary>
    public abstract partial class SongSelect : OsuScreen
    {
        private const float logo_scale = 0.4f;
        private const double fade_duration = 300;

        public const float WEDGE_CONTENT_MARGIN = CORNER_RADIUS_HIDE_OFFSET + OsuGame.SCREEN_EDGE_MARGIN;
        public const float CORNER_RADIUS_HIDE_OFFSET = 20f;
        public const float ENTER_DURATION = 600;

        private readonly ModSelectOverlay modSelectOverlay = new UserModSelectOverlay(OverlayColourScheme.Aquamarine)
        {
            ShowPresets = true,
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private BeatmapCarousel carousel = null!;

        private FilterControl filterControl = null!;
        private BeatmapTitleWedge titleWedge = null!;
        private BeatmapDetailsArea detailsArea = null!;
        private FillFlowContainer wedgesContainer = null!;

        public override bool ShowFooter => true;

        [Resolved]
        private OsuLogo? logo { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.5f), Color4.Black.Opacity(0f)),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Bottom = ScreenFooter.HEIGHT },
                    Child = new PopoverContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new GridContainer // used for max width implementation
                            {
                                RelativeSizeAxes = Axes.Both,
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Relative, 0.5f, maxSize: 850),
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Relative, 0.5f, maxSize: 750),
                                },
                                Content = new[]
                                {
                                    new[]
                                    {
                                        wedgesContainer = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Margin = new MarginPadding
                                            {
                                                Top = -CORNER_RADIUS_HIDE_OFFSET,
                                                Left = -CORNER_RADIUS_HIDE_OFFSET
                                            },
                                            Spacing = new Vector2(0f, 4f),
                                            Direction = FillDirection.Vertical,
                                            Shear = OsuGame.SHEAR,
                                            Children = new Drawable[]
                                            {
                                                new ShearAligningWrapper(titleWedge = new BeatmapTitleWedge()),
                                                new ShearAligningWrapper(detailsArea = new BeatmapDetailsArea()),
                                            },
                                        },
                                        Empty(),
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Children = new CompositeDrawable[]
                                            {
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Padding = new MarginPadding
                                                    {
                                                        Top = FilterControl.HEIGHT_FROM_SCREEN_TOP + 5,
                                                        Bottom = 5,
                                                    },
                                                    Children = new Drawable[]
                                                    {
                                                        carousel = new BeatmapCarousel
                                                        {
                                                            BleedTop = FilterControl.HEIGHT_FROM_SCREEN_TOP + 5,
                                                            BleedBottom = ScreenFooter.HEIGHT + 5,
                                                            RequestPresentBeatmap = _ => OnStart(),
                                                            RelativeSizeAxes = Axes.Both,
                                                        },
                                                    }
                                                },
                                                filterControl = new FilterControl
                                                {
                                                    Anchor = Anchor.TopRight,
                                                    Origin = Anchor.TopRight,
                                                    RelativeSizeAxes = Axes.X,
                                                },
                                            }
                                        },
                                    },
                                }
                            },
                        }
                    },
                },
                modSelectOverlay,
            });
        }

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new ScreenFooterButton[]
        {
            new FooterButtonMods(modSelectOverlay) { Current = Mods },
            new FooterButtonRandom(),
            new FooterButtonOptions(),
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            modSelectOverlay.State.BindValueChanged(v =>
            {
                logo?.ScaleTo(v.NewValue == Visibility.Visible ? 0f : logo_scale, 400, Easing.OutQuint)
                    .FadeTo(v.NewValue == Visibility.Visible ? 0f : 1f, 200, Easing.OutQuint);
            }, true);
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            this.FadeIn();

            titleWedge.Show();
            detailsArea.Show();
            filterControl.Show();

            modSelectOverlay.SelectedMods.BindTo(Mods);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            this.FadeIn(fade_duration, Easing.OutQuint);

            carousel.VisuallyFocusSelected = false;

            titleWedge.Show();
            detailsArea.Show();
            filterControl.Show();

            // required due to https://github.com/ppy/osu-framework/issues/3218
            modSelectOverlay.SelectedMods.Disabled = false;
            modSelectOverlay.SelectedMods.BindTo(Mods);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.FadeOut(fade_duration, Easing.OutQuint);

            modSelectOverlay.SelectedMods.UnbindFrom(Mods);

            titleWedge.Hide();
            detailsArea.Hide();
            filterControl.Hide();

            carousel.VisuallyFocusSelected = true;

            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            this.FadeOut(fade_duration, Easing.OutQuint);

            titleWedge.Hide();
            detailsArea.Hide();
            filterControl.Hide();

            return base.OnExiting(e);
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (logo.Alpha > 0.8f)
                Footer?.StartTrackingLogo(logo, 400, Easing.OutQuint);
            else
            {
                logo.Hide();
                logo.ScaleTo(0.2f);
                Footer?.StartTrackingLogo(logo);
            }

            logo.FadeIn(240, Easing.OutQuint);
            logo.ScaleTo(logo_scale, 240, Easing.OutQuint);

            logo.Action = () =>
            {
                OnStart();
                return false;
            };
        }

        /// <summary>
        /// Called when a selection is made.
        /// </summary>
        /// <returns>If a resultant action occurred that takes the user away from SongSelect.</returns>
        protected abstract bool OnStart();

        protected override void LogoSuspending(OsuLogo logo)
        {
            base.LogoSuspending(logo);
            Footer?.StopTrackingLogo();
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);
            Scheduler.AddDelayed(() => Footer?.StopTrackingLogo(), 120);
            logo.ScaleTo(0.2f, 120, Easing.Out);
            logo.FadeOut(120, Easing.Out);
        }

        /// <summary>
        /// Set the query to the search text box.
        /// </summary>
        /// <param name="query">The string to search.</param>
        public void Search(string query)
        {
            carousel.Filter(new FilterCriteria
            {
                // TODO: this should only set the text of the current criteria, not use a completely new criteria.
                SearchText = query,
            });
        }

        protected override void Update()
        {
            base.Update();
            detailsArea.Height = wedgesContainer.DrawHeight - titleWedge.LayoutSize.Y - 4;
        }
    }
}
