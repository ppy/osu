// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public partial class BPMCounter : RollingCounter<double>, ISkinnableDrawable
    {
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        protected IGameplayClock GameplayClock { get; private set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Colour = colour.BlueLighter;
            Current.Value = DisplayedCount = 0;
        }

        protected override void Update()
        {
            base.Update();

            //We dont want it going to 0 when we pause. so we block the updates
            if (GameplayClock.IsPaused.Value) return;

            // We want to check Rate every update to cover windup/down
            Current.Value = beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(GameplayClock.CurrentTime).BPM * GameplayClock.Rate;
        }

        protected override OsuSpriteText CreateSpriteText()
            => base.CreateSpriteText().With(s => s.Font = s.Font.With(size: 20f, fixedWidth: true));

        protected override LocalisableString FormatCount(double count)
        {
            return $@"{count:0} BPM";
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
