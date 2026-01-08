// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public class TaikoArgonSkinTransformer : SkinTransformer
    {
        public TaikoArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case GlobalSkinnableContainerLookup containerLookup:
                    // Only handle per ruleset defaults here.
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

                                if (leaderboard != null)
                                {
                                    leaderboard.Anchor = leaderboard.Origin = Anchor.BottomLeft;
                                    leaderboard.Position = new Vector2(36, -140);
                                    leaderboard.Height = 140;
                                }

                                if (comboCounter != null)
                                    comboCounter.Position = new Vector2(36, -66);

                                if (spectatorList != null)
                                {
                                    spectatorList.Position = new Vector2(320, -280);
                                    spectatorList.Anchor = Anchor.BottomLeft;
                                    spectatorList.Origin = Anchor.TopLeft;
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
                                    }
                                },
                            };
                    }

                    return null;

                case SkinComponentLookup<HitResult> resultComponent:
                    // This should eventually be moved to a skin setting, when supported.
                    if (Skin is ArgonProSkin && resultComponent.Component >= HitResult.Great)
                        return Drawable.Empty();

                    return new ArgonJudgementPiece(resultComponent.Component);

                case TaikoSkinComponentLookup taikoComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (taikoComponent.Component)
                    {
                        case TaikoSkinComponents.CentreHit:
                            return new ArgonCentreCirclePiece();

                        case TaikoSkinComponents.RimHit:
                            return new ArgonRimCirclePiece();

                        case TaikoSkinComponents.PlayfieldBackgroundLeft:
                            return new ArgonPlayfieldBackgroundLeft();

                        case TaikoSkinComponents.PlayfieldBackgroundRight:
                            return new ArgonPlayfieldBackgroundRight();

                        case TaikoSkinComponents.InputDrum:
                            return new ArgonInputDrum();

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

                        case TaikoSkinComponents.DrumSamplePlayer:
                            return new ArgonDrumSamplePlayer();

                        case TaikoSkinComponents.TaikoExplosionGreat:
                        case TaikoSkinComponents.TaikoExplosionMiss:
                        case TaikoSkinComponents.TaikoExplosionOk:
                            return new ArgonHitExplosion(taikoComponent.Component);

                        case TaikoSkinComponents.Swell:
                            return new ArgonSwell();
                    }

                    break;
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
