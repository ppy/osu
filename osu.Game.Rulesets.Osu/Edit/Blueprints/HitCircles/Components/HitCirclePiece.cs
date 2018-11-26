// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components
{
    public class HitCirclePiece : HitObjectPiece
    {
        private readonly HitCircle hitCircle;

        public HitCirclePiece(HitCircle hitCircle)
            : base(hitCircle)
        {
            this.hitCircle = hitCircle;
            Origin = Anchor.Centre;

            Size = new Vector2((float)OsuHitObject.OBJECT_RADIUS * 2);
            Scale = new Vector2(hitCircle.Scale);
            CornerRadius = Size.X / 2;

            InternalChild = new RingPiece();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;

            PositionBindable.BindValueChanged(_ => UpdatePosition(), true);
            StackHeightBindable.BindValueChanged(_ => UpdatePosition());
            ScaleBindable.BindValueChanged(v => Scale = new Vector2(v), true);
        }

        protected virtual void UpdatePosition() => Position = hitCircle.StackedPosition;
    }
}
