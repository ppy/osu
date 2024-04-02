// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Skinning.Argon
{
    public class CatchArgonSkinTransformer : SkinTransformer
    {
        public CatchArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case CatchSkinComponentLookup catchComponent:
                    if (base.GetDrawableComponent(lookup) is Drawable c)
                        return c;

                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (catchComponent.Component)
                    {
                        case CatchSkinComponents.HitExplosion:
                            return new ArgonHitExplosion();

                        case CatchSkinComponents.Catcher:
                            return new ArgonCatcher();

                        case CatchSkinComponents.Fruit:
                            return new ArgonFruitPiece();

                        case CatchSkinComponents.Banana:
                            return new ArgonBananaPiece();

                        case CatchSkinComponents.Droplet:
                            return new ArgonDropletPiece();
                    }

                    break;
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
