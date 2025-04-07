// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
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
    public partial class RulesetSkinProvidingContainer : SkinProvidingContainer
    {
        protected readonly Ruleset Ruleset;
        protected readonly IBeatmap Beatmap;

        [CanBeNull]
        private readonly ISkin beatmapSkin;

        protected override Container<Drawable> Content { get; } = new Container
        {
            RelativeSizeAxes = Axes.Both,
        };

        public RulesetSkinProvidingContainer(Ruleset ruleset, IBeatmap beatmap, [CanBeNull] ISkin beatmapSkin)
        {
            Ruleset = ruleset;
            Beatmap = beatmap;
            this.beatmapSkin = beatmapSkin;
        }

        [BackgroundDependencyLoader]
        private void load(SkinManager skinManager)
        {
            InternalChild = new BeatmapSkinProvidingContainer(GetRulesetTransformedSkin(beatmapSkin), GetRulesetTransformedSkin(skinManager.DefaultClassicSkin))
            {
                Child = Content,
            };
        }

        private ResourceStoreBackedSkin rulesetResourcesSkin;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            if (Ruleset.CreateResourceStore() is IResourceStore<byte[]> resources)
                rulesetResourcesSkin = new ResourceStoreBackedSkin(resources, parent.Get<GameHost>(), parent.Get<AudioManager>());

            return base.CreateChildDependencies(parent);
        }

        protected override void RefreshSources()
        {
            // Populate a local list first so we can adjust the returned order as we go.
            var sources = new List<ISkin>();

            // We want to transform the current user's skin for the current ruleset.
            // Assume it's the first skin provided by the parent source (generally the case for both SkinManager and tests).
            if (ParentSource?.AllSources.FirstOrDefault() is ISkin skin)
                sources.Add(GetRulesetTransformedSkin(skin));

            // Ruleset resources should be given the ability to override game-wide defaults
            sources.Add(rulesetResourcesSkin);

            SetSources(sources);
        }

        protected ISkin GetRulesetTransformedSkin(ISkin skin)
        {
            if (skin == null)
                return null;

            var rulesetTransformed = Ruleset.CreateSkinTransformer(skin, Beatmap);
            if (rulesetTransformed != null)
                return rulesetTransformed;

            return skin;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            rulesetResourcesSkin?.Dispose();
        }
    }
}
