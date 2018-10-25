using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.States;

namespace osu.Mods.Multi.Rulesets
{
    public class MultiCursorContainer : CursorContainer
    {
        public bool Slave { get; set; }

        public virtual MultiCursorContainer CreateMultiCursor() => null;

        protected override bool OnMouseMove(InputState state)
        {
            if (Slave) return false;
            return base.OnMouseMove(state);
        }
    }
}
