// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    /// <summary>
    /// An overlay which will display a black screen that dims over a period before confirming an exit action.
    /// Action is BYO (derived class will need to call <see cref="HoldToConfirmContainer.BeginConfirm"/> and <see cref="HoldToConfirmContainer.AbortConfirm"/> from a user event).
    /// </summary>
    public abstract partial class HoldToConfirmOverlay : HoldToConfirmContainer
    {
        private Box overlay;

        private readonly BindableDouble audioVolume = new BindableDouble(1);

        [Resolved]
        private AudioManager audio { get; set; }

        private readonly float finalFillAlpha;

        protected HoldToConfirmOverlay(float finalFillAlpha = 1)
        {
            this.finalFillAlpha = finalFillAlpha;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;

            Children = new Drawable[]
            {
                overlay = new Box
                {
                    Alpha = 0,
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                }
            };

            Progress.ValueChanged += p =>
            {
                double target = p.NewValue * finalFillAlpha;

                audioVolume.Value = 1 - target;
                overlay.Alpha = (float)target;
            };

            audio.Tracks.AddAdjustment(AdjustableProperty.Volume, audioVolume);
        }

        protected override void Dispose(bool isDisposing)
        {
            audio?.Tracks.RemoveAdjustment(AdjustableProperty.Volume, audioVolume);
            base.Dispose(isDisposing);
        }
    }
}
