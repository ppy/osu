// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class SongProgress : OverlayContainer, ISkinnableDrawable
    {
        // Some implementations of this element allow seeking during gameplay playback.
        // Set a sane default of never handling input to override the behaviour provided by OverlayContainer.
        public override bool HandleNonPositionalInput => false;
        public override bool HandlePositionalInput => false;
        protected override bool BlockScrollInput => false;

        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        protected IGameplayClock GameplayClock { get; private set; } = null!;

        [Resolved(canBeNull: true)]
        private DrawableRuleset? drawableRuleset { get; set; }

        private IClock? referenceClock;
        private IEnumerable<HitObject>? objects;

        public IEnumerable<HitObject> Objects
        {
            set
            {
                objects = value;
                FirstHitTime = objects.FirstOrDefault()?.StartTime ?? 0;
                //TODO: this isn't always correct (consider mania where a non-last object may last for longer than the last in the list).
                LastHitTime = objects.LastOrDefault()?.GetEndTime() ?? 0;
                UpdateObjects(objects);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Show();
        }

        protected double FirstHitTime { get; private set; }

        protected double LastHitTime { get; private set; }

        protected abstract void UpdateProgress(double progress, bool isIntro);
        protected virtual void UpdateObjects(IEnumerable<HitObject> objects) { }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (drawableRuleset != null)
            {
                Objects = drawableRuleset.Objects;
                referenceClock = drawableRuleset.FrameStableClock;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (objects == null)
                return;

            // The reference clock is used to accurately tell the playfield's time. This is obtained from the drawable ruleset.
            // However, if no drawable ruleset is available (i.e. used in tests), we fall back to the gameplay clock.
            double currentTime = referenceClock?.CurrentTime ?? GameplayClock.CurrentTime;

            bool isInIntro = currentTime < FirstHitTime;

            if (isInIntro)
            {
                double introStartTime = GameplayClock.StartTime;

                double introOffsetCurrent = currentTime - introStartTime;
                double introDuration = FirstHitTime - introStartTime;

                UpdateProgress(introOffsetCurrent / introDuration, true);
            }
            else
            {
                double objectOffsetCurrent = currentTime - FirstHitTime;

                double objectDuration = LastHitTime - FirstHitTime;
                if (objectDuration == 0)
                    UpdateProgress(0, false);
                else
                    UpdateProgress(objectOffsetCurrent / objectDuration, false);
            }
        }
    }
}
