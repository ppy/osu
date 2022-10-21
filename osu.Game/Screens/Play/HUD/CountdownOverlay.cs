// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class CountdownOverlay : CompositeDrawable, ISkinnableDrawable
    {
        private readonly GameplayState gameplayState;

        public CountdownOverlay(GameplayState gameplayState)
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            this.gameplayState = gameplayState;
        }

        [BackgroundDependencyLoader]
        private void load(ISkin skin)
        {
            OnLoadComplete += _ => updateCountdown(skin);
        }

        private void updateCountdown(ISkin skin)
        {
            base.LoadComplete();

            IBeatmap beatmap = gameplayState.Beatmap;

            double firstObject = beatmap.HitObjects[0].StartTime;
            double offset = 0; // ?
            double beatLengthOriginal = beatmap.ControlPointInfo.TimingPointAt(firstObject).BeatLength;

            if (beatLengthOriginal <= 0) beatLengthOriginal = beatmap.ControlPointInfo.TimingPointAt(0).BeatLength;
            if (beatLengthOriginal <= 0) return;

            double goTime = offset - 5;
            double beatLength = beatLengthOriginal;

            //If the bpm is too fast let's double the length just because it seems sensible.
            if (beatLength <= 333)
                beatLength *= 2;

            switch (beatmap.BeatmapInfo.Countdown)
            {
                case CountdownType.DoubleSpeed:
                    beatLength /= 2;
                    break;

                case CountdownType.HalfSpeed:
                    beatLength *= 2;
                    break;

                case CountdownType.None:
                    return; // skip countdown completely

                case CountdownType.Normal:
                    break; // don't do anything special

                default:
                    throw new ArgumentOutOfRangeException($"Unknown countdown type {beatmap.BeatmapInfo.Countdown}");
            }

            // Push the countdown back by the mapper's countdown offset before any other adjustments.
            firstObject -= beatLength * beatmap.BeatmapInfo.CountdownOffset;

            // Skip back until we are at a good place
            if (goTime >= firstObject)
            {
                while (goTime > firstObject - beatLength)
                    goTime -= beatLength;
            }

            int divisor = 1;

            while (goTime < firstObject - beatLength / divisor)
            {
                goTime += beatLength;
                double beat = ((float)(goTime - offset) / beatLengthOriginal) % 4;
                if (beat > 1.5 && beat < 3.5)
                    divisor = 2;
                else
                    divisor = 1;
            }

            goTime -= beatLength;

            bool useCountdown = goTime - 4 * beatLength > 0;

            // in stable the countdown is active in osu, catch and mania, but since it falls behind the playfield in mania it's not added here
            bool rulesetAllowsCountdown = gameplayState.Ruleset.ShortName is "osu" or "catch";

            if (useCountdown && rulesetAllowsCountdown)
            {
                // ? these variables were already defined in the stable code, but im not sure what they do
                // set SkipBoundary to goTime - 6 * beatLength;
                // set CountdownTime to goTime - 3 * beatLength;

                Sprite ready = createSprite(skin.GetTexture("count1"), 1); // (placeholder texture)
                Sprite count3 = createSprite(skin.GetTexture("count3"));
                Sprite count2 = createSprite(skin.GetTexture("count2"));
                Sprite count1 = createSprite(skin.GetTexture("count1"));
                Sprite go = createSprite(skin.GetTexture("go"));

                AddInternal(ready);
                AddInternal(count3);
                AddInternal(count2);
                AddInternal(count1);
                AddInternal(go);

                // stable uses start/end time instead of durations,
                // if the start time is (goTime - 6 * beatLength) and end time (goTime - 5 * beatLength), then the duration is (6-5)*beatLength

                using (ready.BeginAbsoluteSequence(goTime - 6 * beatLength))
                {
                    ready.FadeIn(beatLength)
                         .Delay(beatLength * 2).FadeOut(beatLength).ScaleTo(1.2f, beatLength);
                }

                using (count3.BeginAbsoluteSequence(goTime - 3.2 * beatLength))
                {
                    count3.FadeIn(beatLength * 0.2f).ScaleTo(1, beatLength * 0.2f)
                          .Delay(beatLength).FadeOut(beatLength * 0.2);
                }

                using (count2.BeginAbsoluteSequence(goTime - 2.2 * beatLength))
                {
                    count2.FadeIn(beatLength * 0.2f).ScaleTo(1, beatLength * 0.2f)
                          .Delay(beatLength).FadeOut(beatLength * 0.2);
                }

                using (count1.BeginAbsoluteSequence(goTime - 1.2 * beatLength))
                {
                    count1.FadeIn(beatLength * 0.2f).ScaleTo(1, beatLength * 0.2f)
                          .Delay(beatLength).FadeOut(beatLength * 0.2);
                }

                using (BeginAbsoluteSequence(goTime - 0.6 * beatLength))
                {
                    go.ScaleTo(1, 0.8 * beatLength)
                      .Delay(0.4f * beatLength).FadeIn(0.2 * beatLength)
                      .Delay(0.5f * beatLength).FadeOut(0.3f * beatLength);
                }
            }
            else if (beatmap.HitObjects[0].StartTime > 6000)
            {
                // todo: some arrow thing?
            }
        }

        public bool UsesFixedAnchor { get; set; }

        private Sprite createSprite(Texture texture, float scale = 1.4f)
        {
            return new Sprite
            {
                Name = texture.AssetName,
                Texture = texture,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0,
                Scale = new Vector2(scale),
            };
        }
    }
}
