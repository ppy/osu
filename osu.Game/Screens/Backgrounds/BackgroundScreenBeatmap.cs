// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenBeatmap : BlurrableBackgroundScreen
    {
        private WorkingBeatmap beatmap;
        protected Bindable<double> DimLevel;
        protected Bindable<double> BlurLevel;
        public Bindable<bool> EnableUserDim;

        protected UserDimContainer FadeContainer;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            DimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
        }

        public virtual WorkingBeatmap Beatmap
        {
            get { return beatmap; }
            set
            {
                if (beatmap == value && beatmap != null)
                    return;

                beatmap = value;

                Schedule(() =>
                {
                    FadeContainer = new UserDimContainer { RelativeSizeAxes = Axes.Both };
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
                        FadeContainer.Child = Background = b;
                        Background.BlurSigma = BlurTarget;
                    }));
                    InternalChild = FadeContainer;
                    EnableUserDim = FadeContainer.EnableUserDim;
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

        protected class BeatmapBackground : Background
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
