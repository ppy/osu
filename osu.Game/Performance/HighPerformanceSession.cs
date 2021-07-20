// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Performance
{
    public class HighPerformanceSession : Component
    {
        private readonly IBindable<bool> localUserPlaying = new Bindable<bool>();

        [BackgroundDependencyLoader]
        private void load(OsuGame game)
        {
            localUserPlaying.BindTo(game.LocalUserPlaying);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            localUserPlaying.BindValueChanged(playing =>
            {
                if (playing.NewValue)
                    EnableHighPerformanceSession();
                else
                    DisableHighPerformanceSession();
            }, true);
        }

        protected virtual void EnableHighPerformanceSession()
        {
        }

        protected virtual void DisableHighPerformanceSession()
        {
        }
    }
}
