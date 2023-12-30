// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Legacy;
using osuTK;
using static osu.Game.Skinning.SkinConfiguration;

namespace osu.Game.Skinning
{
    public class LegacySkinTransformer : SkinTransformer
    {
        /// <summary>
        /// Whether the skin being transformed is able to provide legacy resources for the ruleset.
        /// </summary>
        public virtual bool IsProvidingLegacyResources => this.HasFont(LegacyFont.Combo);

        public LegacySkinTransformer(ISkin skin)
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
                            var rulesetHUDComponents = base.GetDrawableComponent(lookup);

                            rulesetHUDComponents ??= new DefaultSkinComponentsContainer(container =>
                            {
                                var combo = container.OfType<LegacyDefaultComboCounter>().FirstOrDefault();

                                if (combo != null)
                                {
                                    combo.Anchor = Anchor.BottomLeft;
                                    combo.Origin = Anchor.BottomLeft;
                                    combo.Scale = new Vector2(1.28f);
                                }
                            })
                            {
                                new LegacyDefaultComboCounter()
                            };

                            return rulesetHUDComponents;
                    }

                    break;
            }

            return base.GetDrawableComponent(lookup);
        }

        public override ISample? GetSample(ISampleInfo sampleInfo)
        {
            if (!(sampleInfo is ConvertHitObjectParser.LegacyHitSampleInfo legacySample))
                return Skin.GetSample(sampleInfo);

            var playLayeredHitSounds = GetConfig<LegacySetting, bool>(LegacySetting.LayeredHitSounds);
            if (legacySample.IsLayered && playLayeredHitSounds?.Value == false)
                return new SampleVirtual();

            return base.GetSample(sampleInfo);
        }
    }
}
