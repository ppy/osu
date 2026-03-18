// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens.Footer;
using osu.Game.Rulesets;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public partial class FooterButtonSearch : ScreenFooterButton, IHasPopover
    {
        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> workingBeatmap { get; set; } = null!;

        [Resolved]
        private ISongSelect? songSelect { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private BeatmapListingOverlay beatmapListing { get; set; } = null!;

        private Live<BeatmapInfo> beatmap = null!;
        private bool f4WasPressedLastFrame;

        public IBindable<WorkingBeatmap> WorkingBeatmap => workingBeatmap;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        public IBindable<RulesetInfo> RulesetBindable => ruleset;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Text = "Search";
            Icon = FontAwesome.Solid.Search;
            AccentColour = colour.Purple1;

            Action = this.ShowPopover;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            workingBeatmap.BindValueChanged(_ => beatmapChanged(), true);
        }

        protected override void Update()
        {
            base.Update();
            var inputManager = GetContainingInputManager();
            if (inputManager == null) return;

            var keyboard = inputManager.CurrentState.Keyboard;
            bool f4Pressed = keyboard.Keys.IsPressed(Key.F4);
            if (f4Pressed && !f4WasPressedLastFrame)
                Action?.Invoke();

            f4WasPressedLastFrame = f4Pressed;
        }

        private void beatmapChanged()
        {
            this.HidePopover();
            Enabled.Value = !workingBeatmap.IsDefault;
            if (!workingBeatmap.IsDefault)
                beatmap = realm.Run(r => r.Find<BeatmapInfo>(workingBeatmap.Value.BeatmapInfo.ID)!.ToLive(realm));
        }

        public Framework.Graphics.UserInterface.Popover GetPopover()
        {
            return new SearchPopover(this, beatmap.Value.Detach(), colourProvider, beatmapListing, songSelect);
        }
    }
}
