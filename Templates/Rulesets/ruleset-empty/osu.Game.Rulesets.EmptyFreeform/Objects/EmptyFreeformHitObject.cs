// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.EmptyFreeform.Objects
{
    public class EmptyFreeformHitObject : HitObject, IHasPosition
    {
        protected override Judgement CreateJudgement() => new Judgement();

        public Vector2 Position { get; set; }

        public float X => Position.X;
        public float Y => Position.Y;
    }
}
