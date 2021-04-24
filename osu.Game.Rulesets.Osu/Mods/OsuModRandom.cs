// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModRandom : ModRandomOsu
    {
        protected override void RandomiseHitObjectPositions(IBeatmap beatmap)
        {
            var rng = new Random();

            foreach (var hitObject in beatmap.HitObjects)
            {
                if (RandomiseCirclePositions.Value && hitObject is HitCircle circle)
                {
                    circle.Position = new Vector2(
                        (float)rng.NextDouble() * OsuPlayfield.BASE_SIZE.X,
                        (float)rng.NextDouble() * OsuPlayfield.BASE_SIZE.Y
                    );
                }
                else if (RandomiseSpinnerPositions.Value && hitObject is Spinner spinner)
                {
                    spinner.Position = new Vector2(
                        (float)rng.NextDouble() * OsuPlayfield.BASE_SIZE.X,
                        (float)rng.NextDouble() * OsuPlayfield.BASE_SIZE.Y
                    );
                }
                else if (RandomiseSliderPositions.Value && hitObject is Slider slider)
                {
                    // Min. distances from the slider's position to the border to prevent the slider from being partially out of the screen
                    float minLeft = 0, minRight = 0, minTop = 0, minBottom = 0;

                    var controlPointPositions = (from position
                                                     in slider.Path.ControlPoints
                                                 select position.Position.Value).ToList();

                    controlPointPositions.Add(slider.EndPosition);
                    controlPointPositions.RemoveAt(controlPointPositions.Count - 1);

                    foreach (var position in controlPointPositions)
                    {
                        if (position.X > minRight)
                        {
                            minRight = position.X;
                        }
                        else if (-position.X > minLeft)
                        {
                            minLeft = -position.X;
                        }

                        if (position.Y > minBottom)
                        {
                            minBottom = position.Y;
                        }
                        else if (-position.Y > minTop)
                        {
                            minTop = -position.Y;
                        }
                    }

                    slider.Position = new Vector2(
                        (float)rng.NextDouble() * (OsuPlayfield.BASE_SIZE.X - minLeft - minRight) + minLeft,
                        (float)rng.NextDouble() * (OsuPlayfield.BASE_SIZE.Y - minTop - minBottom) + minTop
                    );
                }
            }
        }
    }
}
