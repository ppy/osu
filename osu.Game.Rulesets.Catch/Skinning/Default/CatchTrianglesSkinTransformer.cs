// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
    public class CatchTrianglesSkinTransformer : SkinTransformer
    {
        public CatchTrianglesSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case GlobalSkinnableContainerLookup containerLookup:
                {
                    // Only handle per ruleset defaults here.
                    if (containerLookup.Ruleset == null)
                        return base.GetDrawableComponent(lookup);

                    switch (containerLookup.Lookup)
                    {
                        case GlobalSkinnableContainers.MainHUDComponents:
                            return new DefaultSkinComponentsContainer(container =>
                            {
                                const float screen_edge_padding = 10;
                                // Hard to find this at runtime, so taken from the most expanded state during replay.
                                const float song_progress_offset_height = 73;

                                var leaderboard = container.OfType<DrawableGameplayLeaderboard>().FirstOrDefault();
                                var spectatorList = container.OfType<SpectatorList>().FirstOrDefault();
                                var hitError = container.OfType<HitErrorMeter>().FirstOrDefault();
                                var hitError2 = container.OfType<HitErrorMeter>().LastOrDefault();

                                if (leaderboard != null)
                                    leaderboard.Position = new Vector2(40, 60);

                                if (spectatorList != null)
                                {
                                    spectatorList.HeaderFont.Value = Typeface.Venera;
                                    spectatorList.HeaderColour.Value = new OsuColour().BlueLighter;
                                    spectatorList.Anchor = Anchor.BottomLeft;
                                    spectatorList.Origin = Anchor.BottomLeft;
                                    spectatorList.Position = new Vector2(10, -(song_progress_offset_height + screen_edge_padding));
                                }

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
                                    new SpectatorList(),
                                    new ColourHitErrorMeter(),
                                    new ColourHitErrorMeter(),
                                },
                            };
                    }

                    return null;
                }
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
