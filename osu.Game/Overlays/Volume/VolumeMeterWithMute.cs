// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK.Graphics;

namespace osu.Game.Overlays.Volume
{
    public enum MuteMode
    {
        Master,
        Effects,
        Music,
    }

    public partial class VolumeMeterWithMute : VolumeMeter
    {
        private MuteButton muteButton = null!;

        public Bindable<bool> IsMuted { get; } = new Bindable<bool>();

        private readonly BindableDouble muteAdjustment = new BindableDouble();

        private readonly MuteMode mode;

        [Resolved]
        private VolumeOverlay volumeOverlay { get; set; } = null!;

        public VolumeMeterWithMute(string name, float circleSize, Color4 meterColour, MuteMode muteMode)
            : base(name, circleSize, meterColour)
        {
            mode = muteMode;
        }

        private IAdjustableAudioComponent getAudioComponent(AudioManager audio)
        {
            switch (mode)
            {
                case MuteMode.Master:
                    return audio;

                case MuteMode.Effects:
                    return audio.Samples;

                case MuteMode.Music:
                    return audio.Tracks;

                default:
                    throw new System.Diagnostics.UnreachableException();
            }
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            IsMuted.BindValueChanged(muted =>
            {
                IAdjustableAudioComponent component = getAudioComponent(audio);
                if (muted.NewValue)
                    component.AddAdjustment(AdjustableProperty.Volume, muteAdjustment);
                else
                    component.RemoveAdjustment(AdjustableProperty.Volume, muteAdjustment);
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
