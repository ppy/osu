// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Audio;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    public class SkinnableSound : SkinReloadableDrawable
    {
        private readonly ISampleInfo[] hitSamples;

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

        public void Play() => InternalChildren.OfType<DrawableSample>()?.ForEach(c =>
        {
            if (c.AggregateVolume.Value > 0)
                c.Play();
        });

        public override bool IsPresent => false; // We don't need to receive updates.

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            var channels = hitSamples.Select(s =>
            {
                var ch = loadChannel(s, skin.GetSample);
                if (ch == null && allowFallback)
                    ch = loadChannel(s, audio.Samples.Get);
                return ch;
            }).Where(c => c != null);

            ClearInternal();
            foreach (var c in channels)
                AddInternal(new DrawableSample(c));
        }

        private SampleChannel loadChannel(ISampleInfo info, Func<string, SampleChannel> getSampleFunction)
        {
            foreach (var lookup in info.LookupNames)
            {
                var ch = getSampleFunction($"Gameplay/{lookup}");
                if (ch == null)
                    continue;

                ch.Volume.Value = info.Volume / 100.0;
                return ch;
            }

            return null;
        }
    }
}
