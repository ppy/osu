// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Storyboards;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which overrides existing skin options with beatmap-local values.
    /// This also applies ruleset-specific skin transformer to the beatmap skin, similar to <see cref="RulesetSkinProvidingContainer"/>.
    /// </summary>
    public class BeatmapSkinProvidingContainer : SkinProvidingContainer
    {
        private Bindable<bool> beatmapSkins;
        private Bindable<bool> beatmapColours;
        private Bindable<bool> beatmapHitsounds;

        protected override bool AllowConfigurationLookup
        {
            get
            {
                if (beatmapSkins == null)
                    throw new InvalidOperationException($"{nameof(BeatmapSkinProvidingContainer)} needs to be loaded before being consumed.");

                return beatmapSkins.Value;
            }
        }

        protected override bool AllowColourLookup
        {
            get
            {
                if (beatmapColours == null)
                    throw new InvalidOperationException($"{nameof(BeatmapSkinProvidingContainer)} needs to be loaded before being consumed.");

                return beatmapColours.Value;
            }
        }

        protected override bool AllowDrawableLookup(ISkinComponent component)
        {
            if (beatmapSkins == null)
                throw new InvalidOperationException($"{nameof(BeatmapSkinProvidingContainer)} needs to be loaded before being consumed.");

            return beatmapSkins.Value;
        }

        protected override bool AllowTextureLookup(string componentName)
        {
            if (beatmapSkins == null)
                throw new InvalidOperationException($"{nameof(BeatmapSkinProvidingContainer)} needs to be loaded before being consumed.");

            return beatmapSkins.Value;
        }

        protected override bool AllowSampleLookup(ISampleInfo sampleInfo)
        {
            if (beatmapSkins == null)
                throw new InvalidOperationException($"{nameof(BeatmapSkinProvidingContainer)} needs to be loaded before being consumed.");

            return sampleInfo is StoryboardSampleInfo || beatmapHitsounds.Value;
        }

        [CanBeNull]
        private readonly ISkin skin;

        [CanBeNull]
        private readonly Ruleset ruleset;

        [CanBeNull]
        private readonly IBeatmap beatmap;

        public BeatmapSkinProvidingContainer(ISkin skin, Ruleset ruleset = null, IBeatmap beatmap = null)
        {
            this.skin = skin;
            this.ruleset = ruleset;
            this.beatmap = beatmap;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var config = parent.Get<OsuConfigManager>();

            beatmapSkins = config.GetBindable<bool>(OsuSetting.BeatmapSkins);
            beatmapColours = config.GetBindable<bool>(OsuSetting.BeatmapColours);
            beatmapHitsounds = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds);

            return base.CreateChildDependencies(parent);
        }

        [Resolved]
        private SkinManager skins { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            beatmapSkins.BindValueChanged(_ => TriggerSourceChanged());
            beatmapColours.BindValueChanged(_ => TriggerSourceChanged());
            beatmapHitsounds.BindValueChanged(_ => TriggerSourceChanged());

            if (skin != null)
                SetSources(getBeatmapSkinSources());
        }

        private IEnumerable<ISkin> getBeatmapSkinSources()
        {
            ISkin transformedSkin;

            yield return transformedSkin = ruleset == null ? skin : skin.WithRulesetTransformer(ruleset, beatmap.AsNonNull());

            if (transformedSkin is LegacySkinTransformer legacySkin && legacySkin.IsProvidingLegacyResources)
                yield return ruleset == null ? skins.DefaultClassicSkin : skins.DefaultClassicSkin.WithRulesetTransformer(ruleset, beatmap.AsNonNull());
        }
    }
}
