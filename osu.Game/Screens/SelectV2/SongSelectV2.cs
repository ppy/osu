// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
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
        private readonly ModSelectOverlay modSelectOverlay = new SoloModSelectOverlay();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        public override bool ShowFooter => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
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

        public override void OnEntering(ScreenTransitionEvent e)
        {
            this.FadeIn();
            base.OnEntering(e);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            this.FadeIn();
            base.OnResuming(e);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.Delay(400).FadeOut();
            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            this.Delay(400).FadeOut();
            return base.OnExiting(e);
        }

        public override bool OnBackButton()
        {
            if (modSelectOverlay.State.Value == Visibility.Visible)
            {
                modSelectOverlay.Hide();
                return true;
            }

            return false;
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
            logo.ScaleTo(0.4f, 240, Easing.OutQuint);

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

        private partial class SoloModSelectOverlay : ModSelectOverlay
        {
            protected override bool ShowPresets => true;

            public SoloModSelectOverlay()
                : base(OverlayColourScheme.Aquamarine)
            {
            }
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
