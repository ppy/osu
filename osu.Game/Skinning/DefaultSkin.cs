// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class DefaultSkin : Skin
    {
        private readonly IStorageResourceProvider resources;

        private static readonly IReadOnlyDictionary<GameplaySkinSamples, string> sample_mapping
            = new Dictionary<GameplaySkinSamples, string>
            {
                { GameplaySkinSamples.ResultScoreTick, @"Results/score-tick" },
                { GameplaySkinSamples.ResultBadgeTick, @"Results/badge-dink" },
                { GameplaySkinSamples.ResultBadgeTickMax, @"Results/badge-dink-max" },
                { GameplaySkinSamples.ResultSwooshUp, @"Results/swoosh-up" },
                { GameplaySkinSamples.ResultRank_D, @"Results/rank-impact-fail-d" },
                { GameplaySkinSamples.ResultRank_B, @"Results/rank-impact-fail" },
                { GameplaySkinSamples.ResultRank_C, @"Results/rank-impact-fail" },
                { GameplaySkinSamples.ResultRank_A, @"Results/rank-impact-pass" },
                { GameplaySkinSamples.ResultRank_S, @"Results/rank-impact-pass" },
                { GameplaySkinSamples.ResultRank_SS, @"Results/rank-impact-pass-ss" },
                { GameplaySkinSamples.ResultApplause_D, @"Results/applause-d" },
                { GameplaySkinSamples.ResultApplause_B, @"Results/applause-b" },
                { GameplaySkinSamples.ResultApplause_C, @"Results/applause-c" },
                { GameplaySkinSamples.ResultApplause_A, @"Results/applause-a" },
                { GameplaySkinSamples.ResultApplause_S, @"Results/applause-s" },
                { GameplaySkinSamples.ResultApplause_SS, @"Results/applause-s" }
            };

        public DefaultSkin(IStorageResourceProvider resources)
            : this(SkinInfo.Default, resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public DefaultSkin(SkinInfo skin, IStorageResourceProvider resources)
            : base(skin, resources)
        {
            this.resources = resources;
            Configuration = new DefaultSkinConfiguration();
        }

        public override Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => null;

        public override ISample GetSample(ISampleInfo sampleInfo)
        {
            foreach (var lookup in sampleInfo.LookupNames)
            {
                var sample = resources.AudioManager.Samples.Get(lookup);
                if (sample != null)
                    return sample;
            }

            return null;
        }

        public override Drawable GetDrawableComponent(ISkinComponent component)
        {
            if (base.GetDrawableComponent(component) is Drawable c)
                return c;

            switch (component)
            {
                case GameplaySkinComponent<GameplaySkinSamples> sample:
                    if (sample_mapping.ContainsKey(sample.Component))
                        return new DrawableSample(GetSample(new SampleInfo(sample_mapping[sample.Component])));

                    break;

                case SkinnableTargetComponent target:
                    switch (target.Target)
                    {
                        case SkinnableTarget.MainHUDComponents:
                            var skinnableTargetWrapper = new SkinnableTargetComponentsContainer(container =>
                            {
                                var score = container.OfType<DefaultScoreCounter>().FirstOrDefault();
                                var accuracy = container.OfType<DefaultAccuracyCounter>().FirstOrDefault();
                                var combo = container.OfType<DefaultComboCounter>().FirstOrDefault();

                                if (score != null)
                                {
                                    score.Anchor = Anchor.TopCentre;
                                    score.Origin = Anchor.TopCentre;

                                    // elements default to beneath the health bar
                                    const float vertical_offset = 30;

                                    const float horizontal_padding = 20;

                                    score.Position = new Vector2(0, vertical_offset);

                                    if (accuracy != null)
                                    {
                                        accuracy.Position = new Vector2(-accuracy.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).X / 2 - horizontal_padding, vertical_offset + 5);
                                        accuracy.Origin = Anchor.TopRight;
                                        accuracy.Anchor = Anchor.TopCentre;
                                    }

                                    if (combo != null)
                                    {
                                        combo.Position = new Vector2(accuracy.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).X / 2 + horizontal_padding, vertical_offset + 5);
                                        combo.Anchor = Anchor.TopCentre;
                                    }

                                    var hitError = container.OfType<HitErrorMeter>().FirstOrDefault();

                                    if (hitError != null)
                                    {
                                        hitError.Anchor = Anchor.CentreLeft;
                                        hitError.Origin = Anchor.CentreLeft;
                                    }

                                    var hitError2 = container.OfType<HitErrorMeter>().LastOrDefault();

                                    if (hitError2 != null)
                                    {
                                        hitError2.Anchor = Anchor.CentreRight;
                                        hitError2.Scale = new Vector2(-1, 1);
                                        // origin flipped to match scale above.
                                        hitError2.Origin = Anchor.CentreLeft;
                                    }
                                }
                            })
                            {
                                Children = new[]
                                {
                                    GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.ComboCounter)),
                                    GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.ScoreCounter)),
                                    GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.AccuracyCounter)),
                                    GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.HealthDisplay)),
                                    GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.SongProgress)),
                                    GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.BarHitErrorMeter)),
                                    GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.BarHitErrorMeter)),
                                }
                            };

                            return skinnableTargetWrapper;
                    }

                    break;

                case HUDSkinComponent hudComponent:
                {
                    switch (hudComponent.Component)
                    {
                        case HUDSkinComponents.ComboCounter:
                            return new DefaultComboCounter();

                        case HUDSkinComponents.ScoreCounter:
                            return new DefaultScoreCounter();

                        case HUDSkinComponents.AccuracyCounter:
                            return new DefaultAccuracyCounter();

                        case HUDSkinComponents.HealthDisplay:
                            return new DefaultHealthDisplay();

                        case HUDSkinComponents.SongProgress:
                            return new SongProgress();

                        case HUDSkinComponents.BarHitErrorMeter:
                            return new BarHitErrorMeter();

                        case HUDSkinComponents.ColourHitErrorMeter:
                            return new ColourHitErrorMeter();
                    }

                    break;
                }
            }

            return null;
        }

        public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                // todo: this code is pulled from LegacySkin and should not exist.
                // will likely change based on how databased storage of skin configuration goes.
                case GlobalSkinColours global:
                    switch (global)
                    {
                        case GlobalSkinColours.ComboColours:
                            return SkinUtils.As<TValue>(new Bindable<IReadOnlyList<Color4>>(Configuration.ComboColours));
                    }

                    break;
            }

            return null;
        }
    }
}
