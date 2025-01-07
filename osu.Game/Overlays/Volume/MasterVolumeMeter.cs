// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK.Graphics;

namespace osu.Game.Overlays.Volume
{
    public partial class MasterVolumeMeter : VolumeMeter
    {
        private MuteButton muteButton = null!;

        public Bindable<bool> IsMuted { get; } = new Bindable<bool>();

        private readonly BindableDouble muteAdjustment = new BindableDouble();

        [Resolved]
        private VolumeOverlay volumeOverlay { get; set; } = null!;

        public MasterVolumeMeter(string name, float circleSize, Color4 meterColour)
            : base(name, circleSize, meterColour)
        {
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            IsMuted.BindValueChanged(muted =>
            {
                if (muted.NewValue)
                    audio.AddAdjustment(AdjustableProperty.Volume, muteAdjustment);
                else
                    audio.RemoveAdjustment(AdjustableProperty.Volume, muteAdjustment);
            });

            Add(muteButton = new MuteButton
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.Centre,
                Blending = BlendingParameters.Additive,
                X = CircleSize / 2,
                Y = CircleSize * 0.23f,
                Current = { BindTarget = IsMuted }
            });

            muteButton.Current.ValueChanged += _ => volumeOverlay.Show();
        }

        public void ToggleMute() => muteButton.Current.Value = !muteButton.Current.Value;
    }
}
