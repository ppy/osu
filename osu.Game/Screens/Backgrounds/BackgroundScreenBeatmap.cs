// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osuTK.Graphics;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenBeatmap : BlurrableBackgroundScreen
    {
        private WorkingBeatmap beatmap;
        protected Bindable<double> DimLevel;
        public Bindable<bool> UpdateDim;

        protected Container FadeContainer;

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
                    FadeContainer = new Container { RelativeSizeAxes = Axes.Both };
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
                        InternalChild = FadeContainer;
                        Background.BlurSigma = BlurTarget;
                    }));
                });
            }
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            DimLevel.ValueChanged += _ => updateBackgroundDim();
            UpdateDim.ValueChanged += _ => updateBackgroundDim();
            updateBackgroundDim();
        }
        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);
            updateBackgroundDim();
        }

        public override bool OnExiting(IScreen last)
        {
            return base.OnExiting(last);
        }

        private void updateBackgroundDim()
        {
            if (UpdateDim)
                FadeContainer?.FadeColour(OsuColour.Gray(1 - (float)DimLevel), 800, Easing.OutQuint);
            else
                FadeContainer?.FadeColour(Color4.White, 800, Easing.OutQuint);
        }

        public BackgroundScreenBeatmap(WorkingBeatmap beatmap = null)
        {
            Beatmap = beatmap;
            UpdateDim = new Bindable<bool>();
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
