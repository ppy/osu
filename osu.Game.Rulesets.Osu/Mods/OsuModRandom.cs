// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        // The distances from the hit objects to the borders of the playfield they start to "turn around" and curve towards the middle.
        // The closer the hit objects draw to the border, the sharper the turn
        private const byte border_distance_x = 192;
        private const byte border_distance_y = 144;

        [SettingSource("Custom seed", "Use a custom seed instead of a random one", SettingControlType = typeof(OsuModRandomSettingsControl))]
        public Bindable<int?> CustomSeed { get; } = new Bindable<int?>
        {
            Default = null,
            Value = null
        };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (!(beatmap is OsuBeatmap osuBeatmap))
                return;

            var seed = RNG.Next();

            if (CustomSeed.Value != null)
                seed = (int)CustomSeed.Value;

            var rng = new Random(seed);

            var prevObjectInfo = new HitObjectInfo
            {
                AngleRad = 0,
                PosUnchanged = osuBeatmap.HitObjects[0].Position,
                PosChanged = osuBeatmap.HitObjects[0].Position
            };

            // rateOfChangeMultiplier changes every i iterations to prevent shaky-line-shaped streams
            byte i = 3;
            float rateOfChangeMultiplier = 0;

            foreach (var currentHitObject in osuBeatmap.HitObjects)
            {
                var currentObjectInfo = new HitObjectInfo
                {
                    AngleRad = 0,
                    PosUnchanged = currentHitObject.EndPosition,
                    PosChanged = Vector2.Zero
                };

                if (i >= 3)
                {
                    i = 0;
                    rateOfChangeMultiplier = (float)rng.NextDouble() * 2 - 1;
                }

                if (currentHitObject is HitCircle circle)
                {
                    var distanceToPrev = Vector2.Distance(currentObjectInfo.PosUnchanged, prevObjectInfo.PosUnchanged);

                    getObjectInfo(
                        rateOfChangeMultiplier,
                        prevObjectInfo,
                        distanceToPrev,
                        ref currentObjectInfo
                    );

                    circle.Position = currentObjectInfo.PosChanged;
                }

                // TODO: Implement slider position randomisation

                prevObjectInfo = currentObjectInfo;
                i++;
            }
        }

        /// <summary>
        /// Returns the final position of the hit object
        /// </summary>
        /// <returns>Final position of the hit object</returns>
        private void getObjectInfo(
            float rateOfChangeMultiplier,
            HitObjectInfo prevObjectInfo,
            float distanceToPrev,
            ref HitObjectInfo currentObjectInfo)
        {
            // The max. angle (relative to the angle of the vector pointing from the 2nd last to the last hit object)
            // is proportional to the distance between the last and the current hit object
            // to allow jumps and prevent too sharp turns during streams.
            var maxDistance = OsuPlayfield.BASE_SIZE.LengthFast;
            var randomAngleRad = rateOfChangeMultiplier * 2 * Math.PI * distanceToPrev / maxDistance;

            currentObjectInfo.AngleRad = (float)randomAngleRad + prevObjectInfo.AngleRad;
            if (currentObjectInfo.AngleRad < 0)
                currentObjectInfo.AngleRad += 2 * (float)Math.PI;

            var posRelativeToPrev = new Vector2(
                distanceToPrev * (float)Math.Cos(currentObjectInfo.AngleRad),
                distanceToPrev * (float)Math.Sin(currentObjectInfo.AngleRad)
            );

            posRelativeToPrev = getRotatedVector(prevObjectInfo.PosChanged, posRelativeToPrev);

            currentObjectInfo.AngleRad = (float)Math.Atan2(posRelativeToPrev.Y, posRelativeToPrev.X);
            var position = Vector2.Add(prevObjectInfo.PosChanged, posRelativeToPrev);

            // Move hit objects back into the playfield if they are outside of it,
            // which would sometimes happen during big jumps otherwise.
            if (position.X < 0)
                position.X = 0;
            else if (position.X > OsuPlayfield.BASE_SIZE.X)
                position.X = OsuPlayfield.BASE_SIZE.X;

            if (position.Y < 0)
                position.Y = 0;
            else if (position.Y > OsuPlayfield.BASE_SIZE.Y)
                position.Y = OsuPlayfield.BASE_SIZE.Y;

            currentObjectInfo.PosChanged = position;
        }

        /// <summary>
        /// Determines the position of the current hit object relative to the previous one.
        /// </summary>
        /// <returns>The position of the current hit object relative to the previous one</returns>
        private Vector2 getRotatedVector(Vector2 prevPosChanged, Vector2 posRelativeToPrev)
        {
            var relativeRotationDistance = 0f;
            var playfieldMiddle = Vector2.Divide(OsuPlayfield.BASE_SIZE, 2);

            if (prevPosChanged.X < playfieldMiddle.X)
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

            if (prevPosChanged.Y < playfieldMiddle.Y)
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

            return rotateVectorTowardsVector(
                posRelativeToPrev,
                Vector2.Subtract(playfieldMiddle, prevPosChanged),
                relativeRotationDistance / 2
            );
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

        private struct HitObjectInfo
        {
            internal float AngleRad { get; set; }
            internal Vector2 PosUnchanged { get; set; }
            internal Vector2 PosChanged { get; set; }
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
