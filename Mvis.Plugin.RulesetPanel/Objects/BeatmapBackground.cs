using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;

namespace Mvis.Plugin.RulesetPanel.Objects
{
    public class BeatmapBackground : CompositeDrawable
    {
        private readonly WorkingBeatmap beatmap;

        public BeatmapBackground(WorkingBeatmap beatmap = null)
        {
            this.beatmap = beatmap;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fill,
                Texture = beatmap?.Background ?? textures.Get(@"Backgrounds/bg4")
            };
        }
    }
}
