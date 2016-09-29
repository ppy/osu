using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Desktop.KeyCounterTutorial
{
    internal class MouseCount : Count
    {
        public MouseButton CounterMouseButton { get; }

        public MouseCount(string name, MouseButton mouseButton)
            : base(name)
        {
            CounterMouseButton = mouseButton;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (IsCounting && !IsLit && args.Button == CounterMouseButton)
            {
                ++Value;
                IsLit = true;
                UpdateVisualState(IsLit);
            }
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (IsCounting && args.Button == CounterMouseButton)
            {
                IsLit = false;
                UpdateVisualState(IsLit);
            }
            return base.OnMouseUp(state, args);
        }
    }
}