// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Screens;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.SelectV2.Footer;

namespace osu.Game.Screens.SelectV2
{
    public partial class SongSelectV2 : OsuScreen
    {
        private FooterV2 footer = null!;

        private ModSelectOverlay overlay = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = footer = new FooterV2(),
                },
                overlay = new SoloModSelectOverlay()
            });

            footer.AddButton(new FooterButtonModsV2(), overlay);
            footer.AddButton(new FooterButtonRandomV2());
            footer.AddButton(new FooterButtonOptionsV2());

            overlay.Hide();
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            footer.Show();
            this.FadeIn();
            base.OnEntering(e);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            footer.Show();
            this.FadeIn();
            base.OnResuming(e);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            footer.Hide();
            this.Delay(400).FadeOut();
            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            footer.Hide();
            this.Delay(400).FadeOut();
            return base.OnExiting(e);
        }

        private partial class SoloModSelectOverlay : ModSelectOverlay
        {
            protected override bool ShowPresets => true;
        }
    }
}
