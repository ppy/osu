// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// Mod that randomises the positions of the <see cref="HitObject"/>s
    /// </summary>
    public class OsuModRandom : ModRandom, IApplicableToBeatmap
    {
        public override string Description => "It never gets boring!";
        public override bool Ranked => false;

        // The relative distance to the edge of the playfield before objects' positions should start to "turn around" and curve towards the middle.
        // The closer the hit objects draw to the border, the sharper the turn
        private const float playfield_edge_ratio = 0.375f;

        private static readonly float border_distance_x = OsuPlayfield.BASE_SIZE.X * playfield_edge_ratio;
        private static readonly float border_distance_y = OsuPlayfield.BASE_SIZE.Y * playfield_edge_ratio;

        private static readonly Vector2 playfield_middle = Vector2.Divide(OsuPlayfield.BASE_SIZE, 2);

        private static readonly float playfield_diagonal = OsuPlayfield.BASE_SIZE.LengthFast;

        [SettingSource("Seed", "Use a custom seed instead of a random one", SettingControlType = typeof(OsuModRandomSettingsControl))]
        public Bindable<int?> Seed { get; } = new Bindable<int?>
        {
            Default = null,
            Value = null
        };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (!(beatmap is OsuBeatmap osuBeatmap))
                return;

            var hitObjects = osuBeatmap.HitObjects;

            Seed.Value ??= RNG.Next();

            var rng = new Random((int)Seed.Value);

            var prevObjectInfo = new RandomObjectInfo
            {
                PositionOriginal = hitObjects[0].Position,
                EndPositionOriginal = hitObjects[0].EndPosition,
                PositionRandomised = hitObjects[0].Position,
                EndPositionRandomised = hitObjects[0].EndPosition
            };

            float rateOfChangeMultiplier = 0;

            for (int i = 0; i < hitObjects.Count; i++)
            {
                var hitObject = hitObjects[i];

                var currentObjectInfo = new RandomObjectInfo(hitObject);

                if (i == 0)
                    prevObjectInfo = currentObjectInfo;

                // rateOfChangeMultiplier only changes every i iterations to prevent shaky-line-shaped streams
                if (i % 3 == 0)
                    rateOfChangeMultiplier = (float)rng.NextDouble() * 2 - 1;

                var distanceToPrev = Vector2.Distance(prevObjectInfo.EndPositionOriginal, currentObjectInfo.PositionOriginal);

                switch (hitObject)
                {
                    case HitCircle circle:
                        applyRandomisation(
                            rateOfChangeMultiplier,
                            prevObjectInfo,
                            distanceToPrev,
                            ref currentObjectInfo
                        );

                        circle.Position = currentObjectInfo.PositionRandomised;
                        currentObjectInfo.EndPositionRandomised = currentObjectInfo.PositionRandomised;

                        break;

                    case Slider slider:
                        currentObjectInfo.EndPositionOriginal = slider.EndPosition;

                        currentObjectInfo.EndPositionOriginal = slider.TailCircle.Position;

                        applyRandomisation(
                            rateOfChangeMultiplier,
                            prevObjectInfo,
                            distanceToPrev,
                            ref currentObjectInfo
                        );

                        slider.Position = currentObjectInfo.PositionRandomised;
                        currentObjectInfo.EndPositionRandomised = slider.TailCircle.Position;

                        moveSliderIntoPlayfield(ref slider, ref currentObjectInfo);

                        var sliderShift = Vector2.Subtract(slider.Position, currentObjectInfo.PositionOriginal);

                        foreach (var tick in slider.NestedHitObjects.OfType<SliderTick>())
                            tick.Position = Vector2.Add(tick.Position, sliderShift);

                        break;
                }

                prevObjectInfo = currentObjectInfo;
            }
        }

        /// <summary>
        /// Returns the final position of the hit object
        /// </summary>
        /// <returns>Final position of the hit object</returns>
        private void applyRandomisation(float rateOfChangeMultiplier, RandomObjectInfo prevObjectInfo, float distanceToPrev, ref RandomObjectInfo currentObjectInfo)
        {
            // The max. angle (relative to the angle of the vector pointing from the 2nd last to the last hit object)
            // is proportional to the distance between the last and the current hit object
            // to allow jumps and prevent too sharp turns during streams.
            var randomAngleRad = rateOfChangeMultiplier * 2 * Math.PI * distanceToPrev / playfield_diagonal;

            currentObjectInfo.AngleRad = (float)randomAngleRad + prevObjectInfo.AngleRad;
            if (currentObjectInfo.AngleRad < 0)
                currentObjectInfo.AngleRad += 2 * (float)Math.PI;

            var posRelativeToPrev = new Vector2(
                distanceToPrev * (float)Math.Cos(currentObjectInfo.AngleRad),
                distanceToPrev * (float)Math.Sin(currentObjectInfo.AngleRad)
            );

            posRelativeToPrev = getRotatedVector(prevObjectInfo.EndPositionRandomised, posRelativeToPrev);

            currentObjectInfo.AngleRad = (float)Math.Atan2(posRelativeToPrev.Y, posRelativeToPrev.X);

            var position = Vector2.Add(prevObjectInfo.EndPositionRandomised, posRelativeToPrev);

            // Move hit objects back into the playfield if they are outside of it,
            // which would sometimes happen during big jumps otherwise.
            position.X = MathHelper.Clamp(position.X, 0, OsuPlayfield.BASE_SIZE.X);
            position.Y = MathHelper.Clamp(position.Y, 0, OsuPlayfield.BASE_SIZE.Y);

            currentObjectInfo.PositionRandomised = position;
        }

        private void moveSliderIntoPlayfield(ref Slider slider, ref RandomObjectInfo currentObjectInfo)
        {
            foreach (var controlPoint in slider.Path.ControlPoints)
            {
                // Position of controlPoint relative to slider.Position
                var pos = controlPoint.Position.Value;

                var playfieldSize = OsuPlayfield.BASE_SIZE;

                if (pos.X + slider.Position.X < 0)
                    slider.Position = new Vector2(-pos.X, slider.Position.Y);
                else if (pos.X + slider.Position.X > playfieldSize.X)
                    slider.Position = new Vector2(playfieldSize.X - pos.X, slider.Position.Y);

                if (pos.Y + slider.Position.Y < 0)
                    slider.Position = new Vector2(slider.Position.X, -pos.Y);
                else if (pos.Y + slider.Position.Y > playfieldSize.Y)
                    slider.Position = new Vector2(slider.Position.X, playfieldSize.Y - pos.Y);
            }

            currentObjectInfo.EndPositionRandomised = slider.TailCircle.Position;
        }

        /// <summary>
        /// Determines the position of the current hit object relative to the previous one.
        /// </summary>
        /// <returns>The position of the current hit object relative to the previous one</returns>
        private Vector2 getRotatedVector(Vector2 prevPosChanged, Vector2 posRelativeToPrev)
        {
            var relativeRotationDistance = 0f;

            if (prevPosChanged.X < playfield_middle.X)
            {
                relativeRotationDistance = Math.Max(
                    (border_distance_x - prevPosChanged.X) / border_distance_x,
                    relativeRotationDistance
                );
            }
            else
            {
                relativeRotationDistance = Math.Max(
                    (prevPosChanged.X - (OsuPlayfield.BASE_SIZE.X - border_distance_x)) / border_distance_x,
                    relativeRotationDistance
                );
            }

            if (prevPosChanged.Y < playfield_middle.Y)
            {
                relativeRotationDistance = Math.Max(
                    (border_distance_y - prevPosChanged.Y) / border_distance_y,
                    relativeRotationDistance
                );
            }
            else
            {
                relativeRotationDistance = Math.Max(
                    (prevPosChanged.Y - (OsuPlayfield.BASE_SIZE.Y - border_distance_y)) / border_distance_y,
                    relativeRotationDistance
                );
            }

            return rotateVectorTowardsVector(posRelativeToPrev, playfield_middle - prevPosChanged, relativeRotationDistance / 2);
        }

        /// <summary>
        /// Rotates vector "initial" towards vector "destinantion"
        /// </summary>
        /// <param name="initial">Vector to rotate to "destination"</param>
        /// <param name="destination">Vector "initial" should be rotated to</param>
        /// <param name="relativeDistance">The angle the vector should be rotated relative to the difference between the angles of the the two vectors.</param>
        /// <returns>Resulting vector</returns>
        private Vector2 rotateVectorTowardsVector(Vector2 initial, Vector2 destination, float relativeDistance)
        {
            var initialAngleRad = Math.Atan2(initial.Y, initial.X);
            var destAngleRad = Math.Atan2(destination.Y, destination.X);

            var diff = destAngleRad - initialAngleRad;

            while (diff < -Math.PI)
            {
                diff += 2 * Math.PI;
            }

            while (diff > Math.PI)
            {
                diff -= 2 * Math.PI;
            }

            var finalAngleRad = initialAngleRad + relativeDistance * diff;

            return new Vector2(
                initial.Length * (float)Math.Cos(finalAngleRad),
                initial.Length * (float)Math.Sin(finalAngleRad)
            );
        }

        private struct RandomObjectInfo
        {
            internal float AngleRad { get; set; }

            internal Vector2 PositionOriginal { get; set; }
            internal Vector2 PositionRandomised { get; set; }

            internal Vector2 EndPositionOriginal { get; set; }
            internal Vector2 EndPositionRandomised { get; set; }

            public RandomObjectInfo(OsuHitObject hitObject)
            {
                PositionRandomised = PositionOriginal = hitObject.Position;
                EndPositionRandomised = EndPositionOriginal = hitObject.EndPosition;
                AngleRad = 0;
            }
        }

        public class OsuModRandomSettingsControl : SettingsItem<int?>
        {
            protected override Drawable CreateControl() => new SeedControl
            {
                RelativeSizeAxes = Axes.X,
                Margin = new MarginPadding { Top = 5 }
            };

            private sealed class SeedControl : CompositeDrawable, IHasCurrentValue<int?>
            {
                private readonly BindableWithCurrent<int?> current = new BindableWithCurrent<int?>();

                public Bindable<int?> Current
                {
                    get => current;
                    set
                    {
                        current.Current = value;
                        seedNumberBox.Text = value.Value.ToString();
                    }
                }

                private readonly OsuNumberBox seedNumberBox;

                public SeedControl()
                {
                    AutoSizeAxes = Axes.Y;

                    InternalChildren = new[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, 2),
                                new Dimension(GridSizeMode.Relative, 0.25f)
                            },
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    seedNumberBox = new OsuNumberBox
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        CommitOnFocusLost = true
                                    }
                                }
                            }
                        }
                    };

                    seedNumberBox.Current.BindValueChanged(e =>
                    {
                        int? value = null;

                        if (int.TryParse(e.NewValue, out var intVal))
                            value = intVal;

                        current.Value = value;
                    });
                }

                protected override void Update()
                {
                    if (current.Value == null)
                        seedNumberBox.Text = current.Current.Value.ToString();
                }
            }
        }
    }
}
