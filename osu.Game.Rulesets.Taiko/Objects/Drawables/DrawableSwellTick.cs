// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Framework.Input.Events;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public partial class DrawableSwellTick : DrawableTaikoHitObject<SwellTick>
    {
        public override bool DisplayResult => false;

        public DrawableSwellTick()
            : this(null)
        {
        }

        public DrawableSwellTick([CanBeNull] SwellTick hitObject)
            : base(hitObject)
        {
        }

        public void TriggerResult(bool hit)
        {
            HitObject.StartTime = Time.Current;

            if (hit)
                ApplyMaxResult();
            else
                ApplyMinResult();
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
        }

        public override bool OnPressed(KeyBindingPressEvent<TaikoAction> e) => false;

        protected override SkinnableDrawable CreateMainPiece() => null;
    }
}
