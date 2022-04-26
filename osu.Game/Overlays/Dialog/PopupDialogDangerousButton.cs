// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Dialog
{
    public class PopupDialogDangerousButton : PopupDialogButton
    {
        private Box progressBox;
        private DangerousConfirmContainer confirmContainer;

        [Resolved]
        private AudioManager audioManager { get; set; }

        private readonly BindableDouble speedChange = new BindableDouble();

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

            confirmContainer.Progress.BindValueChanged(progress => speedChange.Value = 1d - progress.NewValue, true);

            audioManager.Tracks.AddAdjustment(AdjustableProperty.Frequency, speedChange);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            confirmContainer.Progress.BindValueChanged(progress => progressBox.Width = (float)progress.NewValue, true);
        }

        protected override void Dispose(bool isDisposing)
        {
            audioManager?.Tracks.RemoveAdjustment(AdjustableProperty.Frequency, speedChange);
            base.Dispose(isDisposing);
        }

        private class DangerousConfirmContainer : HoldToConfirmContainer
        {
            protected override double? HoldActivationDelay => 500;

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                BeginConfirm();
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (!e.HasAnyButtonPressed)
                    AbortConfirm();
            }
        }
    }
}
