// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Android.Content.PM;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game;

namespace osu.Android
{
    public class GameplayScreenRotationLocker : Component
    {
        private Bindable<bool> localUserPlaying;

        [Resolved]
        private OsuGameActivity gameActivity { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuGame game)
        {
            localUserPlaying = game.LocalUserPlaying.GetBoundCopy();
            localUserPlaying.BindValueChanged(updateLock, true);
        }

        private void updateLock(ValueChangedEvent<bool> userPlaying)
        {
            gameActivity.RunOnUiThread(() =>
            {
                gameActivity.RequestedOrientation = userPlaying.NewValue ? ScreenOrientation.Locked : gameActivity.DefaultOrientation;
            });
        }
    }
}
