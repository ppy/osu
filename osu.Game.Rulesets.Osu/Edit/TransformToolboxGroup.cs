// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class TransformToolboxGroup : EditorToolboxGroup, IKeyBindingHandler<GlobalAction>
    {
        private readonly Bindable<bool> canRotate = new BindableBool();
        private readonly Bindable<bool> canScale = new BindableBool();

        private EditorToolButton rotateButton = null!;
        private EditorToolButton scaleButton = null!;

        private Bindable<bool> canRotatePlayfieldOrigin = null!;
        private Bindable<bool> canRotateSelectionOrigin = null!;

        private Bindable<bool> canScaleX = null!;
        private Bindable<bool> canScaleY = null!;
        private Bindable<bool> canScalePlayfieldOrigin = null!;

        public SelectionRotationHandler RotationHandler { get; init; } = null!;
        public OsuSelectionScaleHandler ScaleHandler { get; init; } = null!;

        public TransformToolboxGroup()
            : base("transform")
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    rotateButton = new EditorToolButton("Rotate",
                        () => new SpriteIcon { Icon = FontAwesome.Solid.Undo },
                        () => new PreciseRotationPopover(RotationHandler)),
                    scaleButton = new EditorToolButton("Scale",
                        () => new SpriteIcon { Icon = FontAwesome.Solid.ArrowsAlt },
                        () => new PreciseScalePopover(ScaleHandler))
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // aggregate two values into canRotate
            canRotatePlayfieldOrigin = RotationHandler.CanRotateAroundPlayfieldOrigin.GetBoundCopy();
            canRotatePlayfieldOrigin.BindValueChanged(_ => updateCanRotateAggregate());

            canRotateSelectionOrigin = RotationHandler.CanRotateAroundSelectionOrigin.GetBoundCopy();
            canRotateSelectionOrigin.BindValueChanged(_ => updateCanRotateAggregate());

            void updateCanRotateAggregate()
            {
                canRotate.Value = RotationHandler.CanRotateAroundPlayfieldOrigin.Value || RotationHandler.CanRotateAroundSelectionOrigin.Value;
            }

            // aggregate three values into canScale
            canScaleX = ScaleHandler.CanScaleX.GetBoundCopy();
            canScaleX.BindValueChanged(_ => updateCanScaleAggregate());

            canScaleY = ScaleHandler.CanScaleY.GetBoundCopy();
            canScaleY.BindValueChanged(_ => updateCanScaleAggregate());

            canScalePlayfieldOrigin = ScaleHandler.CanScaleFromPlayfieldOrigin.GetBoundCopy();
            canScalePlayfieldOrigin.BindValueChanged(_ => updateCanScaleAggregate());

            void updateCanScaleAggregate()
            {
                canScale.Value = ScaleHandler.CanScaleX.Value || ScaleHandler.CanScaleY.Value || ScaleHandler.CanScaleFromPlayfieldOrigin.Value;
            }

            // bindings to `Enabled` on the buttons are decoupled on purpose
            // due to the weird `OsuButton` behaviour of resetting `Enabled` to `false` when `Action` is set.
            canRotate.BindValueChanged(_ => rotateButton.Enabled.Value = canRotate.Value, true);
            canScale.BindValueChanged(_ => scaleButton.Enabled.Value = canScale.Value, true);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat) return false;

            switch (e.Action)
            {
                case GlobalAction.EditorToggleRotateControl:
                {
                    rotateButton.TriggerClick();
                    return true;
                }

                case GlobalAction.EditorToggleScaleControl:
                {
                    scaleButton.TriggerClick();
                    return true;
                }
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
