using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using System;

namespace osu.Game.Screens.Menu
{
    public class MenuSideFlashes : BeatSyncedContainer
    {
        public override bool HandleInput => false;

        private Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private Box leftBox;
        private Box rightBox;

        private const int amplitude_dead_zone = 9000;
        private const float alpha_multiplier = (short.MaxValue - amplitude_dead_zone) / 0.55f;
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
                    ColourInfo = ColourInfo.GradientHorizontal(new Color4(255, 255, 255, box_max_alpha), Color4.Transparent),
                },
                rightBox = new Box
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = 300,
                    Alpha = 0,
                    BlendingMode = BlendingMode.Additive,
                    ColourInfo = ColourInfo.GradientHorizontal(Color4.Transparent, new Color4(255, 255, 255, box_max_alpha)),
                }
            };
        }

        protected override void OnNewBeat(int newBeat, double beatLength, TimeSignatures timeSignature, bool kiai)
        {
            if (!beatmap?.Value?.Track?.IsRunning ?? false)
            {
                leftBox.FadeOut(50);
                rightBox.FadeOut(50);
            }
            else if (newBeat >= 0)
            {
                short[] lev = beatmap.Value.Track.ChannelPeakAmplitudes;
                bool nextIsLeft = newBeat % 2 == 0;
                if (kiai ? nextIsLeft : newBeat % (int)timeSignature == 0)
                {
                    leftBox.ClearTransforms();
                    leftBox.FadeTo(Math.Max(0, (lev[0] - amplitude_dead_zone) / alpha_multiplier), 65);
                    using (leftBox.BeginDelayedSequence(box_fade_in_time))
                        leftBox.FadeOut(beatLength, EasingTypes.In);
                    leftBox.DelayReset();
                }
                if (kiai ? !nextIsLeft : newBeat % (int)timeSignature == 0)
                {
                    rightBox.ClearTransforms();
                    rightBox.FadeTo(Math.Max(0, (lev[1] - amplitude_dead_zone) / alpha_multiplier), 65);
                    using (rightBox.BeginDelayedSequence(box_fade_in_time))
                        rightBox.FadeOut(beatLength, EasingTypes.In);
                    rightBox.DelayReset();
                }
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            beatmap = game.Beatmap;
        }
    }
}