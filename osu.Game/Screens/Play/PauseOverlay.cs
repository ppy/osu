// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Audio;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play
{
    public partial class PauseOverlay : GameplayMenuOverlay
    {
        public override bool IsPresent => base.IsPresent || pauseLoop.IsPlaying;

        public override LocalisableString Header => GameplayMenuOverlayStrings.PausedHeader;

        private SkinnableSound pauseLoop;

        protected override Action BackAction => () =>
        {
            if (Buttons.Any())
                Buttons.First().TriggerClick();
            else
                OnResume?.Invoke();
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(pauseLoop = new SkinnableSound(new SampleInfo("Gameplay/pause-loop"))
            {
                Looping = true,
                Volume = { Value = 0 }
            });
        }

        public void StopAllSamples()
        {
            if (!IsLoaded)
                return;

            pauseLoop.Stop();
        }

        protected override void PopIn()
        {
            base.PopIn();

            pauseLoop.VolumeTo(1.0f, TRANSITION_DURATION, Easing.InQuint);
            pauseLoop.Play();
        }

        protected override void PopOut()
        {
            base.PopOut();

            pauseLoop.VolumeTo(0, TRANSITION_DURATION, Easing.OutQuad).Finally(_ => pauseLoop.Stop());
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.PauseGameplay:
                    InternalButtons.First().TriggerClick();
                    return true;
            }

            return base.OnPressed(e);
        }
    }
}
