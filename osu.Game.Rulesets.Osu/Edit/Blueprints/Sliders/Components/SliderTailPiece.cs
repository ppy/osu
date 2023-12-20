// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
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

        private readonly Cached<SliderPath> fullPathCache = new Cached<SliderPath>();

        [Resolved(CanBeNull = true)]
        private EditorBeatmap? editorBeatmap { get; set; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public SliderTailPiece(Slider slider, SliderPosition position)
            : base(slider, position)
        {
            Slider.Path.ControlPoints.CollectionChanged += (_, _) => fullPathCache.Invalidate();
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
            double oldDistance = Slider.Path.Distance;
            double proposedDistance = findClosestPathDistance(e);

            proposedDistance = MathHelper.Clamp(proposedDistance, 0, Slider.Path.CalculatedDistance);
            proposedDistance = MathHelper.Clamp(proposedDistance,
                0.1 * oldDistance / Slider.SliderVelocityMultiplier,
                10 * oldDistance / Slider.SliderVelocityMultiplier);

            if (Precision.AlmostEquals(proposedDistance, oldDistance))
                return;

            Slider.SliderVelocityMultiplier *= proposedDistance / oldDistance;
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

        /// <summary>
        /// Finds the expected distance value for which the slider end is closest to the mouse position.
        /// </summary>
        private double findClosestPathDistance(DragEvent e)
        {
            const double step1 = 10;
            const double step2 = 0.1;

            var desiredPosition = e.MousePosition - Slider.Position;

            if (!fullPathCache.IsValid)
                fullPathCache.Value = new SliderPath(Slider.Path.ControlPoints.ToArray());

            // Do a linear search to find the closest point on the path to the mouse position.
            double bestValue = 0;
            double minDistance = double.MaxValue;

            for (double d = 0; d <= fullPathCache.Value.CalculatedDistance; d += step1)
            {
                double t = d / fullPathCache.Value.CalculatedDistance;
                float dist = Vector2.Distance(fullPathCache.Value.PositionAt(t), desiredPosition);

                if (dist >= minDistance) continue;

                minDistance = dist;
                bestValue = d;
            }

            // Do another linear search to fine-tune the result.
            for (double d = bestValue - step1; d <= bestValue + step1; d += step2)
            {
                double t = d / fullPathCache.Value.CalculatedDistance;
                float dist = Vector2.Distance(fullPathCache.Value.PositionAt(t), desiredPosition);

                if (dist >= minDistance) continue;

                minDistance = dist;
                bestValue = d;
            }

            return bestValue;
        }
    }
}
