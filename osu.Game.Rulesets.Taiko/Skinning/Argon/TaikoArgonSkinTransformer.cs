// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public class TaikoArgonSkinTransformer : SkinTransformer
    {
        public TaikoArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponent component)
        {
            switch (component)
            {
                case GameplaySkinComponent<HitResult> resultComponent:
                    return new ArgonJudgementPiece(resultComponent.Component);

                case TaikoSkinComponent catchComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (catchComponent.Component)
                    {
                        case TaikoSkinComponents.CentreHit:
                            return new ArgonCentreCirclePiece();

                        case TaikoSkinComponents.RimHit:
                            return new ArgonRimCirclePiece();

                        case TaikoSkinComponents.PlayfieldBackgroundLeft:
                            return new ArgonPlayfieldBackgroundLeft();

                        case TaikoSkinComponents.PlayfieldBackgroundRight:
                            return new ArgonPlayfieldBackgroundRight();

                        case TaikoSkinComponents.HitTarget:
                            return new ArgonHitTarget();

                        case TaikoSkinComponents.BarLine:
                            return new ArgonBarLine();

                        case TaikoSkinComponents.DrumRollBody:
                            return new ArgonElongatedCirclePiece();

                        case TaikoSkinComponents.DrumRollTick:
                            return new ArgonTickPiece();

                        case TaikoSkinComponents.TaikoExplosionKiai:
                            // the drawable needs to expire as soon as possible to avoid accumulating empty drawables on the playfield.
                            return Drawable.Empty().With(d => d.Expire());

                        case TaikoSkinComponents.TaikoExplosionGreat:
                        case TaikoSkinComponents.TaikoExplosionMiss:
                        case TaikoSkinComponents.TaikoExplosionOk:
                            return new ArgonHitExplosion(catchComponent.Component);
                    }

                    break;
            }

            return base.GetDrawableComponent(component);
        }
    }
}
