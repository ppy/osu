// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Storyboards;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which overrides existing skin options with beatmap-local values.
    /// </summary>
    public partial class BeatmapSkinProvidingContainer : SkinProvidingContainer
    {
        private Bindable<bool> beatmapSkins = null!;
        private Bindable<bool> beatmapColours = null!;
        private Bindable<bool> beatmapHitsounds = null!;

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

        protected override bool AllowDrawableLookup(ISkinComponentLookup lookup)
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

        private readonly ISkin skin;
        private readonly ISkin? classicFallback;

        private Bindable<Skin> currentSkin = null!;

        public BeatmapSkinProvidingContainer(ISkin skin, ISkin? classicFallback = null)
            : base(skin)
        {
            this.skin = skin;
            this.classicFallback = classicFallback;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var config = parent.Get<OsuConfigManager>();

            beatmapSkins = config.GetBindable<bool>(OsuSetting.BeatmapSkins);
            beatmapColours = config.GetBindable<bool>(OsuSetting.BeatmapColours);
            beatmapHitsounds = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds);

            return base.CreateChildDependencies(parent);
        }

        [BackgroundDependencyLoader]
        private void load(SkinManager skins)
        {
            beatmapSkins.BindValueChanged(_ => TriggerSourceChanged());
            beatmapColours.BindValueChanged(_ => TriggerSourceChanged());
            beatmapHitsounds.BindValueChanged(_ => TriggerSourceChanged());

            currentSkin = skins.CurrentSkin.GetBoundCopy();
            currentSkin.BindValueChanged(_ =>
            {
                bool userSkinIsLegacy = skins.CurrentSkin.Value is LegacySkin;
                bool beatmapProvidingResources = skin is LegacySkinTransformer legacySkin && legacySkin.IsProvidingLegacyResources;

                // If the beatmap skin looks to have skinnable resources and the user's skin choice is not a legacy skin,
                // add the default classic skin as a fallback opportunity.
                if (!userSkinIsLegacy && beatmapProvidingResources && classicFallback != null)
                    SetSources(new[] { skin, classicFallback });
                else
                    SetSources(new[] { skin });
            }, true);
        }
    }
}
