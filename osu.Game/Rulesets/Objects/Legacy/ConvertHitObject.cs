// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Objects.Legacy
{
    /// <summary>
    /// Represents a legacy hit object.
    /// </summary>
    /// <remarks>
    /// Only used for parsing beatmaps and not gameplay.
    /// </remarks>
    internal abstract class ConvertHitObject : HitObject, IHasCombo, IHasPosition, IHasLegacyHitObjectType
    {
        public bool NewCombo { get; set; }

        public int ComboOffset { get; set; }

        public float X
        {
            get => Position.X;
            set => Position = new Vector2(value, Position.Y);
        }

        public float Y
        {
            get => Position.Y;
            set => Position = new Vector2(Position.X, value);
        }

        public Vector2 Position { get; set; }

        public LegacyHitObjectType LegacyType { get; set; } = LegacyHitObjectType.Circle;

        public override Judgement CreateJudgement() => new IgnoreJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
