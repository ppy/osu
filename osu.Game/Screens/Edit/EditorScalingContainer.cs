// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Edit
{
    public partial class EditorScalingContainer : ScalingContainer.ScalingDrawSizePreservingFillContainer
    {
        private float absoluteScaleFactor { get; set; }

        private readonly Bindable<bool> useNativeResolution = new Bindable<bool>();

        public EditorScalingContainer()
            : base(true)
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager? config)
        {
            config?.BindWith(OsuSetting.EditorUseNativeResolution, useNativeResolution);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            useNativeResolution.BindValueChanged(e =>
            {
                this.TransformTo(nameof(absoluteScaleFactor), e.NewValue ? 1f : 0f, ScalingContainer.TRANSITION_DURATION, Easing.OutQuart);
            }, true);
            FinishTransforms();
        }

        protected override void Update()
        {
            float inverseParentScale = Parent!.DrawInfo.MatrixInverse.ExtractScale().X;

            float scale = CurrentScale * float.Lerp(1, inverseParentScale * 1.5f, absoluteScaleFactor);

            Scale = new Vector2(scale);
            Size = new Vector2(1 / scale);
        }
    }
}
