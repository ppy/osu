// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A type of <see cref="SkinProvidingContainer"/> specialized for <see cref="DrawableRuleset"/> and other gameplay-related components.
    /// Providing access to parent skin sources and the beatmap skin each surrounded with the ruleset legacy skin transformer.
    /// </summary>
    public class RulesetSkinProvidingContainer : SkinProvidingContainer
    {
        protected readonly Ruleset Ruleset;
        protected readonly IBeatmap Beatmap;

        /// <remarks>
        /// This container already re-exposes all parent <see cref="ISkinSource"/> sources in a ruleset-usable form.
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

        private ISkinSource parentSource;

        private ResourceStoreBackedSkin rulesetResourcesSkin;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            parentSource = parent.Get<ISkinSource>();
            parentSource.SourceChanged += OnSourceChanged;

            if (Ruleset.CreateResourceStore() is IResourceStore<byte[]> resources)
                rulesetResourcesSkin = new ResourceStoreBackedSkin(resources, parent.Get<GameHost>(), parent.Get<AudioManager>());

            // ensure sources are populated and ready for use before childrens' asynchronous load flow.
            UpdateSkinSources();

            return base.CreateChildDependencies(parent);
        }

        protected override void OnSourceChanged()
        {
            UpdateSkinSources();
            base.OnSourceChanged();
        }

        protected virtual void UpdateSkinSources()
        {
            SkinSources.Clear();

            foreach (var skin in parentSource.AllSources)
            {
                switch (skin)
                {
                    case LegacySkin legacySkin:
                        SkinSources.Add(GetLegacyRulesetTransformedSkin(legacySkin));
                        break;

                    default:
                        SkinSources.Add(skin);
                        break;
                }
            }

            int lastDefaultSkinIndex = SkinSources.IndexOf(SkinSources.OfType<DefaultSkin>().LastOrDefault());

            // Ruleset resources should be given the ability to override game-wide defaults
            // This is achieved by placing them before the last instance of DefaultSkin.
            // Note that DefaultSkin may not be present in some test scenes.
            if (lastDefaultSkinIndex >= 0)
                SkinSources.Insert(lastDefaultSkinIndex, rulesetResourcesSkin);
            else
                SkinSources.Add(rulesetResourcesSkin);
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

            if (parentSource != null)
                parentSource.SourceChanged -= OnSourceChanged;

            rulesetResourcesSkin?.Dispose();
        }
    }
}
