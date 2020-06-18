// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboardSample : Component
    {
        /// <summary>
        /// The amount of time allowable beyond the start time of the sample, for the sample to start.
        /// </summary>
        private const double allowable_late_start = 100;

        private readonly StoryboardSampleInfo sampleInfo;

        protected SampleChannel Channel;

        public override bool RemoveWhenNotAlive => false;

        public DrawableStoryboardSample(StoryboardSampleInfo sampleInfo)
        {
            this.sampleInfo = sampleInfo;
            LifetimeStart = sampleInfo.StartTime;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap, IBindable<IReadOnlyList<Mod>> mods)
        {
            Channel = beatmap.Value.Skin.GetSample(sampleInfo);

            if (Channel != null)
            {
                Channel.Volume.Value = sampleInfo.Volume / 100.0;

                foreach (var mod in mods.Value.OfType<IApplicableToSample>())
                    mod.ApplyToSample(Channel);
            }
        }

        protected override void Update()
        {
            base.Update();

            // TODO: this logic will need to be consolidated with other game samples like hit sounds.
            if (Time.Current < sampleInfo.StartTime)
            {
                // We've rewound before the start time of the sample
                Channel?.Stop();

                // In the case that the user fast-forwards to a point far beyond the start time of the sample,
                // we want to be able to fall into the if-conditional below (therefore we must not have a life time end)
                LifetimeStart = sampleInfo.StartTime;
                LifetimeEnd = double.MaxValue;
            }
            else if (Time.Current - Time.Elapsed <= sampleInfo.StartTime)
            {
                // We've passed the start time of the sample. We only play the sample if we're within an allowable range
                // from the sample's start, to reduce layering if we've been fast-forwarded far into the future
                if (Time.Current - sampleInfo.StartTime < allowable_late_start)
                    Channel?.Play();

                // In the case that the user rewinds to a point far behind the start time of the sample,
                // we want to be able to fall into the if-conditional above (therefore we must not have a life time start)
                LifetimeStart = double.MinValue;
                LifetimeEnd = sampleInfo.StartTime;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            Channel?.Stop();
            base.Dispose(isDisposing);
        }
    }
}
