// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components
{
    public class HitCirclePiece : CompositeDrawable
    {
        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();
        private readonly IBindable<int> stackHeightBindable = new Bindable<int>();
        private readonly IBindable<float> scaleBindable = new Bindable<float>();

        private readonly HitCircle hitCircle;

        public HitCirclePiece(HitCircle hitCircle)
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

            positionBindable.BindValueChanged(_ => UpdatePosition());
            stackHeightBindable.BindValueChanged(_ => UpdatePosition());
            scaleBindable.BindValueChanged(v => Scale = new Vector2(v));

            positionBindable.BindTo(hitCircle.PositionBindable);
            stackHeightBindable.BindTo(hitCircle.StackHeightBindable);
            scaleBindable.BindTo(hitCircle.ScaleBindable);
        }

        protected virtual void UpdatePosition() => Position = hitCircle.StackedPosition;
    }
}
