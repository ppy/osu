// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Screens.Play;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Represents a <see cref="SkinnableSound"/> that pauses its samples and resumes them based on the cached <see cref="GameplayClock"/>'s state.
    /// </summary>
    // todo: there should be a sample store cached adjusted to follow the gameplay clock's rate instead, but that requires https://github.com/ppy/osu-framework/issues/2760 first.
    public class GameplaySkinnableSound : SkinnableSound
    {
        private bool requestedPlaying;

        public GameplaySkinnableSound(ISampleInfo hitSamples)
            : base(hitSamples)
        {
        }

        public GameplaySkinnableSound(IEnumerable<ISampleInfo> hitSamples)
            : base(hitSamples)
        {
        }

        private IBindable<bool> gameplayPaused;

        [BackgroundDependencyLoader]
        private void load(GameplayClock gameplayClock)
        {
            gameplayPaused = gameplayClock.IsPaused.GetBoundCopy();
            gameplayPaused.BindValueChanged(paused =>
            {
                if (requestedPlaying)
                {
                    if (paused.NewValue)
                        base.Stop();
                    // it's not easy to know if a sample has finished playing (to end).
                    // to keep things simple only resume playing looping samples.
                    else if (Looping)
                        base.Play();
                }
            });
        }

        public override void Play()
        {
            requestedPlaying = true;
            base.Play();
        }

        public override void Stop()
        {
            requestedPlaying = false;
            base.Stop();
        }
    }
}
