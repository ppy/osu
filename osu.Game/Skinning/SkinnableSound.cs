// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    public class SkinnableSound : SkinReloadableDrawable
    {
        private readonly ISampleInfo[] hitSamples;

        private readonly List<(AdjustableProperty, BindableDouble)> adjustments = new List<(AdjustableProperty, BindableDouble)>();

        private SampleChannel[] channels;

        private AudioManager audio;

        public SkinnableSound(IEnumerable<ISampleInfo> hitSamples)
        {
            this.hitSamples = hitSamples.ToArray();
        }

        public SkinnableSound(ISampleInfo hitSamples)
        {
            this.hitSamples = new[] { hitSamples };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            this.audio = audio;
        }

        private bool looping;

        public bool Looping
        {
            get => looping;
            set
            {
                looping = value;

                channels?.ForEach(c => c.Looping = looping);
            }
        }

        public void Play() => channels?.ForEach(c => c.Play());
        public void Stop() => channels?.ForEach(c => c.Stop());

        public void AddAdjustment(AdjustableProperty type, BindableDouble adjustBindable)
        {
            adjustments.Add((type, adjustBindable));
            channels?.ForEach(c => c.AddAdjustment(type, adjustBindable));
        }

        public void RemoveAdjustment(AdjustableProperty type, BindableDouble adjustBindable)
        {
            adjustments.Remove((type, adjustBindable));
            channels?.ForEach(c => c.RemoveAdjustment(type, adjustBindable));
        }

        public override bool IsPresent => Scheduler.HasPendingTasks;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            channels = hitSamples.Select(s =>
            {
                var ch = skin.GetSample(s);

                if (ch == null && allowFallback)
                    foreach (var lookup in s.LookupNames)
                        if ((ch = audio.Samples.Get($"Gameplay/{lookup}")) != null)
                            break;

                if (ch != null)
                {
                    ch.Looping = looping;
                    ch.Volume.Value = s.Volume / 100.0;

                    foreach (var adjustment in adjustments)
                        ch.AddAdjustment(adjustment.Item1, adjustment.Item2);
                }

                return ch;
            }).Where(c => c != null).ToArray();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var c in channels)
                c.Dispose();
        }
    }
}
