// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    public class SkinnableSound : SkinReloadableDrawable
    {
        // The second element is false if osu!'s default sound should be used for this sample
        private readonly (ISampleInfo, bool)[] hitSamples;

        private List<(AdjustableProperty property, BindableDouble bindable)> adjustments;

        private SampleChannel[] channels;

        [Resolved]
        private ISampleStore samples { get; set; }

        public SkinnableSound(IEnumerable<HitSampleInfo> hitSamples)
        {
            this.hitSamples = hitSamples.Select<HitSampleInfo, (ISampleInfo, bool)>(s => (s, s.IsCustom)).ToArray();
        }

        public SkinnableSound(IEnumerable<ISampleInfo> hitSamples)
        {
            this.hitSamples = hitSamples.Select(s => (s, true)).ToArray();
        }

        public SkinnableSound(ISampleInfo hitSamples)
        {
            this.hitSamples = new[] { (hitSamples, true) };
        }

        private bool looping;

        public bool Looping
        {
            get => looping;
            set
            {
                if (value == looping) return;

                looping = value;

                channels?.ForEach(c => c.Looping = looping);
            }
        }

        public void Play() => channels?.ForEach(c => c.Play());

        public void Stop() => channels?.ForEach(c => c.Stop());

        public void AddAdjustment(AdjustableProperty type, BindableDouble adjustBindable)
        {
            if (adjustments == null) adjustments = new List<(AdjustableProperty, BindableDouble)>();

            adjustments.Add((type, adjustBindable));
            channels?.ForEach(c => c.AddAdjustment(type, adjustBindable));
        }

        public void RemoveAdjustment(AdjustableProperty type, BindableDouble adjustBindable)
        {
            adjustments?.Remove((type, adjustBindable));
            channels?.ForEach(c => c.RemoveAdjustment(type, adjustBindable));
        }

        public override bool IsPresent => Scheduler.HasPendingTasks;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            channels = hitSamples.Select(sample =>
            {
                var s = sample.Item1;
                var isCustom = sample.Item2;

                var ch = isCustom ? skin.GetSample(s) : null;

                if (ch == null && allowFallback)
                {
                    foreach (var lookup in s.LookupNames)
                    {
                        if ((ch = samples.Get($"Gameplay/{lookup}")) != null)
                            break;
                    }
                }

                if (ch != null)
                {
                    ch.Looping = looping;
                    ch.Volume.Value = s.Volume / 100.0;

                    if (adjustments != null)
                    {
                        foreach (var (property, bindable) in adjustments)
                            ch.AddAdjustment(property, bindable);
                    }
                }

                return ch;
            }).Where(c => c != null).ToArray();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (channels != null)
            {
                foreach (var c in channels)
                    c.Dispose();
            }
        }
    }
}
