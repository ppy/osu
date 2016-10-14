using osu.Framework;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays
{
    /// <summary>
    /// An element which starts hidden and can be toggled to visible.
    /// </summary>
    public abstract class Overlay : Container, IStateful<Visibility>
    {
        public override void Load(BaseGame game)
        {
            base.Load(game);

            //TODO: init code using Alpha or IsVisible override to ensure we don't call Load on children before we first get unhidden.
            PopOut();
            Flush();
        }

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

        public void ToggleVisibility() => State = (State == Visibility.Visible ? Visibility.Hidden : Visibility.Visible);
    }

    public enum Visibility
    {
        Hidden,
        Visible
    }
}
