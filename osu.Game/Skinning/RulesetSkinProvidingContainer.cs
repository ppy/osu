// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
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
        protected readonly Ruleset Ruleset;
        protected readonly IBeatmap Beatmap;

        protected override Container<Drawable> Content { get; }

        public RulesetSkinProvidingContainer(Ruleset ruleset, IBeatmap beatmap, [CanBeNull] ISkin beatmapSkin)
        {
            Ruleset = ruleset;
            Beatmap = beatmap;

            InternalChild = new BeatmapSkinProvidingContainer(beatmapSkin == null ? null : ruleset.CreateLegacySkinProvider(beatmapSkin, beatmap))
            {
                Child = Content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private AudioManager audio { get; set; }

        [Resolved]
        private SkinManager skinManager { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            UpdateSkins();
        }

        protected override void OnSourceChanged()
        {
            UpdateSkins();
            base.OnSourceChanged();
        }

        protected virtual void UpdateSkins()
        {
            SkinSources.Clear();

            SkinSources.Add(Ruleset.CreateLegacySkinProvider(skinManager.CurrentSkin.Value, Beatmap));

            if (skinManager.CurrentSkin.Value is LegacySkin)
                SkinSources.Add(Ruleset.CreateLegacySkinProvider(skinManager.DefaultLegacySkin, Beatmap));

            SkinSources.Add(Ruleset.CreateLegacySkinProvider(skinManager.DefaultSkin, Beatmap));

            SkinSources.Add(new RulesetResourcesSkin(Ruleset, host, audio));
        }
    }
}
