// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using System;
using osu.Game.Graphics;
using TrackAmplitudes = osu.Framework.Audio.Track.Track.TrackAmplitudes;

namespace osu.Game.Screens.Menu
{
    public class MenuSideFlashes : BeatSyncedContainer
    {
        public override bool HandleInput => false;

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private readonly Box leftBox;
        private readonly Box rightBox;

        private const float amplitude_dead_zone = 0.25f;
        private const float alpha_multiplier = (1 - amplitude_dead_zone) / 0.55f;
        private const float kiai_multiplier = (1 - amplitude_dead_zone * 0.95f) / 0.8f;

        private const int box_max_alpha = 200;
        private const double box_fade_in_time = 65;
        private const int box_width = 200;

        public MenuSideFlashes()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Children = new Drawable[]
            {
                leftBox = new Box
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = box_width,
                    Alpha = 0,
                    BlendingMode = BlendingMode.Additive,
                },
                rightBox = new Box
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = box_width,
                    Alpha = 0,
                    BlendingMode = BlendingMode.Additive,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuColour colours)
        {
            beatmap.BindTo(game.Beatmap);

            // linear colour looks better in this case, so let's use it for now.
            Color4 gradientDark = colours.Blue.Opacity(0).ToLinear();
            Color4 gradientLight = colours.Blue.Opacity(0.3f).ToLinear();

            leftBox.ColourInfo = ColourInfo.GradientHorizontal(gradientLight, gradientDark);
            rightBox.ColourInfo = ColourInfo.GradientHorizontal(gradientDark, gradientLight);
        }

        protected override void OnNewBeat(int newBeat, double beatLength, TimeSignatures timeSignature, bool kiai)
        {
            if (newBeat < 0)
                return;

            if (kiai ? newBeat % 2 == 0 : newBeat % (int)timeSignature == 0)
                flash(leftBox, beatLength, kiai);
            if (kiai ? newBeat % 2 == 1 : newBeat % (int)timeSignature == 0)
                flash(rightBox, beatLength, kiai);
        }

        private void flash(Drawable d, double beatLength, bool kiai)
        {
            TrackAmplitudes amp = beatmap.Value.Track.CurrentAmplitudes;

            d.FadeTo(Math.Max(0, ((d.Equals(leftBox) ? amp.LeftChannel : amp.RightChannel) - amplitude_dead_zone) / (kiai ? kiai_multiplier : alpha_multiplier)), box_fade_in_time);
            using (d.BeginDelayedSequence(box_fade_in_time))
                d.FadeOut(beatLength, EasingTypes.In);
        }
    }
}
