// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Game.Screens.SelectV2.Footer;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// This screen is intended to house all components introduced in the new song select design to add transitions and examine the overall look.
    /// This will be gradually built upon and ultimately replace <see cref="Select.SongSelect"/> once everything is in place.
    /// </summary>
    public partial class SongSelectV2 : OsuScreen
    {
        private const float logo_scale = 0.4f;

        private readonly ModSelectOverlay modSelectOverlay = new SoloModSelectOverlay();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        public override bool ShowFooter => true;

        [Resolved]
        private OsuLogo? logo { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Bottom = ScreenFooter.HEIGHT },
                    Child = new BeatmapCarousel
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.6f,
                    },
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

        public override void OnResuming(ScreenTransitionEvent e)
        {
            this.FadeIn();

            // required due to https://github.com/ppy/osu-framework/issues/3218
            modSelectOverlay.SelectedMods.Disabled = false;
            modSelectOverlay.SelectedMods.BindTo(Mods);

            base.OnResuming(e);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.Delay(400).FadeOut();

            modSelectOverlay.SelectedMods.UnbindFrom(Mods);

            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            this.Delay(400).FadeOut();
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
                this.Push(new PlayerLoaderV2(() => new SoloPlayer()));
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

        private partial class SoloModSelectOverlay : UserModSelectOverlay
        {
            protected override bool ShowPresets => true;
        }

        private partial class PlayerLoaderV2 : PlayerLoader
        {
            public override bool ShowFooter => true;

            public PlayerLoaderV2(Func<Player> createPlayer)
                : base(createPlayer)
            {
            }
        }
    }
}
