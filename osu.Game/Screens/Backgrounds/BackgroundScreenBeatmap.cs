// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenBeatmap : BlurrableBackgroundScreen
    {
        private WorkingBeatmap beatmap;

        public WorkingBeatmap Beatmap
        {
            get { return beatmap; }
            set
            {
                if (beatmap == value && beatmap != null)
                    return;

                beatmap = value;

                Schedule(() =>
                {
                    LoadComponentAsync(new BeatmapBackground(beatmap), b => Schedule(() =>
                    {
                        float newDepth = 0;
                        if (Background != null)
                        {
                            newDepth = Background.Depth + 1;
                            Background.FinishTransforms();
                            Background.FadeOut(250);
                            Background.Expire();
                        }

                        b.Depth = newDepth;
                        Add(Background = b);
                        Background.BlurSigma = BlurTarget;
                    }));
                });
            }
        }

        public BackgroundScreenBeatmap(WorkingBeatmap beatmap = null)
        {
            Beatmap = beatmap;
        }

        public override bool Equals(BackgroundScreen other)
        {
            var otherBeatmapBackground = other as BackgroundScreenBeatmap;
            if (otherBeatmapBackground == null) return false;

            return base.Equals(other) && beatmap == otherBeatmapBackground.Beatmap;
        }

        private class BeatmapBackground : Background
        {
            private readonly WorkingBeatmap beatmap;

            public BeatmapBackground(WorkingBeatmap beatmap)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Sprite.Texture = beatmap?.Background ?? textures.Get(@"Backgrounds/bg1");
            }
        }
    }
}
