// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public partial class SliderTailPiece : SliderCircleOverlay
    {
        /// <summary>
        /// Whether this is currently being dragged.
        /// </summary>
        private bool isDragging;

        private InputManager inputManager = null!;

        [Resolved(CanBeNull = true)]
        private EditorBeatmap? editorBeatmap { get; set; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public SliderTailPiece(Slider slider, SliderPosition position)
            : base(slider, position)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => CirclePiece.ReceivePositionalInputAt(screenSpacePos);

        protected override bool OnHover(HoverEvent e)
        {
            updateCirclePieceColour();
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateCirclePieceColour();
        }

        private void updateCirclePieceColour()
        {
            Color4 colour = colours.Yellow;

            if (IsHovered)
                colour = colour.Lighten(1);

            CirclePiece.Colour = colour;
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (e.Button == MouseButton.Right || !inputManager.CurrentState.Keyboard.ShiftPressed)
                return false;

            isDragging = true;
            editorBeatmap?.BeginChange();

            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            double proposedDistance = Slider.Path.Distance + e.Delta.X;

            proposedDistance = MathHelper.Clamp(proposedDistance, 0, Slider.Path.CalculatedDistance);
            proposedDistance = MathHelper.Clamp(proposedDistance,
                0.1 * Slider.Path.Distance / Slider.SliderVelocityMultiplier,
                10 * Slider.Path.Distance / Slider.SliderVelocityMultiplier);

            if (Precision.AlmostEquals(proposedDistance, Slider.Path.Distance))
                return;

            Slider.SliderVelocityMultiplier *= proposedDistance / Slider.Path.Distance;
            Slider.Path.ExpectedDistance.Value = proposedDistance;
            editorBeatmap?.Update(Slider);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            if (isDragging)
            {
                editorBeatmap?.EndChange();
            }
        }
    }
}
