// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public class ManiaArgonSkinTransformer : SkinTransformer
    {
        public ManiaArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponent component)
        {
            switch (component)
            {
                case ManiaSkinComponent maniaComponent:
                    switch (maniaComponent.Component)
                    {
                        case ManiaSkinComponents.HitTarget:
                            return new ArgonHitTarget();

                        case ManiaSkinComponents.KeyArea:
                            return new ArgonKeyArea();

                        // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    }

                    break;
            }

            return base.GetDrawableComponent(component);
        }
    }
}
