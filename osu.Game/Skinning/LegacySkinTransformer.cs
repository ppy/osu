// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Legacy;
using static osu.Game.Skinning.SkinConfiguration;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Transformer used to handle support of legacy features for individual rulesets.
    /// </summary>
    public abstract class LegacySkinTransformer : SkinTransformer
    {
        /// <summary>
        /// Whether the skin being transformed is able to provide legacy resources for the ruleset.
        /// </summary>
        public virtual bool IsProvidingLegacyResources => this.HasFont(LegacyFont.Combo);

        protected LegacySkinTransformer(ISkin skin)
            : base(skin)
        {
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

        protected Drawable? ReplaceBaseComboCounter(ISkinComponentLookup lookup)
        {
            if (lookup is SkinComponentsContainerLookup containerLookup)
            {
                switch (containerLookup.Target)
                {
                    case SkinComponentsContainerLookup.TargetArea.MainHUDComponents:
                        var components = base.GetDrawableComponent(lookup) as Container;

                        if (components != null)
                        {
                            // some rulesets may provide its own combo counter; hide the default.
                            // todo: this should be done in an elegant way per ruleset, defining which HUD skin components should be displayed.
                            foreach (var legacyBaseComboCounter in components.OfType<LegacyBaseComboCounter>())
                                legacyBaseComboCounter.HiddenByRulesetImplementation = false;
                        }

                        return components;
                }
            }

            return null;
        }
    }
}
