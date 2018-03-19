using osu.Framework.Input;
using OpenTK;
using OpenTK.Input;

namespace Symcol.Core.Graphics.Containers
{
    public class SymcolDragContainer : SymcolContainer
    {
        protected override bool OnDragStart(InputState state) => true;

        public bool AllowLeftClickDrag { get; set; } = true;

        private bool drag;

        private Vector2 startPosition;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            startPosition = Position;

            if (args.Button == MouseButton.Left && AllowLeftClickDrag || args.Button == MouseButton.Right)
                drag = true;

            return base.OnMouseDown(state, args);
        }

        protected override bool OnDrag(InputState state)
        {
            if (drag)
                Position = startPosition + state.Mouse.Position - state.Mouse.PositionMouseDown.GetValueOrDefault();

            return base.OnDrag(state);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (args.Button == MouseButton.Left && AllowLeftClickDrag || args.Button == MouseButton.Right)
                drag = false;

            return base.OnMouseUp(state, args);
        }
    }
}
