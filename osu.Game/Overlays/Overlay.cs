using osu.Framework;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays
{
    public abstract class Overlay : Container, IStateful<Visibility>
    {
        private Visibility state;
        public Visibility State
        {
            get { return state; }
            set
            {
                if (value == state) return;
                state = value;

                switch (value)
                {
                    case Visibility.Hidden:
                        PopOut();
                        break;
                    case Visibility.Visible:
                        PopIn();
                        break;
                }
            }
        }

        protected abstract void PopIn();

        protected abstract void PopOut();

        public void ReverseVisibility()
            => State = (State == Visibility.Visible ? Visibility.Hidden : Visibility.Visible);
    }
    public enum Visibility
    {
        Hidden,
        Visible
    }
}
