// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Audio.Effects;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Dialog
{
    public class PopupDialogDangerousButton : PopupDialogButton
    {
        private Box progressBox;
        private DangerousConfirmContainer confirmContainer;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ButtonColour = colours.Red3;

            ColourContainer.Add(progressBox = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Blending = BlendingParameters.Additive,
            });

            AddInternal(confirmContainer = new DangerousConfirmContainer
            {
                Action = () => Action(),
                RelativeSizeAxes = Axes.Both,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            confirmContainer.Progress.BindValueChanged(progress => progressBox.Width = (float)progress.NewValue, true);
        }

        private class DangerousConfirmContainer : HoldToConfirmContainer
        {
            public DangerousConfirmContainer()
                : base(isDangerousAction: true)
            {
            }

            private Sample tickSample;
            private Sample confirmSample;
            private bool isTicking;
            private double lastTickPlaybackTime;
            private AudioFilter lowPassFilter = null!;

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                lowPassFilter = new AudioFilter(audio.SampleMixer);
                tickSample = audio.Samples.Get(@"UI/dialog-dangerous-tick");
                confirmSample = audio.Samples.Get(@"UI/dialog-dangerous-select");
            }

            protected override void Update()
            {
                base.Update();

                if (!isTicking) return;

                if (Precision.AlmostEquals(Progress.Value, 1))
                {
                    confirmSample?.Play();
                    isTicking = false;
                }

                if (Clock.CurrentTime - lastTickPlaybackTime >= 30)
                    playTick(Progress.Value);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                startTickSound();
                BeginConfirm();
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (!e.HasAnyButtonPressed)
                {
                    stopTickSound();
                    AbortConfirm();
                }
            }

            private void playTick(double progress)
            {
                lowPassFilter.CutoffTo((int)(Progress.Value * AudioFilter.MAX_LOWPASS_CUTOFF * 0.5f));

                tickSample.Frequency.Value = 1 + progress * 0.5f;
                tickSample.Volume.Value = 0.5f + progress / 2f;
                tickSample.Play();

                lastTickPlaybackTime = Clock.CurrentTime;
            }

            private void startTickSound()
            {
                lowPassFilter.CutoffTo(0);
                playTick(Progress.Value);
                isTicking = true;
            }

            private void stopTickSound()
            {
                isTicking = false;
                lastTickPlaybackTime = Clock.CurrentTime;
                lowPassFilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF);
            }
        }
    }
}
