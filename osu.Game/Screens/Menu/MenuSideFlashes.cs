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
using TrackAmplitudes = osu.Framework.Audio.Track.Track.TrackAmplitudes;

namespace osu.Game.Screens.Menu
{
    public class MenuSideFlashes : BeatSyncedContainer
    {
        public override bool HandleInput => false;

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private static readonly ColourInfo gradient_white_to_transparent_black = ColourInfo.GradientHorizontal(new Color4(255, 255, 255, box_max_alpha), Color4.Black.Opacity(0));
        private static readonly ColourInfo gradient_transparent_black_to_white = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0), new Color4(255, 255, 255, box_max_alpha));

        private readonly Box leftBox;
        private readonly Box rightBox;

        private const float amplitude_dead_zone = 0.25f;
        private const float alpha_multiplier = (1 - amplitude_dead_zone) / 0.55f;
        private const float kiai_multiplier = (1 - amplitude_dead_zone * 0.95f) / 0.8f;
        private const int box_max_alpha = 200;
        private const double box_fade_in_time = 65;
        private const int box_width = 300;

        public MenuSideFlashes()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            BlendingMode = BlendingMode.Additive;
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
                    ColourInfo = gradient_white_to_transparent_black,
                },
                rightBox = new Box
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = box_width,
                    Alpha = 0,
                    BlendingMode = BlendingMode.Additive,
                    ColourInfo = gradient_transparent_black_to_white,
                }
            };
        }

        private bool kiai;
        private double beatLength;

        protected override void OnNewBeat(int newBeat, double beatLength, TimeSignatures timeSignature, bool kiai)
        {
            if (newBeat < 0)
                return;

            this.kiai = kiai;
            this.beatLength = beatLength;

            if (kiai ? newBeat % 2 == 0 : newBeat % (int)timeSignature == 0)
                flash(leftBox);
            if (kiai ? newBeat % 2 == 1 : newBeat % (int)timeSignature == 0)
                flash(rightBox);
        }

        private void flash(Drawable d)
        {
            TrackAmplitudes amp = beatmap.Value.Track.CurrentAmplitudes;
            d.FadeTo(Math.Max(0, ((d.Equals(leftBox) ? amp.LeftChannel : amp.RightChannel) - amplitude_dead_zone) / (kiai ? kiai_multiplier : alpha_multiplier)), box_fade_in_time);
            using (d.BeginDelayedSequence(box_fade_in_time))
                d.FadeOut(beatLength, EasingTypes.In);
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            beatmap.BindTo(game.Beatmap);
        }
    }
}