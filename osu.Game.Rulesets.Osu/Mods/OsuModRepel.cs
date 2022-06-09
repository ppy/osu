// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Utils;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModRepel : OsuEaseHitObjectPositionsMod
    {
        public override string Name => "Repel";
        public override string Acronym => "RP";
        public override IconUsage? Icon => FontAwesome.Solid.ExpandArrowsAlt;
        public override string Description => "Run away!";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModMagnetised)).ToArray();

        [SettingSource("Repulsion strength", "How strong the repulsion is.", 0)]
        public override BindableFloat EasementStrength { get; } = new BindableFloat(0.6f)
        {
            Precision = 0.05f,
            MinValue = 0.05f,
            MaxValue = 1.0f,
        };

        protected override float EasementStrengthMultiplier => 0.8f;

        protected override Vector2 DestinationVector
        {
            get
            {
                float x = Math.Clamp(2 * WorkingHitObject.X - CursorPosition.X, 0, OsuPlayfield.BASE_SIZE.X);
                float y = Math.Clamp(2 * WorkingHitObject.Y - CursorPosition.Y, 0, OsuPlayfield.BASE_SIZE.Y);

                if (WorkingHitObject.HitObject is Slider slider)
                {
                    var possibleMovementBounds = OsuHitObjectGenerationUtils.CalculatePossibleMovementBounds(slider);

                    x = possibleMovementBounds.Width < 0
                        ? x
                        : Math.Clamp(x, possibleMovementBounds.Left, possibleMovementBounds.Right);

                    y = possibleMovementBounds.Height < 0
                        ? y
                        : Math.Clamp(y, possibleMovementBounds.Top, possibleMovementBounds.Bottom);
                }

                return new Vector2(x, y);
            }
        }
    }
}
