// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using OpenTK;
using System.Linq;
using osu.Game.Audio;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract class DrawableTaikoHitObject<TaikoHitType> : DrawableHitObject<TaikoHitObject>, IKeyBindingHandler<TaikoAction>
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

            InternalChild = MainPiece = CreateMainPiece();
            MainPiece.KiaiMode = HitObject.Kiai;
        }

        // Normal and clap samples are handled by the drum
        protected override IEnumerable<SampleInfo> GetSamples() => HitObject.Samples.Where(s => s.Name != SampleInfo.HIT_NORMAL && s.Name != SampleInfo.HIT_CLAP);

        protected override string SampleNamespace => "Taiko";

        protected virtual TaikoPiece CreateMainPiece() => new CirclePiece();

        public abstract bool OnPressed(TaikoAction action);

        public virtual bool OnReleased(TaikoAction action) => false;
    }
}
