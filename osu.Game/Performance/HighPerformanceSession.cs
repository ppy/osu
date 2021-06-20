// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Performance
{
    public class HighPerformanceSession : Component
    {
        //private readonly IBindable<bool> localUserPlaying = new Bindable<bool>();
        private GCLatencyMode originalGCMode;

        //[BackgroundDependencyLoader]
        //private void load(OsuGame game)
        //{
        //    localUserPlaying.BindTo(game.LocalUserPlaying);
        //}

        protected override void LoadComplete()
        {
            base.LoadComplete();

            originalGCMode = GCSettings.LatencyMode;
            DisableHighPerformanceSession();
            //localUserPlaying.BindValueChanged(playing =>
            //{
            //    DisableHighPerformanceSession();
            //}, true);
        }

        protected virtual void EnableHighPerformanceSession()
        {
            originalGCMode = GCSettings.LatencyMode;
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
        }

        protected virtual void DisableHighPerformanceSession()
        {
            if (GCSettings.LatencyMode == GCLatencyMode.LowLatency)
                GCSettings.LatencyMode = originalGCMode;
        }
    }
}
