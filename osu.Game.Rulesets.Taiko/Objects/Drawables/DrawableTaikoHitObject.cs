// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using OpenTK;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract class DrawableTaikoHitObject<TaikoHitType> : DrawableScrollingHitObject<TaikoHitObject>, IKeyBindingHandler<TaikoAction>
        where TaikoHitType : TaikoHitObject
    {
        public override Vector2 OriginPosition => new Vector2(DrawHeight / 2);

        protected readonly Vector2 BaseSize;

        protected readonly TaikoPiece MainPiece;

        public new TaikoHitType HitObject;

        protected DrawableTaikoHitObject(TaikoHitType hitObject)
            : base(hitObject)
        {
            HitObject = hitObject;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Custom;

            RelativeSizeAxes = Axes.Both;
            Size = BaseSize = new Vector2(HitObject.IsStrong ? TaikoHitObject.DEFAULT_STRONG_SIZE : TaikoHitObject.DEFAULT_SIZE);

            Add(MainPiece = CreateMainPiece());
            MainPiece.KiaiMode = HitObject.Kiai;
        }

        protected virtual TaikoPiece CreateMainPiece() => new CirclePiece();

        public abstract bool OnPressed(TaikoAction action);

        public virtual bool OnReleased(TaikoAction action) => false;
    }
}
