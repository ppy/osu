using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Desktop.KeyCounterTutorial
{
    internal class KeyboardCount : Count
    {
        public Key CounterKey { get; }

        public KeyboardCount(string name, Key keyboardKey)
            : base(name)
        {
            CounterKey = keyboardKey;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (IsCounting && args.Key == CounterKey)
            {
                ++Value;
                IsLit = true;
            }
            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            if (IsCounting && args.Key == CounterKey)
                IsLit = false;
            return base.OnKeyUp(state, args);
        }
    }
}