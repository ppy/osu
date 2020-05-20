// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    public class SkinnableSound : SkinReloadableDrawable
    {
        private readonly ISampleInfo[] hitSamples;

        [Resolved]
        private ISampleStore samples { get; set; }

        public SkinnableSound(ISampleInfo hitSamples)
            : this(new[] { hitSamples })
        {
        }

        public SkinnableSound(IEnumerable<ISampleInfo> hitSamples)
        {
            this.hitSamples = hitSamples.ToArray();
            InternalChild = samplesContainer = new AudioContainer<DrawableSample>();
        }

        private bool looping;

        private readonly AudioContainer<DrawableSample> samplesContainer;

        public BindableNumber<double> Volume => samplesContainer.Volume;

        public BindableNumber<double> Balance => samplesContainer.Balance;

        public BindableNumber<double> Frequency => samplesContainer.Frequency;

        public BindableNumber<double> Tempo => samplesContainer.Tempo;

        public bool Looping
        {
            get => looping;
            set
            {
                if (value == looping) return;

                looping = value;

                samplesContainer.ForEach(c => c.Looping = looping);
            }
        }

        public void Play() => samplesContainer.ForEach(c =>
        {
            if (c.AggregateVolume.Value > 0)
                c.Play();
        });

        public void Stop() => samplesContainer.ForEach(c => c.Stop());

        public override bool IsPresent => Scheduler.HasPendingTasks;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            var channels = hitSamples.Select(s =>
            {
                var ch = skin.GetSample(s);

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
                }

                return ch;
            }).Where(c => c != null);

            samplesContainer.ChildrenEnumerable = channels.Select(c => new DrawableSample(c));
        }
    }
}
