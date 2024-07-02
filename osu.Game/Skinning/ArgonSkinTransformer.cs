// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning
{
    public class ArgonSkinTransformer : SkinTransformer
    {
        public ArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            if (lookup is SkinComponentsContainerLookup containerLookup
                && containerLookup.Target == SkinComponentsContainerLookup.TargetArea.MainHUDComponents
                && containerLookup.Ruleset != null)
            {
                return base.GetDrawableComponent(lookup) ?? new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new ArgonComboCounter
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Position = new Vector2(36, -66),
                        Scale = new Vector2(1.3f),
                    },
                };
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
