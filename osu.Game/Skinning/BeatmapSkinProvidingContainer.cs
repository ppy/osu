// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which overrides existing skin options with beatmap-local values.
    /// </summary>
    public partial class BeatmapSkinProvidingContainer : SkinProvidingContainer
    {
        public BindableWithCurrent<bool> BeatmapSkins = new BindableWithCurrent<bool>(true);
        public BindableWithCurrent<bool> BeatmapColours = new BindableWithCurrent<bool>(true);
        public BindableWithCurrent<bool> BeatmapHitsounds = new BindableWithCurrent<bool>(true);

        protected override bool AllowConfigurationLookup => BeatmapSkins.Value;

        protected override bool AllowColourLookup => BeatmapColours.Value;

        protected override bool AllowDrawableLookup(ISkinComponentLookup lookup) => BeatmapSkins.Value;

        protected override bool AllowTextureLookup(string componentName) => BeatmapSkins.Value;

        protected override bool AllowSampleLookup(ISampleInfo sampleInfo) => BeatmapHitsounds.Value;

        private readonly ISkin skin;
        private readonly ISkin? classicFallback;

        private Bindable<Skin> currentSkin = null!;

        public BeatmapSkinProvidingContainer(ISkin skin, ISkin? classicFallback = null)
            : base(skin)
        {
            this.skin = skin;
            this.classicFallback = classicFallback;
        }

        [BackgroundDependencyLoader]
        private void load(SkinManager skins)
        {
            BeatmapSkins.BindValueChanged(_ => TriggerSourceChanged());
            BeatmapColours.BindValueChanged(_ => TriggerSourceChanged());
            BeatmapHitsounds.BindValueChanged(_ => TriggerSourceChanged());

            currentSkin = skins.CurrentSkin.GetBoundCopy();
            currentSkin.BindValueChanged(_ =>
            {
                bool userSkinIsLegacy = skins.CurrentSkin.Value is LegacySkin;
                bool beatmapProvidingResources = skin is LegacySkinTransformer legacySkin && legacySkin.IsProvidingLegacyResources;

                // Some beatmaps provide a limited selection of skin elements to add some visual flair.
                // In stable, these elements will take lookup priority over the selected skin (whether that be a user skin or default).
                //
                // To replicate this we need to pay special attention to the fallback order.
                // If a user has a non-legacy skin (argon, triangles) selected, the game won't normally fall back to a legacy skin.
                // In turn this can create an unexpected visual experience.
                //
                // So here, check what skin the user has selected. If it's already a legacy skin then we don't need to do anything special.
                // If it isn't, we insert the classic default. Note that this is only done if the beatmap seems to be providing skin elements,
                // as we only want to override the user's (non-legacy) skin choice when required for beatmap skin visuals.
                if (!userSkinIsLegacy && beatmapProvidingResources && classicFallback != null)
                    SetSources(new[] { skin, classicFallback });
                else
                    SetSources(new[] { skin });
            }, true);
        }
    }
}
