// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning;
using osuTK;

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
                case GlobalSkinnableContainerLookup containerLookup:
                    if (containerLookup.Ruleset == null)
                        return base.GetDrawableComponent(lookup);

                    switch (containerLookup.Lookup)
                    {
                        case GlobalSkinnableContainers.MainHUDComponents:
                            return new DefaultSkinComponentsContainer(container =>
                            {
                                var leaderboard = container.OfType<DrawableGameplayLeaderboard>().FirstOrDefault();
                                var comboCounter = container.OfType<ArgonComboCounter>().FirstOrDefault();
                                var spectatorList = container.OfType<SpectatorList>().FirstOrDefault();
                                var hitError = container.OfType<HitErrorMeter>().FirstOrDefault();
                                var hitError2 = container.OfType<HitErrorMeter>().LastOrDefault();

                                if (leaderboard != null)
                                    leaderboard.Position = new Vector2(36, 115);

                                Vector2 pos = new Vector2(36, -66);

                                if (comboCounter != null)
                                {
                                    comboCounter.Position = pos;
                                    pos -= new Vector2(0, comboCounter.DrawHeight * 1.4f + 20);
                                }

                                if (spectatorList != null)
                                    spectatorList.Position = pos;

                                if (hitError is ColourHitErrorMeter colourHitError)
                                {
                                    colourHitError.Anchor = Anchor.CentreLeft;
                                    colourHitError.Origin = Anchor.CentreLeft;
                                    colourHitError.JudgementCount.Value = 28;
                                    colourHitError.JudgementSpacing.Value = 1.25f;
                                    colourHitError.JudgementShape.Value = ColourHitErrorMeter.ShapeStyle.Square;
                                }

                                if (hitError2 is ColourHitErrorMeter colourHitError2)
                                {
                                    colourHitError2.Anchor = Anchor.CentreRight;
                                    colourHitError2.Scale = new Vector2(-1, 1);
                                    // origin flipped to match scale above.
                                    colourHitError2.Origin = Anchor.CentreLeft;
                                    colourHitError2.JudgementCount.Value = 28;
                                    colourHitError2.JudgementSpacing.Value = 1.25f;
                                    colourHitError2.JudgementShape.Value = ColourHitErrorMeter.ShapeStyle.Square;
                                }

                                foreach (var d in container.OfType<ISerialisableDrawable>())
                                    d.UsesFixedAnchor = true;
                            })
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new DrawableGameplayLeaderboard(),
                                    new ArgonComboCounter
                                    {
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        Scale = new Vector2(1.3f),
                                    },
                                    new SpectatorList
                                    {
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                    },
                                    new ColourHitErrorMeter(),
                                    new ColourHitErrorMeter(),
                                },
                            };
                    }

                    return null;

                case CatchSkinComponentLookup catchComponent:
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
