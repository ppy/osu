// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public partial class AudioOffsetCalibrationBeatDisplay : CompositeDrawable
    {
        private readonly FramedBeatmapClock trackClock;
        private readonly IBindable<double> offset;
        private readonly CircularContainer marker;
        private readonly Circle markerBackground;
        private readonly OsuSpriteText beatNumber;
        private readonly Box target;
        private readonly Container tapMarks;
        private readonly AudioOffsetCalibrationVisualisation? gameplay;
        private readonly double beatLength;
        private OsuColour colours = null!;

        public double ReferenceTime => trackClock.CurrentTime + (offset.Value + FramedBeatmapClock.GetPlatformOffset(audio.UseExperimentalWasapi.Value)) * trackClock.Rate;

        public Action? Tapped { get; set; }

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        public AudioOffsetCalibrationBeatDisplay(IAdjustableClock track, IBindable<double> offset, double beatLength = AudioOffsetCalibrationTrackStore.BEAT_LENGTH,
                                                 Ruleset? ruleset = null)
        {
            this.offset = offset.GetBoundCopy();
            this.beatLength = beatLength;
            gameplay = ruleset?.CreateAudioOffsetCalibrationVisualisation();

            RelativeSizeAxes = Axes.X;
            Height = gameplay == null ? 65 : 140;
            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                // Use the same interpolation as gameplay, without the current song's local offset or mods.
                trackClock = new FramedBeatmapClock(false, false, track),
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(20, 16, 24, 255),
                },
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = 2,
                    Alpha = 0.3f,
                },
                target = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(4, 45),
                },
                marker = new CircularContainer
                {
                    Name = "Beat marker",
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.X,
                    Size = new Vector2(28),
                    Masking = true,
                    Children = new Drawable[]
                    {
                        markerBackground = new Circle { RelativeSizeAxes = Axes.Both },
                        beatNumber = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Default.With(size: 18, weight: FontWeight.Bold),
                            Colour = Color4.Black,
                        },
                    },
                },
                tapMarks = new Container { RelativeSizeAxes = Axes.Both },
                new Box
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(2, 20),
                    Y = -3,
                    Alpha = 0.5f,
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(6, -3),
                    Font = OsuFont.Default.With(size: 12),
                    Text = AudioSettingsStrings.CalibrationEarly,
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Position = new Vector2(-6, -3),
                    Font = OsuFont.Default.With(size: 12),
                    Text = AudioSettingsStrings.CalibrationLate,
                },
            };

            if (gameplay != null)
            {
                marker.Hide();
                target.Hide();
                AddInternal(new RulesetSkinProvidingContainer(ruleset!, new Beatmap(), null) { Child = gameplay });
                gameplay.GetReferenceTime = () => ReferenceTime;
                gameplay.BeatLength = beatLength;
                gameplay.Tapped = () => Tapped?.Invoke();
            }
        }

        public void AddTap(double error)
        {
            if (tapMarks.Count >= AudioOffsetTapEstimator.MAX_TAPS)
                tapMarks.Remove(tapMarks[0], true);

            foreach (var mark in tapMarks)
                mark.Alpha = 0.35f;

            tapMarks.Add(new Box
            {
                Name = "Tap timing",
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomCentre,
                RelativePositionAxes = Axes.X,
                X = (float)(0.5 + error / beatLength),
                Y = -3,
                Size = new Vector2(3, 15),
                Colour = error < 0 ? colours.Blue : colours.Yellow,
            });
        }

        public void ClearTaps() => tapMarks.Clear();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            this.colours = colours;
            target.Colour = colours.Blue;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // Positive global offset advances gameplay relative to the audio, just as in FramedBeatmapClock.
            double phase = (ReferenceTime - AudioOffsetCalibrationTrackStore.FIRST_BEAT) / beatLength;
            int beat = ((int)Math.Floor(phase + 0.5) % 4 + 4) % 4;
            phase -= Math.Floor(phase + 0.5);

            marker.X = (float)(0.5 + phase);
            marker.Alpha = gameplay == null ? trackClock.IsRunning ? 1 : 0.3f : 0;
            markerBackground.Colour = beat == 0 ? colours.Yellow : colours.Blue;
            beatNumber.Text = (beat + 1).ToString();
        }
    }
}
