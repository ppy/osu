// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public abstract class SongProgress : OverlayContainer, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private GameplayClock gameplayClock { get; set; }

        [Resolved(canBeNull: true)]
        private DrawableRuleset drawableRuleset { get; set; }

        [Resolved(canBeNull: true)]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        private IClock referenceClock;
        private IEnumerable<HitObject> objects;

        public IEnumerable<HitObject> Objects
        {
            set => UpdateObjects(objects = value);
        }

        protected double FirstHitTime => objects.FirstOrDefault()?.StartTime ?? 0;

        //TODO: this isn't always correct (consider mania where a non-last object may last for longer than the last in the list).
        protected double LastHitTime => objects.LastOrDefault()?.GetEndTime() ?? 0;

        protected double FirstEventTime { get; private set; }

        protected abstract void UpdateProgress(double progress, double time, bool isIntro);
        protected abstract void UpdateObjects(IEnumerable<HitObject> objects);

        [BackgroundDependencyLoader]
        private void load()
        {
            if (drawableRuleset != null)
            {
                Objects = drawableRuleset.Objects;
                referenceClock = drawableRuleset.FrameStableClock;
            }

            if (beatmap != null)
            {
                FirstEventTime = beatmap.Value.Storyboard.EarliestEventTime ?? 0;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (objects == null)
                return;

            double gameplayTime = gameplayClock?.CurrentTime ?? Time.Current;
            double frameStableTime = referenceClock?.CurrentTime ?? gameplayTime;

            if (frameStableTime < FirstHitTime)
            {
                UpdateProgress((frameStableTime - FirstEventTime) / (FirstHitTime - FirstEventTime), gameplayTime, true);
            }
            else
            {
                UpdateProgress((frameStableTime - FirstHitTime) / (LastHitTime - FirstHitTime), gameplayTime, false);
            }
        }
    }
}
