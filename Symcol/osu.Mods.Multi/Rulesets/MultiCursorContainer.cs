using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;

namespace osu.Mods.Multi.Rulesets
{
    public class MultiCursorContainer : CursorContainer
    {
        public bool Slave { get; set; }

        public virtual MultiCursorContainer CreateMultiCursor() => null;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (Slave) return false;
            return base.OnMouseMove(e);
        }
    }
}
