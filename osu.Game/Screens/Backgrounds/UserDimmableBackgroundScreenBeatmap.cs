// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Screens.Backgrounds
{
    public class UserDimmableBackgroundScreenBeatmap : BackgroundScreenBeatmap
    {
        protected Bindable<double> DimLevel;
        protected float BackgroundOpacity => 1 - (float)DimLevel;
        private Container fadeContainer;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            DimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            fadeContainer = new Container { RelativeSizeAxes = Axes.Both};
        }

        protected override void AddBackground(Drawable d)
        {
            fadeContainer.Child = d;
            InternalChild = fadeContainer;
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            DimLevel.ValueChanged += _ => updateBackgroundDim();
            updateBackgroundDim();
        }
        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);
            updateBackgroundDim();
        }

        public UserDimmableBackgroundScreenBeatmap(WorkingBeatmap beatmap = null)
            :base(beatmap)
        {
        }

        private void updateBackgroundDim()
        {
            fadeContainer?.FadeColour(OsuColour.Gray(BackgroundOpacity), 800, Easing.OutQuint);
        }
    }
}
