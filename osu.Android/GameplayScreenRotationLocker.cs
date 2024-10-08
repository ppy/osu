// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Android.Content.PM;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game;
using osu.Game.Screens.Play;

namespace osu.Android
{
    public partial class GameplayScreenRotationLocker : Component
    {
        private IBindable<LocalUserPlayingState> localUserPlaying = null!;

        [Resolved]
        private OsuGameActivity gameActivity { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(ILocalUserPlayInfo localUserPlayInfo)
        {
            localUserPlaying = localUserPlayInfo.PlayingState.GetBoundCopy();
            localUserPlaying.BindValueChanged(updateLock, true);
        }

        private void updateLock(ValueChangedEvent<LocalUserPlayingState> userPlaying)
        {
            gameActivity.RunOnUiThread(() =>
            {
                gameActivity.RequestedOrientation = userPlaying.NewValue != LocalUserPlayingState.NotPlaying ? ScreenOrientation.Locked : gameActivity.DefaultOrientation;
            });
        }
    }
}
