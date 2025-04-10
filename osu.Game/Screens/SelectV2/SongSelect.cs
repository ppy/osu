// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// This screen is intended to house all components introduced in the new song select design to add transitions and examine the overall look.
    /// This will be gradually built upon and ultimately replace <see cref="Select.SongSelect"/> once everything is in place.
    /// </summary>
    public abstract partial class SongSelect : ScreenWithBeatmapBackground
    {
        private const float logo_scale = 0.4f;

        public const float WEDGE_CONTENT_MARGIN = CORNER_RADIUS_HIDE_OFFSET + OsuGame.SCREEN_EDGE_MARGIN;

        public const float CORNER_RADIUS_HIDE_OFFSET = 20f;

        public const double ENTER_DURATION = 600;

        private const double fade_duration = 300;

        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private readonly ModSelectOverlay modSelectOverlay = new UserModSelectOverlay(OverlayColourScheme.Aquamarine)
        {
            ShowPresets = true,
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private BeatmapCarousel carousel = null!;

        private BeatmapFilterControl filterControl = null!;
        private BeatmapInfoWedge infoWedge = null!;
        private BeatmapWedgesArea wedgesArea = null!;
        private FillFlowContainer wedgesContainer = null!;

        public override bool ShowFooter => true;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private OsuLogo? logo { get; set; }

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new ScreenFooterButton[]
        {
            new FooterButtonMods(modSelectOverlay) { Current = Mods },
            new FooterButtonRandom(),
            new FooterButtonOptions(),
        };

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
                                            Children = new Drawable[]
                                            {
                                                new ShearAlignedDrawable(shear, infoWedge = new BeatmapInfoWedge()),
                                                new ShearAlignedDrawable(shear, wedgesArea = new BeatmapWedgesArea()),
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
                                                        Top = BeatmapFilterControl.HEIGHT_FROM_SCREEN_TOP + 5,
                                                        Bottom = 5,
                                                    },
                                                    Children = new Drawable[]
                                                    {
                                                        carousel = new BeatmapCarousel
                                                        {
                                                            BleedTop = BeatmapFilterControl.HEIGHT_FROM_SCREEN_TOP + 5,
                                                            BleedBottom = ScreenFooter.HEIGHT + 5,
                                                            RequestSelectBeatmap = b => Beatmap.Value = beatmaps.GetWorkingBeatmap(b),
                                                            RequestPresentBeatmap = _ => OnStart(),
                                                            RelativeSizeAxes = Axes.Both,
                                                        },
                                                    }
                                                },
                                                filterControl = new BeatmapFilterControl
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

        /// <summary>
        /// Called when a selection is made.
        /// </summary>
        /// <returns>If a resultant action occurred that takes the user away from SongSelect.</returns>
        protected abstract bool OnStart();

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            this.FadeIn();

            Beatmap.BindValueChanged(onBeatmapChanged, true);

            infoWedge.Show();
            wedgesArea.Show();
            filterControl.Show();

            modSelectOverlay.State.BindValueChanged(onModSelectStateChanged, true);
            modSelectOverlay.SelectedMods.BindTo(Mods);

            updateScreenBackground();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            this.FadeIn(fade_duration, Easing.OutQuint);

            carousel.VisuallyFocusSelected = false;

            infoWedge.Show();
            wedgesArea.Show();
            filterControl.Show();

            // required due to https://github.com/ppy/osu-framework/issues/3218
            modSelectOverlay.SelectedMods.Disabled = false;
            modSelectOverlay.SelectedMods.BindTo(Mods);

            updateScreenBackground();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.FadeOut(fade_duration, Easing.OutQuint);

            modSelectOverlay.SelectedMods.UnbindFrom(Mods);

            infoWedge.Hide();
            wedgesArea.Hide();
            filterControl.Hide();

            carousel.VisuallyFocusSelected = true;

            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            this.FadeOut(fade_duration, Easing.OutQuint);

            infoWedge.Hide();
            wedgesArea.Hide();
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

        private void onBeatmapChanged(ValueChangedEvent<WorkingBeatmap> b)
        {
            if (this.IsCurrentScreen())
                updateScreenBackground();
        }

        private void updateScreenBackground()
        {
            ApplyToBackground(backgroundModeBeatmap =>
            {
                backgroundModeBeatmap.Beatmap = Beatmap.Value;
                backgroundModeBeatmap.DimWhenUserSettingsIgnored.Value = 0.25f;
                backgroundModeBeatmap.BlurAmount.Value = 0f;
                backgroundModeBeatmap.IgnoreUserSettings.Value = true;
                backgroundModeBeatmap.FadeColour(Color4.White, 250);
            });
        }

        private void onModSelectStateChanged(ValueChangedEvent<Visibility> v)
        {
            if (v.NewValue == Visibility.Visible)
                logo?.ScaleTo(0f, 400, Easing.OutQuint).FadeTo(0f, 200, Easing.OutQuint);
            else
                logo?.ScaleTo(logo_scale, 400, Easing.OutQuint).FadeTo(1f, 200, Easing.OutQuint);
        }

        protected override void Update()
        {
            base.Update();
            wedgesArea.Height = wedgesContainer.DrawHeight - infoWedge.LayoutSize.Y - 4;
        }
    }
}
