// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModBarrelRoll<TObject> : Mod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<TObject>
        where TObject : HitObject
    {
        /// <summary>
        /// The current angle of rotation being applied by this mod.
        /// Generally should be used to apply inverse rotation to elements which should not be rotated.
        /// </summary>
        protected float CurrentRotation { get; private set; }

        [SettingSource("Roll speed", "Rotations per minute")]
        public BindableNumber<double> SpinSpeed { get; } = new BindableDouble(0.5)
        {
            MinValue = 0.02,
            MaxValue = 12,
            Precision = 0.01,
        };

        [SettingSource("Direction", "The direction of rotation")]
        public Bindable<RotationDirection> Direction { get; } = new Bindable<RotationDirection>();

        public override string Name => "Barrel Roll";
        public override string Acronym => "BR";
        public override string Description => "The whole playfield is on a wheel!";
        public override double ScoreMultiplier => 1;

        public override string SettingDescription => $"{SpinSpeed.Value} rpm {Direction.Value.GetDescription().ToLowerInvariant()}";

        public void Update(Playfield playfield)
        {
            playfield.Rotation = CurrentRotation = (Direction.Value == RotationDirection.Counterclockwise ? -1 : 1) * 360 * (float)(playfield.Time.Current / 60000 * SpinSpeed.Value);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            // scale the playfield to allow all hitobjects to stay within the visible region.

            var playfieldSize = drawableRuleset.Playfield.DrawSize;
            float minSide = MathF.Min(playfieldSize.X, playfieldSize.Y);
            float maxSide = MathF.Max(playfieldSize.X, playfieldSize.Y);
            drawableRuleset.Playfield.Scale = new Vector2(minSide / maxSide);
        }
    }
}
