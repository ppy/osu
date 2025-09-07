// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using ManagedBass.Fx;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Audio;
using osu.Game.Audio.Effects;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Skinning;
using osuTK;
using osu.Game.Localisation;

namespace osu.Game.Screens.Play
{
    public partial class ReplayFailIndicator : CompositeDrawable
    {
        public Action? GoToResults { get; init; }

        private readonly GameplayClockContainer gameplayClockContainer;
        private readonly BindableDouble trackFreq = new BindableDouble(1);
        private readonly BindableDouble volumeAdjustment = new BindableDouble(1);

        private Track track = null!;
        private SkinnableSound failSample = null!;
        private AudioFilter failLowPassFilter = null!;
        private AudioFilter failHighPassFilter = null!;
        private Container content = null!;

        private double? failTime;

        // relied on to make arbitrary seeks / rewinding work pretty well out-of-the-box, leveraging custom clock and absolute transform sequences
        public override bool RemoveCompletedTransforms => false;

        public ReplayFailIndicator(GameplayClockContainer gameplayClockContainer)
        {
            Clock = this.gameplayClockContainer = gameplayClockContainer;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, AudioManager audio, IBindable<WorkingBeatmap> beatmap, GameHost host)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            track = beatmap.Value.Track;

            RoundedButton goToResultsButton;

            InternalChildren = new Drawable[]
            {
                failSample = new SkinnableSound(new SampleInfo(@"Gameplay/failsound")),
                failLowPassFilter = new AudioFilter(audio.TrackMixer),
                failHighPassFilter = new AudioFilter(audio.TrackMixer, BQFType.HighPass),
                content = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 20,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Gray3,
                            Alpha = 0.8f,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Padding = new MarginPadding(20),
                            Spacing = new Vector2(15),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.Style.Title,
                                    Text = ReplayFailIndicatorStrings.ReplayFailed,
                                },
                                goToResultsButton = new RoundedButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Width = 150,
                                    Text = ReplayFailIndicatorStrings.GoToResults,
                                    Action = GoToResults,
                                }
                            }
                        }
                    }
                }
            };

            // every single component here is fine being synced to the gameplay clock...
            // except the "go to results" button, which starts having hover animations synced to the audio track
            // which is something that we don't want.
            // it is maybe probably possible to restructure the drawable hierarchy here to remove the button from under the gameplay clock,
            // but it would resort in uglier and more complicated drawable code.
            // thus, resort to the escape hatch extension method to ensure the button specifically still runs on the game update clock.
            goToResultsButton.ApplyGameWideClock(host);

            track.AddAdjustment(AdjustableProperty.Volume, volumeAdjustment);
            track.AddAdjustment(AdjustableProperty.Frequency, trackFreq);
        }

        public void Display()
        {
            failTime = Clock.CurrentTime;

            using (BeginAbsoluteSequence(failTime.Value))
            {
                // intentionally shorter than the actual fail animation
                const double audio_sweep_duration = 1000;

                content.FadeInFromZero(200, Easing.OutQuint);
                this.ScaleTo(1.1f, audio_sweep_duration, Easing.OutElasticHalf);
                this.TransformBindableTo(trackFreq, 0, audio_sweep_duration);
                this.TransformBindableTo(volumeAdjustment, 0.5);
                failHighPassFilter.CutoffTo(300);
                failLowPassFilter.CutoffTo(300, audio_sweep_duration, Easing.OutCubic);
            }
        }

        private bool failSamplePlaybackInitiated;

        protected override void Update()
        {
            base.Update();

            // the playback of the fail sample is the one thing that cannot be easily written using rewindable transforms and such.
            // this part needs to be hardcoded in update to work.
            if (gameplayClockContainer.GetTrueGameplayRate() > 0 && Time.Current >= failTime && !failSamplePlaybackInitiated)
            {
                failSamplePlaybackInitiated = true;
                failSample.Play();
            }

            if (Time.Current < failTime && failSamplePlaybackInitiated)
            {
                failSamplePlaybackInitiated = false;
                failSample.Stop();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            failSample.Stop();
            failSample.Dispose();
            track.RemoveAdjustment(AdjustableProperty.Frequency, trackFreq);
            track.RemoveAdjustment(AdjustableProperty.Volume, volumeAdjustment);
            base.Dispose(isDisposing);
        }
    }
}
