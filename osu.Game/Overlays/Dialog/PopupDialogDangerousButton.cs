// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Audio.Effects;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Dialog
{
    public partial class PopupDialogDangerousButton : PopupDialogButton
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

        private partial class DangerousConfirmContainer : HoldToConfirmContainer
        {
            public DangerousConfirmContainer()
                : base(isDangerousAction: true)
            {
            }

            private Sample tickSample;
            private Sample confirmSample;
            private double lastTickPlaybackTime;
            private AudioFilter lowPassFilter = null!;
            private bool mouseDown;

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                tickSample = audio.Samples.Get(@"UI/dialog-dangerous-tick");
                confirmSample = audio.Samples.Get(@"UI/dialog-dangerous-select");

                AddInternal(lowPassFilter = new AudioFilter(audio.SampleMixer));
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Progress.BindValueChanged(progressChanged);
            }

            protected override void AbortConfirm()
            {
                lowPassFilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF);
                base.AbortConfirm();
            }

            protected override void Confirm()
            {
                lowPassFilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF);
                confirmSample?.Play();
                base.Confirm();
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                BeginConfirm();
                mouseDown = true;
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (!e.HasAnyButtonPressed)
                {
                    AbortConfirm();
                    mouseDown = false;
                }
            }

            protected override bool OnHover(HoverEvent e)
            {
                if (mouseDown)
                    BeginConfirm();

                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                if (!mouseDown) return;

                AbortConfirm();
            }

            private void progressChanged(ValueChangedEvent<double> progress)
            {
                if (progress.NewValue < progress.OldValue) return;

                if (Clock.CurrentTime - lastTickPlaybackTime < 30) return;

                lowPassFilter.CutoffTo((int)(progress.NewValue * AudioFilter.MAX_LOWPASS_CUTOFF * 0.5));

                var channel = tickSample.GetChannel();

                channel.Frequency.Value = 1 + progress.NewValue * 0.5f;
                channel.Volume.Value = 0.5f + progress.NewValue / 2f;

                channel.Play();

                lastTickPlaybackTime = Clock.CurrentTime;
            }
        }
    }
}
