// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
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

        private static readonly Bindable<int> seed = new Bindable<int>
        {
            Default = -1
        };

        private static readonly BindableBool random_seed = new BindableBool
        {
            Value = true,
            Default = true
        };

        [SettingSource("Random seed", "Generate a random seed for the beatmap generation")]
        public BindableBool RandomSeed => random_seed;

        [SettingSource("Seed", "Seed for the random beatmap generation", SettingControlType = typeof(OsuModRandomSettingsControl))]
        public Bindable<int> Seed => seed;

        internal static bool CustomSeedDisabled => random_seed.Value;

        public OsuModRandom()
        {
            if (seed.Default != -1)
                return;

            var random = RNG.Next();
            seed.Value = random;
            seed.Default = random;
            seed.BindValueChanged(e => seed.Default = e.NewValue);
        }

        public void ApplyToBeatmap(IBeatmap iBeatmap)
        {
            if (!(iBeatmap is OsuBeatmap beatmap))
                return;

            if (RandomSeed.Value)
                seed.Value = RNG.Next();

            var rng = new Random(seed.Value);

            // Absolute angle
            float prevAngleRad = 0;

            // Absolute positions
            Vector2 prevPosUnchanged = beatmap.HitObjects[0].Position;
            Vector2 prevPosChanged = beatmap.HitObjects[0].Position;

            // rateOfChangeMultiplier changes every i iterations to prevent shaky-line-shaped streams
            byte i = 3;
            float rateOfChangeMultiplier = 0;

            foreach (var beatmapHitObject in beatmap.HitObjects)
            {
                if (!(beatmapHitObject is OsuHitObject hitObject))
                    return;

                // posUnchanged: position from the original beatmap (not randomised)
                var posUnchanged = hitObject.EndPosition;
                var posChanged = Vector2.Zero;

                // Angle of the vector pointing from the last to the current hit object
                float angleRad = 0;

                if (i >= 3)
                {
                    i = 0;
                    rateOfChangeMultiplier = (float)rng.NextDouble() * 2 - 1;
                }

                if (hitObject is HitCircle circle)
                {
                    var distanceToPrev = Vector2.Distance(posUnchanged, prevPosUnchanged);

                    circle.Position = posChanged = getRandomisedPosition(
                        rateOfChangeMultiplier,
                        prevPosChanged,
                        prevAngleRad,
                        distanceToPrev,
                        out angleRad
                    );
                }

                // TODO: Implement slider position randomisation

                prevAngleRad = angleRad;
                prevPosUnchanged = posUnchanged;
                prevPosChanged = posChanged;
                i++;
            }
        }

        /// <summary>
        /// Returns the final position of the hit object
        /// </summary>
        /// <returns>Final position of the hit object</returns>
        private Vector2 getRandomisedPosition(
            float rateOfChangeMultiplier,
            Vector2 prevPosChanged,
            float prevAngleRad,
            float distanceToPrev,
            out float newAngle)
        {
            // The max. angle (relative to the angle of the vector pointing from the 2nd last to the last hit object)
            // is proportional to the distance between the last and the current hit object
            // to allow jumps and prevent too sharp turns during streams.
            var maxDistance = OsuPlayfield.BASE_SIZE.LengthFast;
            var randomAngleRad = rateOfChangeMultiplier * 2 * Math.PI * distanceToPrev / maxDistance;

            newAngle = (float)randomAngleRad + prevAngleRad;
            if (newAngle < 0)
                newAngle += 2 * (float)Math.PI;

            var posRelativeToPrev = new Vector2(
                distanceToPrev * (float)Math.Cos(newAngle),
                distanceToPrev * (float)Math.Sin(newAngle)
            );

            posRelativeToPrev = getRotatedVector(prevPosChanged, posRelativeToPrev);

            newAngle = (float)Math.Atan2(posRelativeToPrev.Y, posRelativeToPrev.X);
            var position = Vector2.Add(prevPosChanged, posRelativeToPrev);

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

            return position;
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
                relativeRotationDistance
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

            // Divide by 2 to limit the max. angle to 90°
            // (90° is enough to prevent the hit objects from leaving the playfield)
            relativeDistance /= 2;

            var diff = destAngleRad - initialAngleRad;

            while (diff < -Math.PI)
            {
                diff += 2 * Math.PI;
            }

            while (diff > Math.PI)
            {
                diff -= 2 * Math.PI;
            }

            var finalAngle = 0d;

            if (diff > 0)
            {
                finalAngle = initialAngleRad + relativeDistance * diff;
            }
            else if (diff < 0)
            {
                finalAngle = initialAngleRad + relativeDistance * diff;
            }

            return new Vector2(
                initial.Length * (float)Math.Cos(finalAngle),
                initial.Length * (float)Math.Sin(finalAngle)
            );
        }
    }

    public class OsuModRandomSettingsControl : SettingsItem<int>
    {
        [Resolved]
        private static GameHost host { get; set; }

        [BackgroundDependencyLoader]
        private void load(GameHost gameHost) => host = gameHost;

        protected override Drawable CreateControl() => new SeedControl
        {
            RelativeSizeAxes = Axes.X,
            Margin = new MarginPadding { Top = 5 }
        };

        private sealed class SeedControl : CompositeDrawable, IHasCurrentValue<int>
        {
            private readonly BindableWithCurrent<int> current = new BindableWithCurrent<int>();

            public Bindable<int> Current
            {
                get => current;
                set => Scheduler.Add(() => current.Current = value);
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
                                },
                                null,
                                new TriangleButton
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Height = 1,
                                    Text = "Copy",
                                    Action = copySeedToClipboard
                                }
                            }
                        }
                    }
                };

                seedNumberBox.Current.BindValueChanged(onTextBoxValueChanged);
            }

            private void onTextBoxValueChanged(ValueChangedEvent<string> e)
            {
                string seed = e.NewValue;

                while (!string.IsNullOrEmpty(seed) && !int.TryParse(seed, out _))
                    seed = seed[..^1];

                if (!int.TryParse(seed, out var intVal))
                    intVal = 0;

                current.Value = intVal;
            }

            private void copySeedToClipboard() => host.GetClipboard().SetText(seedNumberBox.Text);

            protected override void Update()
            {
                seedNumberBox.ReadOnly = OsuModRandom.CustomSeedDisabled;

                if (seedNumberBox.HasFocus)
                    return;

                seedNumberBox.Text = current.Current.Value.ToString();
            }
        }
    }
}
