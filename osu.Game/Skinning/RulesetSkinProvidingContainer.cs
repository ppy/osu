// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A type of <see cref="SkinProvidingContainer"/> that provides access to the beatmap skin and user skin,
    /// each transformed with the ruleset's own skin transformer individually.
    /// </summary>
    public class RulesetSkinProvidingContainer : SkinProvidingContainer
    {
        private readonly Ruleset ruleset;
        private readonly IBeatmap beatmap;

        protected override Container<Drawable> Content { get; }

        public RulesetSkinProvidingContainer(Ruleset ruleset, IBeatmap beatmap, ISkin beatmapSkin)
        {
            this.ruleset = ruleset;
            this.beatmap = beatmap;

            InternalChild = new BeatmapSkinProvidingContainer(ruleset.CreateLegacySkinProvider(beatmapSkin, beatmap))
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
            updateSkins();
        }

        protected override void OnSourceChanged()
        {
            updateSkins();
            base.OnSourceChanged();
        }

        private void updateSkins()
        {
            SkinSources.Clear();
            SkinSources.AddRange(skinManager.CurrentSkinLayers.Select(s => ruleset.CreateLegacySkinProvider(s, beatmap)));
        }
    }
}
