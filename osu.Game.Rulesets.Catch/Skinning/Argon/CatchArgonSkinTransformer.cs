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

        public override Drawable? GetDrawableComponent(ISkinComponent component)
        {
            switch (component)
            {
                case CatchSkinComponent osuComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (osuComponent.Component)
                    {
                        case CatchSkinComponents.Catcher:
                            return new ArgonCatcher();

                        case CatchSkinComponents.Fruit:
                            return new ArgonFruitPiece();
                    }

                    break;
            }

            return base.GetDrawableComponent(component);
        }
    }
}
