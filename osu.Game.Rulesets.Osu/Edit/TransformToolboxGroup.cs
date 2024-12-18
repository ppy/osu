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
        private readonly AggregateBindable<bool> canRotate = new AggregateBindable<bool>((x, y) => x || y);
        private readonly AggregateBindable<bool> canScale = new AggregateBindable<bool>((x, y) => x || y);

        private EditorToolButton rotateButton = null!;
        private EditorToolButton scaleButton = null!;

        public SelectionRotationHandler RotationHandler { get; init; } = null!;
        public OsuSelectionScaleHandler ScaleHandler { get; init; } = null!;

        public OsuGridToolboxGroup GridToolbox { get; init; } = null!;

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
                        () => new PreciseRotationPopover(RotationHandler, GridToolbox)),
                    scaleButton = new EditorToolButton("Scale",
                        () => new SpriteIcon { Icon = FontAwesome.Solid.ArrowsAlt },
                        () => new PreciseScalePopover(ScaleHandler, GridToolbox))
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            canRotate.AddSource(RotationHandler.CanRotateAroundPlayfieldOrigin);
            canRotate.AddSource(RotationHandler.CanRotateAroundSelectionOrigin);

            canScale.AddSource(ScaleHandler.CanScaleX);
            canScale.AddSource(ScaleHandler.CanScaleY);
            canScale.AddSource(ScaleHandler.CanScaleFromPlayfieldOrigin);

            // bindings to `Enabled` on the buttons are decoupled on purpose
            // due to the weird `OsuButton` behaviour of resetting `Enabled` to `false` when `Action` is set.
            canRotate.Result.BindValueChanged(rotate => rotateButton.Enabled.Value = rotate.NewValue, true);
            canScale.Result.BindValueChanged(scale => scaleButton.Enabled.Value = scale.NewValue, true);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat) return false;

            switch (e.Action)
            {
                case GlobalAction.EditorToggleRotateControl:
                {
                    if (!RotationHandler.OperationInProgress.Value || rotateButton.Selected.Value)
                        rotateButton.TriggerClick();
                    return true;
                }

                case GlobalAction.EditorToggleScaleControl:
                {
                    if (!ScaleHandler.OperationInProgress.Value || scaleButton.Selected.Value)
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
