// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Legacy;
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
            if (lookup is SkinComponentsContainerLookup containerLookup
                && containerLookup.Target == SkinComponentsContainerLookup.TargetArea.MainHUDComponents
                && containerLookup.Ruleset != null)
            {
                return base.GetDrawableComponent(lookup) ?? new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new LegacyComboCounter(),
                };
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
