// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenBeatmap : BlurrableBackgroundScreen
    {
        private WorkingBeatmap beatmap;

        /// <summary>
        /// Whether or not user dim settings should be applied to this Background.
        /// </summary>
        public readonly Bindable<bool> EnableUserDim = new Bindable<bool>();

        public readonly Bindable<bool> StoryboardReplacesBackground = new Bindable<bool>();

        private readonly UserDimContainer fadeContainer;

        protected virtual UserDimContainer CreateFadeContainer() => new UserDimContainer { RelativeSizeAxes = Axes.Both };

        public virtual WorkingBeatmap Beatmap
        {
            get => beatmap;
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
                        fadeContainer.Add(Background = b);
                        Background.BlurSigma = BlurTarget;
                        StoryboardReplacesBackground.BindTo(fadeContainer.StoryboardReplacesBackground);
                    }));
                });
            }
        }

        public BackgroundScreenBeatmap(WorkingBeatmap beatmap = null)
        {
            Beatmap = beatmap;
            InternalChild = fadeContainer = CreateFadeContainer();
            fadeContainer.EnableUserDim.BindTo(EnableUserDim);
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
