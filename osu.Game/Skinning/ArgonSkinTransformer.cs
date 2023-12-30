// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
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
            switch (lookup)
            {
                case SkinComponentsContainerLookup containerLookup:
                    switch (containerLookup.Target)
                    {
                        case SkinComponentsContainerLookup.TargetArea.MainHUDComponents when containerLookup.Ruleset != null:
                            var rulesetHUDComponents = Skin.GetDrawableComponent(lookup);

                            rulesetHUDComponents ??= new DefaultSkinComponentsContainer(container =>
                            {
                                var combo = container.OfType<ArgonComboCounter>().FirstOrDefault();

                                if (combo != null)
                                {
                                    combo.Anchor = Anchor.BottomLeft;
                                    combo.Origin = Anchor.BottomLeft;
                                    combo.Position = new Vector2(36, -66);
                                    combo.Scale = new Vector2(1.3f);
                                }
                            })
                            {
                                new ArgonComboCounter(),
                            };

                            return rulesetHUDComponents;
                    }

                    break;
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
