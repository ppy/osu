// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osu.Game.Screens.SelectV2.Footer;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// This screen is intended to house all components introduced in the new song select design to add transitions and examine the overall look.
    /// This will be gradually built upon and ultimately replace <see cref="Select.SongSelect"/> once everything is in place.
    /// </summary>
    public abstract partial class SongSelect : OsuScreen
    {
        private const float logo_scale = 0.4f;

        private readonly ModSelectOverlay modSelectOverlay = new ModSelectOverlay
        {
            ShowPresets = true,
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private BeatmapCarousel carousel = null!;

        public override bool ShowFooter => true;

        [Resolved]
        private OsuLogo? logo { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new GridContainer // used for max width implementation
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Relative, 0.5f, maxSize: 750),
                    },
                    Content = new[]
                    {
                        new[]
                        {
                            Empty(),
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Bottom = ScreenFooter.HEIGHT },
                                Child = carousel = new BeatmapCarousel
                                {
                                    RequestPresentBeatmap = _ => OnStart(),
                                    RelativeSizeAxes = Axes.Both
                                },
                            },
                        }
                    }
                },
                modSelectOverlay,
            });
        }

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new ScreenFooterButton[]
        {
            new ScreenFooterButtonMods(modSelectOverlay) { Current = Mods },
            new ScreenFooterButtonRandom(),
            new ScreenFooterButtonOptions(),
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
            this.FadeIn();

            modSelectOverlay.SelectedMods.BindTo(Mods);

            base.OnEntering(e);
        }

        private const double fade_duration = 300;

        public override void OnResuming(ScreenTransitionEvent e)
        {
            this.FadeIn(fade_duration, Easing.OutQuint);

            carousel.VisuallyFocusSelected = false;

            // required due to https://github.com/ppy/osu-framework/issues/3218
            modSelectOverlay.SelectedMods.Disabled = false;
            modSelectOverlay.SelectedMods.BindTo(Mods);

            base.OnResuming(e);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.Delay(100).FadeOut(fade_duration, Easing.OutQuint);

            modSelectOverlay.SelectedMods.UnbindFrom(Mods);

            carousel.VisuallyFocusSelected = true;

            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            this.FadeOut(fade_duration, Easing.OutQuint);
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
    }
}
