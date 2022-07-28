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
    public abstract class SongProgress : OverlayContainer, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private GameplayClockContainer? gameplayClockContainer { get; set; }

        [Resolved(canBeNull: true)]
        private DrawableRuleset? drawableRuleset { get; set; }

        private IClock? referenceClock;
        private IEnumerable<HitObject>? objects;

        public IEnumerable<HitObject> Objects
        {
            set
            {
                objects = value;
                FirstHitTime = objects?.FirstOrDefault()?.StartTime ?? 0;
                LastHitTime = objects?.LastOrDefault()?.GetEndTime() ?? 0;
                UpdateObjects(objects);
            }
        }

        protected double FirstHitTime { get; private set; }

        //TODO: this isn't always correct (consider mania where a non-last object may last for longer than the last in the list).
        protected double LastHitTime { get; private set; }

        protected abstract void UpdateProgress(double progress, bool isIntro);
        protected abstract void UpdateObjects(IEnumerable<HitObject>? objects);

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
            // However, if no drawable ruleset is available (i.e. used in tests), we fall back to either the gameplay clock container or this drawable's own clock.
            double gameplayTime = referenceClock?.CurrentTime ?? gameplayClockContainer?.GameplayClock.CurrentTime ?? Time.Current;

            if (gameplayTime < FirstHitTime)
            {
                double earliest = gameplayClockContainer?.StartTime ?? 0;
                double introDuration = FirstHitTime - earliest;
                double currentIntroTime = gameplayTime - earliest;
                UpdateProgress(currentIntroTime / introDuration, true);
            }
            else
            {
                double duration = LastHitTime - FirstHitTime;
                double currentTime = gameplayTime - FirstHitTime;
                UpdateProgress(currentTime / duration, false);
            }
        }
    }
}
