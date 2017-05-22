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

        private readonly Box leftBox;
        private readonly Box rightBox;

        private const float amplitude_dead_zone = 0.25f;
        private const float alpha_multiplier = (1 - amplitude_dead_zone) / 0.55f;
        private const float kiai_multiplier = (1 - amplitude_dead_zone * 0.95f) / 0.8f;
        private const int box_max_alpha = 200;
        private const double box_fade_in_time = 65;

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
                    Width = 300,
                    Alpha = 0,
                    BlendingMode = BlendingMode.Additive,
                    ColourInfo = ColourInfo.GradientHorizontal(new Color4(255, 255, 255, box_max_alpha), Color4.Black.Opacity(0)),
                },
                rightBox = new Box
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = 300,
                    Alpha = 0,
                    BlendingMode = BlendingMode.Additive,
                    ColourInfo = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0), new Color4(255, 255, 255, box_max_alpha)),
                }
            };
        }

        protected override void OnNewBeat(int newBeat, double beatLength, TimeSignatures timeSignature, bool kiai)
        {
            if (newBeat < 0)
                return;
            TrackAmplitudes amp = beatmap.Value.Track.CurrentAmplitudes;
            if (newBeat % (kiai ? 2 : (int)timeSignature) == 0)
            {
                leftBox.ClearTransforms();
                leftBox.FadeTo(Math.Max(0, (amp.LeftChannel - amplitude_dead_zone) / (kiai ? kiai_multiplier : alpha_multiplier)), 65);
                using (leftBox.BeginDelayedSequence(box_fade_in_time))
                    leftBox.FadeOut(beatLength, EasingTypes.In);
                leftBox.DelayReset();
            }
            if (kiai ? newBeat % 2 == 1 : newBeat % (int)timeSignature == 0)
            {
                rightBox.ClearTransforms();
                rightBox.FadeTo(Math.Max(0, (amp.RightChannel - amplitude_dead_zone) / (kiai ? kiai_multiplier : alpha_multiplier)), 65);
                using (rightBox.BeginDelayedSequence(box_fade_in_time))
                    rightBox.FadeOut(beatLength, EasingTypes.In);
                rightBox.DelayReset();
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            beatmap.BindTo(game.Beatmap);
        }
    }
}