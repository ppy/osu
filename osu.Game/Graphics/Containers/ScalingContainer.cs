// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// Handles user-defined scaling, allowing application at multiple levels defined by <see cref="ScalingMode"/>.
    /// </summary>
    public class ScalingContainer : Container
    {
        private Bindable<float> sizeX;
        private Bindable<float> sizeY;
        private Bindable<float> posX;
        private Bindable<float> posY;

        private readonly ScalingMode? targetMode;

        private Bindable<ScalingMode> scalingMode;

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        private readonly Container sizableContainer;

        private BackgroundScreenStack backgroundStack;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="targetMode">The mode which this container should be handling. Handles all modes if null.</param>
        public ScalingContainer(ScalingMode? targetMode = null)
        {
            this.targetMode = targetMode;
            RelativeSizeAxes = Axes.Both;

            InternalChild = sizableContainer = new AlwaysInputContainer
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Both,
                CornerRadius = 10,
                Child = content = new ScalingDrawSizePreservingFillContainer(targetMode != ScalingMode.Gameplay)
            };
        }

        private class ScalingDrawSizePreservingFillContainer : DrawSizePreservingFillContainer
        {
            private readonly bool applyUIScale;
            private Bindable<float> uiScale;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            public ScalingDrawSizePreservingFillContainer(bool applyUIScale)
            {
                this.applyUIScale = applyUIScale;
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager osuConfig)
            {
                if (applyUIScale)
                {
                    uiScale = osuConfig.GetBindable<float>(OsuSetting.UIScale);
                    uiScale.BindValueChanged(scaleChanged, true);
                }
            }

            private void scaleChanged(ValueChangedEvent<float> args)
            {
                this.ScaleTo(new Vector2(args.NewValue), 500, Easing.Out);
                this.ResizeTo(new Vector2(1 / args.NewValue), 500, Easing.Out);
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            scalingMode = config.GetBindable<ScalingMode>(OsuSetting.Scaling);
            scalingMode.ValueChanged += _ => updateSize();

            sizeX = config.GetBindable<float>(OsuSetting.ScalingSizeX);
            sizeX.ValueChanged += _ => updateSize();

            sizeY = config.GetBindable<float>(OsuSetting.ScalingSizeY);
            sizeY.ValueChanged += _ => updateSize();

            posX = config.GetBindable<float>(OsuSetting.ScalingPositionX);
            posX.ValueChanged += _ => updateSize();

            posY = config.GetBindable<float>(OsuSetting.ScalingPositionY);
            posY.ValueChanged += _ => updateSize();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateSize();
            sizableContainer.FinishTransforms();
        }

        private bool requiresBackgroundVisible => (scalingMode.Value == ScalingMode.Everything || scalingMode.Value == ScalingMode.ExcludeOverlays) && (sizeX.Value != 1 || sizeY.Value != 1);

        private void updateSize()
        {
            const float fade_time = 500;

            if (targetMode == ScalingMode.Everything)
            {
                // the top level scaling container manages the background to be displayed while scaling.
                if (requiresBackgroundVisible)
                {
                    if (backgroundStack == null)
                    {
                        AddInternal(backgroundStack = new BackgroundScreenStack
                        {
                            Colour = OsuColour.Gray(0.1f),
                            Alpha = 0,
                            Depth = float.MaxValue
                        });

                        backgroundStack.Push(new ScalingBackgroundScreen());
                    }

                    backgroundStack.FadeIn(fade_time);
                }
                else
                    backgroundStack?.FadeOut(fade_time);
            }

            bool scaling = targetMode == null || scalingMode.Value == targetMode;

            var targetSize = scaling ? new Vector2(sizeX.Value, sizeY.Value) : Vector2.One;
            var targetPosition = scaling ? new Vector2(posX.Value, posY.Value) * (Vector2.One - targetSize) : Vector2.Zero;
            bool requiresMasking = scaling && targetSize != Vector2.One;

            if (requiresMasking)
                sizableContainer.Masking = true;

            sizableContainer.MoveTo(targetPosition, 500, Easing.OutQuart);
            sizableContainer.ResizeTo(targetSize, 500, Easing.OutQuart).OnComplete(_ => { sizableContainer.Masking = requiresMasking; });
        }

        private class ScalingBackgroundScreen : BackgroundScreenDefault
        {
            public override void OnEntering(IScreen last)
            {
                this.FadeInFromZero(4000, Easing.OutQuint);
            }
        }

        private class AlwaysInputContainer : Container
        {
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            public AlwaysInputContainer()
            {
                RelativeSizeAxes = Axes.Both;
            }
        }
    }
}
