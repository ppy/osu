// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using System;

namespace osu.Game.Screens.Menu
{
    public class MenuSideFlashes : BeatSyncedContainer
    {
        public override bool HandleNonPositionalInput => false;
        public override bool HandlePositionalInput => false;

        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private Box leftBox;
        private Box rightBox;

        private const float amplitude_dead_zone = 0.25f;
        private const float alpha_multiplier = (1 - amplitude_dead_zone) / 0.55f;
        private const float kiai_multiplier = (1 - amplitude_dead_zone * 0.95f) / 0.8f;

        private const int box_max_alpha = 200;
        private const double box_fade_in_time = 65;
        private const int box_width = 200;

        public MenuSideFlashes()
        {
            EarlyActivationMilliseconds = box_fade_in_time;

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap beatmap, OsuColour colours)
        {
            this.beatmap.BindTo(beatmap);

            // linear colour looks better in this case, so let's use it for now.
            Color4 gradientDark = colours.Blue.Opacity(0).ToLinear();
            Color4 gradientLight = colours.Blue.Opacity(0.6f).ToLinear();

            Children = new Drawable[]
            {
                leftBox = new Box
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = box_width * 2,
                    // align off-screen to make sure our edges don't become visible during parallax.
                    X = -box_width,
                    Alpha = 0,
                    Blending = BlendingMode.Additive,
                    Colour = ColourInfo.GradientHorizontal(gradientLight, gradientDark)
                },
                rightBox = new Box
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = box_width * 2,
                    X = box_width,
                    Alpha = 0,
                    Blending = BlendingMode.Additive,
                    Colour = ColourInfo.GradientHorizontal(gradientDark, gradientLight)
                }
            };
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            if (beatIndex < 0)
                return;

            if (effectPoint.KiaiMode ? beatIndex % 2 == 0 : beatIndex % (int)timingPoint.TimeSignature == 0)
                flash(leftBox, timingPoint.BeatLength, effectPoint.KiaiMode, amplitudes);
            if (effectPoint.KiaiMode ? beatIndex % 2 == 1 : beatIndex % (int)timingPoint.TimeSignature == 0)
                flash(rightBox, timingPoint.BeatLength, effectPoint.KiaiMode, amplitudes);
        }

        private void flash(Drawable d, double beatLength, bool kiai, TrackAmplitudes amplitudes)
        {
            d.FadeTo(Math.Max(0, ((d.Equals(leftBox) ? amplitudes.LeftChannel : amplitudes.RightChannel) - amplitude_dead_zone) / (kiai ? kiai_multiplier : alpha_multiplier)), box_fade_in_time)
             .Then()
             .FadeOut(beatLength, Easing.In);
        }
    }
}
