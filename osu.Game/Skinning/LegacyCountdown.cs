// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning
{
    public class LegacyCountdown : CountdownOverlay
    {
        protected override void LoadComplete()
        {
            IBeatmap beatmap = GameplayState.Beatmap;

            double firstObject = beatmap.HitObjects[0].StartTime;
            double offset = beatmap.ControlPointInfo.TimingPointAt(firstObject).Time;
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
            bool rulesetAllowsCountdown = GameplayState.Ruleset.ShortName is "osu" or "fruits";

            if (useCountdown && rulesetAllowsCountdown)
            {
                // ? these variables were already defined in the stable code, but im not sure what they do
                // set SkipBoundary to goTime - 6 * beatLength;
                // set CountdownTime to goTime - 3 * beatLength;

                Sprite ready = createSprite(Skin.GetTexture(@"ready"), 1); // (placeholder texture)
                Sprite count3 = createSprite(Skin.GetTexture(@"count3"));
                Sprite count2 = createSprite(Skin.GetTexture(@"count2"));
                Sprite count1 = createSprite(Skin.GetTexture(@"count1"));
                Sprite go = createSprite(Skin.GetTexture(@"go"));

                AddInternal(ready);
                AddInternal(count3);
                AddInternal(count2);
                AddInternal(count1);
                AddInternal(go);

                scheduleAudio(goTime, beatLength);

                // stable uses start/end time instead of durations,
                // if the start time is (goTime - x * beatLength) and end time (goTime - y * beatLength), then the duration is ((y-x)*beatLength)

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
                // todo: some arrow thing? see stable code below
                /*
                int loopStartTime = hitObjectManager.hitObjects[0].StartTime - hitObjectManager.PreEmpt - 900;
                int loopCount = ARROW_LOOP_COUNT + Math.Min(2, hitObjectManager.PreEmpt / 200);
                ApplyArrowTransformations(loopCount, loopStartTime);
                */
            }
        }

        private void scheduleAudio(double goTime, double beatLength)
        {
            // the "readys" sample will only play if added to the skin, it does not have a default
            ISample sampleReady = Skin.GetSample(new SampleInfo(@"readys"));

            // if "count3" does not exist, the "count" sample will be used for all counts
            // if "count3s" does exist, "count2s", "count1s" and "gos" will be used and there will not be a fallback to "count"
            ISample sampleCount3 = Skin.GetSample(new SampleInfo(@"count3s"));
            ISample sampleCount2;
            ISample sampleCount1;
            ISample sampleGo;

            if (sampleCount3 == null)
            {
                ISample count = Skin.GetSample(new SampleInfo(@"count"));
                sampleCount3 = count;
                sampleCount2 = count;
                sampleCount1 = count;
                sampleGo = count;
            }
            else
            {
                sampleCount2 = Skin.GetSample(new SampleInfo(@"count2s"));
                sampleCount1 = Skin.GetSample(new SampleInfo(@"count1s"));
                sampleGo = Skin.GetSample(new SampleInfo(@"gos"));
            }

            Scheduler.AddDelayed(() => sampleReady?.Play(), goTime - 5.9 * beatLength);
            Scheduler.AddDelayed(() => sampleCount3?.Play(), goTime - 3 * beatLength);
            Scheduler.AddDelayed(() => sampleCount2?.Play(), goTime - 2 * beatLength);
            Scheduler.AddDelayed(() => sampleCount1?.Play(), goTime - 1 * beatLength);
            Scheduler.AddDelayed(() => sampleGo?.Play(), goTime);
        }

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
