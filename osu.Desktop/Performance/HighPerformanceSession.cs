// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game;
using osu.Game.Configuration;

namespace osu.Desktop.Performance
{
    public class HighPerformanceSession : Component
    {
        private Bindable<bool> localUserPlaying;

        [BackgroundDependencyLoader]
        private void load(OsuGame game, OsuConfigManager config)
        {
            localUserPlaying = game.LocalUserPlaying.GetBoundCopy();
            localUserPlaying.BindValueChanged(e => onPlayerStateChange(e.NewValue));
        }

        private void onPlayerStateChange(bool state)
        {
            if (state)
                enableHighPerformanceSession();
            else
                disableHighPerformanceSession();
        }

        private void enableHighPerformanceSession()
        {
            GamemodeRequest.RequestStart();
        }

        private void disableHighPerformanceSession()
        {
            GamemodeRequest.RequestEnd();
        }
    }
}
