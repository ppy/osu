// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class BPMCounter : RollingCounter<double>, ISkinnableDrawable
    {
        protected override double RollingDuration => 1000;

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        [Resolved]
        protected IGameplayClock GameplayClock { get; private set; } = null!;

        public BPMCounter()
        {
            Current.Value = 0;
        }

        protected override LocalisableString FormatCount(double count) => count.ToString(@"0 BPM");

        public BindableDouble GetBPM()
        {
            return new BindableDouble(gameplayState.Beatmap.ControlPointInfo.TimingPointAt(GameplayClock.CurrentTime).BPM);
        }

        protected override void Update()
        {
            base.Update();
            Current.Value = GetBPM().Value;
        }

        public bool UsesFixedAnchor { get; set; }
    }
}