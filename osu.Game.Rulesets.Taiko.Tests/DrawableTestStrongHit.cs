// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public partial class DrawableTestStrongHit : DrawableTestHit
    {
        private readonly bool hitBoth;

        public DrawableTestStrongHit(double startTime, HitResult type = HitResult.Great, bool hitBoth = true)
            : base(new Hit
            {
                IsStrong = true,
                StartTime = startTime,
            }, type)
        {
            this.hitBoth = hitBoth;
        }

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();

            var nestedStrongHit = (DrawableStrongNestedHit)NestedHitObjects.Single();
            nestedStrongHit.Result.Type = hitBoth ? Type : HitResult.Miss;
        }

        public override bool OnPressed(KeyBindingPressEvent<TaikoAction> e) => false;
    }
}
