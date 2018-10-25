using osu.Game.Overlays.Toolbar;
using osu.Game.Screens;
using System;
using osu.Framework.Platform;

namespace osu.Game.ModLoader
{
    public abstract class SymcolBaseSet : IDisposable
    {
        public abstract OsuScreen GetMenuScreen();

        public virtual Toolbar GetToolbar() => null;

        public virtual void LoadComplete(OsuGame game, GameHost host) { }

        public virtual void Dispose() { }
    }
}
