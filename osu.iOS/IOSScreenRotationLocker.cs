// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.iOS;
using osu.Framework.Platform;
using osu.Game.Screens.Play;
using UIKit;

namespace osu.iOS
{
    public partial class IOSScreenRotationLocker : Component
    {
        private IBindable<LocalUserPlayingState> localUserPlaying = null!;

        [Resolved]
        private GameHost host { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(ILocalUserPlayInfo localUserPlayInfo)
        {
            localUserPlaying = localUserPlayInfo.PlayingState.GetBoundCopy();
            localUserPlaying.BindValueChanged(updateLock, true);
        }

        private void updateLock(ValueChangedEvent<LocalUserPlayingState> userPlaying)
        {
            var iosAppDelegate = (GameApplicationDelegate)UIApplication.SharedApplication.Delegate;
            iosAppDelegate.LockScreenOrientation = userPlaying.NewValue == LocalUserPlayingState.Playing;
        }
    }
}
