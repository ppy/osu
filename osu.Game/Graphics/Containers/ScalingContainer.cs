// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Layout;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// Handles user-defined scaling, allowing application at multiple levels defined by <see cref="ScalingMode"/>.
    /// </summary>
    public partial class ScalingContainer : Container
    {
        internal const float TRANSITION_DURATION = 500;

        private Bindable<float> sizeX;
        private Bindable<float> sizeY;
        private Bindable<float> posX;
        private Bindable<float> posY;
        private Bindable<bool> applySafeAreaPadding;

        private Bindable<MarginPadding> safeAreaPadding;

        private readonly ScalingMode? targetMode;

        private Bindable<ScalingMode> scalingMode;

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        private readonly Container sizableContainer;

        private BackgroundScreenStack backgroundStack;

        private Bindable<float> scalingMenuBackgroundDim;

        private RectangleF? customRect;
        private bool customRectIsRelativePosition;

        /// <summary>
        /// Set a custom position and scale which overrides any user specification.
        /// </summary>
        /// <param name="rect">A rectangle with positional and sizing information for this container to conform to. <c>null</c> will clear the custom rect and revert to user settings.</param>
        /// <param name="relativePosition">Whether the position portion of the provided rect is in relative coordinate space or not.</param>
        public void SetCustomRect(RectangleF? rect, bool relativePosition = false)
        {
            customRect = rect;
            customRectIsRelativePosition = relativePosition;

            if (IsLoaded) Scheduler.AddOnce(updateSize);
        }

        private const float corner_radius = 10;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="targetMode">The mode which this container should be handling. Handles all modes if null.</param>
        public ScalingContainer(ScalingMode? targetMode = null)
        {
            this.targetMode = targetMode;
            RelativeSizeAxes = Axes.Both;

            InternalChild = sizableContainer = new SizeableAlwaysInputContainer(targetMode == ScalingMode.Everything)
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Both,
                CornerRadius = corner_radius,
                Child = content = new ScalingDrawSizePreservingFillContainer(targetMode != ScalingMode.Gameplay)
            };
        }

        public partial class ScalingDrawSizePreservingFillContainer : DrawSizePreservingFillContainer
        {
            private readonly bool applyUIScale;
            private Bindable<float> uiScale;

            protected float CurrentScale { get; private set; } = 1;

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
                    uiScale.BindValueChanged(args => this.TransformTo(nameof(CurrentScale), args.NewValue, TRANSITION_DURATION, Easing.OutQuart), true);
                }
            }

            protected override void Update()
            {
                Scale = new Vector2(CurrentScale);
                Size = new Vector2(1 / CurrentScale);

                base.Update();
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, ISafeArea safeArea)
        {
            scalingMode = config.GetBindable<ScalingMode>(OsuSetting.Scaling);
            scalingMode.ValueChanged += _ => Scheduler.AddOnce(updateSize);

            sizeX = config.GetBindable<float>(OsuSetting.ScalingSizeX);
            sizeX.ValueChanged += _ => Scheduler.AddOnce(updateSize);

            sizeY = config.GetBindable<float>(OsuSetting.ScalingSizeY);
            sizeY.ValueChanged += _ => Scheduler.AddOnce(updateSize);

            posX = config.GetBindable<float>(OsuSetting.ScalingPositionX);
            posX.ValueChanged += _ => Scheduler.AddOnce(updateSize);

            posY = config.GetBindable<float>(OsuSetting.ScalingPositionY);
            posY.ValueChanged += _ => Scheduler.AddOnce(updateSize);

            applySafeAreaPadding = config.GetBindable<bool>(OsuSetting.SafeAreaConsiderations);
            applySafeAreaPadding.BindValueChanged(_ => Scheduler.AddOnce(updateSize));

            safeAreaPadding = safeArea.SafeAreaPadding.GetBoundCopy();
            safeAreaPadding.BindValueChanged(_ => Scheduler.AddOnce(updateSize));

            scalingMenuBackgroundDim = config.GetBindable<float>(OsuSetting.ScalingBackgroundDim);
            scalingMenuBackgroundDim.ValueChanged += _ => Scheduler.AddOnce(updateSize);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateSize();
            sizableContainer.FinishTransforms();
        }

        private bool requiresBackgroundVisible => (scalingMode.Value == ScalingMode.Everything || scalingMode.Value == ScalingMode.ExcludeOverlays)
                                                  && (sizeX.Value != 1 || sizeY.Value != 1)
                                                  && scalingMenuBackgroundDim.Value < 1;

        private void updateSize()
        {
            if (targetMode == ScalingMode.Everything)
            {
                // the top level scaling container manages the background to be displayed while scaling.
                if (requiresBackgroundVisible)
                {
                    if (backgroundStack == null)
                    {
                        AddInternal(backgroundStack = new BackgroundScreenStack
                        {
                            Alpha = 0,
                            Colour = Color4.Black,
                            Depth = float.MaxValue
                        });

                        backgroundStack.Push(new ScalingBackgroundScreen());
                    }

                    backgroundStack.FadeIn(TRANSITION_DURATION);
                    backgroundStack.FadeColour(OsuColour.Gray(1.0f - scalingMenuBackgroundDim.Value), TRANSITION_DURATION, Easing.OutQuint);
                }
                else
                    backgroundStack?.FadeOut(TRANSITION_DURATION);
            }

            RectangleF targetRect = new RectangleF(Vector2.Zero, Vector2.One);

            if (customRect != null)
            {
                sizableContainer.RelativePositionAxes = customRectIsRelativePosition ? Axes.Both : Axes.None;

                targetRect = customRect.Value;
            }
            else if (targetMode == null || scalingMode.Value == targetMode)
            {
                sizableContainer.RelativePositionAxes = Axes.Both;

                Vector2 scale = new Vector2(sizeX.Value, sizeY.Value);
                Vector2 pos = new Vector2(posX.Value, posY.Value) * (Vector2.One - scale);

                targetRect = new RectangleF(pos, scale);
            }

            bool requiresMasking = targetRect.Size != Vector2.One
                                   // For the top level scaling container, for now we apply masking if safe areas are in use.
                                   // In the future this can likely be removed as more of the actual UI supports overflowing into the safe areas.
                                   || (targetMode == ScalingMode.Everything && (applySafeAreaPadding.Value && safeAreaPadding.Value.Total != Vector2.Zero));

            if (requiresMasking)
                sizableContainer.Masking = true;

            sizableContainer.MoveTo(targetRect.Location, TRANSITION_DURATION, Easing.OutQuart);
            sizableContainer.ResizeTo(targetRect.Size, TRANSITION_DURATION, Easing.OutQuart);

            // Of note, this will not work great in the case of nested ScalingContainers where multiple are applying corner radius.
            // Masking and corner radius should likely only be applied at one point in the full game stack to fix this.
            // An example of how this can occur is when the skin editor is visible and the game screen scaling is set to "Everything".
            sizableContainer.TransformTo(nameof(CornerRadius), requiresMasking ? corner_radius : 0, TRANSITION_DURATION, requiresMasking ? Easing.OutQuart : Easing.None)
                            .OnComplete(_ => { sizableContainer.Masking = requiresMasking; });
        }

        private partial class ScalingBackgroundScreen : BackgroundScreenDefault
        {
            protected override bool AllowStoryboardBackground => false;

            public override void OnEntering(ScreenTransitionEvent e)
            {
                this.FadeInFromZero(4000, Easing.OutQuint);
            }
        }

        private partial class SizeableAlwaysInputContainer : Container
        {
            [Resolved]
            private GameHost host { get; set; }

            [Resolved]
            private ISafeArea safeArea { get; set; }

            [Resolved]
            private OsuConfigManager config { get; set; }

            private readonly bool confineHostCursor;
            private readonly LayoutValue cursorRectCache = new LayoutValue(Invalidation.RequiredParentSizeToFit);

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            /// <summary>
            /// Container used for sizing/positioning purposes in <see cref="ScalingContainer"/>. Always receives mouse input.
            /// </summary>
            /// <param name="confineHostCursor">Whether to confine the host cursor to the draw area of this container.</param>
            /// <remarks>Cursor confinement will abide by the <see cref="OsuSetting.ConfineMouseMode"/> setting.</remarks>
            public SizeableAlwaysInputContainer(bool confineHostCursor)
            {
                RelativeSizeAxes = Axes.Both;
                this.confineHostCursor = confineHostCursor;

                if (confineHostCursor)
                    AddLayout(cursorRectCache);
            }

            protected override void Update()
            {
                base.Update();

                if (confineHostCursor && !cursorRectCache.IsValid)
                {
                    updateHostCursorConfineRect();
                    cursorRectCache.Validate();
                }
            }

            private void updateHostCursorConfineRect()
            {
                if (host.Window == null) return;

                bool coversWholeScreen = Size == Vector2.One && (!config.Get<bool>(OsuSetting.SafeAreaConsiderations) || safeArea.SafeAreaPadding.Value.Total == Vector2.Zero);
                host.Window.CursorConfineRect = coversWholeScreen ? null : ToScreenSpace(DrawRectangle).AABBFloat;
            }
        }
    }
}
