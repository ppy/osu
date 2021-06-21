// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A type of <see cref="SkinProvidingContainer"/> specialized for <see cref="DrawableRuleset"/> and other gameplay-related components.
    /// Providing access to the <see cref="SkinManager"/> skin sources and the beatmap skin each surrounded with the ruleset legacy skin transformer.
    /// While also limiting lookups from falling back to any parent <see cref="ISkinSource"/>s out of this container.
    /// </summary>
    public class RulesetSkinProvidingContainer : SkinProvidingContainer
    {
        protected readonly Ruleset Ruleset;
        protected readonly IBeatmap Beatmap;

        /// <remarks>
        /// This container already re-exposes all <see cref="SkinManager"/> skin sources in a ruleset-usable form.
        /// Therefore disallow falling back to any parent <see cref="ISkinSource"/> any further.
        /// </remarks>
        protected override bool AllowFallingBackToParent => false;

        protected override Container<Drawable> Content { get; }

        public RulesetSkinProvidingContainer(Ruleset ruleset, IBeatmap beatmap, [CanBeNull] ISkin beatmapSkin)
        {
            Ruleset = ruleset;
            Beatmap = beatmap;

            InternalChild = new BeatmapSkinProvidingContainer(beatmapSkin is LegacySkin ? GetLegacyRulesetTransformedSkin(beatmapSkin) : beatmapSkin)
            {
                Child = Content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        [Resolved]
        private SkinManager skinManager { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            UpdateSkins();
            skinManager.SourceChanged += UpdateSkins;
        }

        protected virtual void UpdateSkins()
        {
            SkinSources.Clear();

            // TODO: we also want to insert a DefaultLegacySkin here if the current *beatmap* is providing any skinned elements.

            switch (skinManager.CurrentSkin.Value)
            {
                case LegacySkin currentLegacySkin:
                    SkinSources.Add(GetLegacyRulesetTransformedSkin(currentLegacySkin));

                    if (currentLegacySkin != skinManager.DefaultLegacySkin)
                        SkinSources.Add(GetLegacyRulesetTransformedSkin(skinManager.DefaultLegacySkin));

                    break;

                default:
                    SkinSources.Add(skinManager.CurrentSkin.Value);
                    break;
            }

            SkinSources.Add(skinManager.DefaultSkin);
        }

        protected ISkin GetLegacyRulesetTransformedSkin(ISkin legacySkin)
        {
            if (legacySkin == null)
                return null;

            var rulesetTransformed = Ruleset.CreateLegacySkinProvider(legacySkin, Beatmap);
            if (rulesetTransformed != null)
                return rulesetTransformed;

            return legacySkin;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (skinManager != null)
                skinManager.SourceChanged -= UpdateSkins;
        }
    }
}
