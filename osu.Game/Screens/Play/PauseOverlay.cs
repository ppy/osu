// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class PauseOverlay : GameplayMenuOverlay
    {
        public Action OnResume;

        public override string Header => "paused";
        public override string Description => "you're not going to do what i think you're going to do, are ya?";

        private DrawableSample pauseLoop;

        protected override Action BackAction => () => InternalButtons.Children.First().Click();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, AudioManager audio, SkinManager skins)
        {
            AddButton("Continue", colours.Green, () => OnResume?.Invoke());
            AddButton("Retry", colours.YellowDark, () => OnRetry?.Invoke());
            AddButton("Quit", new Color4(170, 27, 39, 255), () => OnQuit?.Invoke());

            var sampleChannel = skins.GetSample(new SampleInfo("pause-loop")) ?? audio.Samples.Get(@"Gameplay/pause-loop");

            if (sampleChannel != null)
            {
                AddInternal(pauseLoop = new DrawableSample(sampleChannel)
                {
                    Looping = true,
                });
            }
        }

        protected override void PopIn()
        {
            base.PopIn();
            pauseLoop?.Play();
            pauseLoop?.VolumeTo(1.0f, 400, Easing.InQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();
            pauseLoop?.VolumeTo(0.0f);
            pauseLoop?.Stop();
        }
    }
}
