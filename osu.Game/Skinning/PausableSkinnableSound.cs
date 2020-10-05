// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Screens.Play;

namespace osu.Game.Skinning
{
    public class PausableSkinnableSound : SkinnableSound
    {
        protected bool RequestedPlaying { get; private set; }

        public PausableSkinnableSound(ISampleInfo hitSamples)
            : base(hitSamples)
        {
        }

        public PausableSkinnableSound(IEnumerable<ISampleInfo> hitSamples)
            : base(hitSamples)
        {
        }

        private readonly IBindable<bool> samplePlaybackDisabled = new Bindable<bool>();

        [BackgroundDependencyLoader(true)]
        private void load(ISamplePlaybackDisabler samplePlaybackDisabler)
        {
            // if in a gameplay context, pause sample playback when gameplay is paused.
            if (samplePlaybackDisabler != null)
            {
                samplePlaybackDisabled.BindTo(samplePlaybackDisabler.SamplePlaybackDisabled);
                samplePlaybackDisabled.BindValueChanged(disabled =>
                {
                    if (!RequestedPlaying) return;

                    // let non-looping samples that have already been started play out to completion (sounds better than abruptly cutting off).
                    if (!Looping) return;

                    if (disabled.NewValue)
                        base.Stop();
                    else
                    {
                        // schedule so we don't start playing a sample which is no longer alive.
                        Schedule(() =>
                        {
                            if (RequestedPlaying)
                                base.Play();
                        });
                    }
                });
            }
        }

        public override void Play()
        {
            RequestedPlaying = true;

            if (samplePlaybackDisabled.Value)
                return;

            base.Play();
        }

        public override void Stop()
        {
            RequestedPlaying = false;
            base.Stop();
        }
    }
}
