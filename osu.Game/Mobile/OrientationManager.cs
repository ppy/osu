// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.Play;

namespace osu.Game.Mobile
{
    /// <summary>
    /// A <see cref="Component"/> that manages the device orientations a game can display in.
    /// </summary>
    public abstract partial class OrientationManager : Component
    {
        /// <summary>
        /// Whether the current orientation of the game is portrait.
        /// </summary>
        protected abstract bool IsCurrentOrientationPortrait { get; }

        /// <summary>
        /// Whether the mobile device is considered a tablet.
        /// </summary>
        protected abstract bool IsTablet { get; }

        [Resolved]
        private OsuGame game { get; set; } = null!;

        [Resolved]
        private ILocalUserPlayInfo localUserPlayInfo { get; set; } = null!;

        private IBindable<bool> requiresPortraitOrientation = null!;
        private IBindable<LocalUserPlayingState> localUserPlaying = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            requiresPortraitOrientation = game.RequiresPortraitOrientation.GetBoundCopy();
            requiresPortraitOrientation.BindValueChanged(_ => updateOrientations());

            localUserPlaying = localUserPlayInfo.PlayingState.GetBoundCopy();
            localUserPlaying.BindValueChanged(_ => updateOrientations());

            updateOrientations();
        }

        private void updateOrientations()
        {
            bool lockCurrentOrientation = localUserPlaying.Value == LocalUserPlayingState.Playing;
            bool lockToPortraitOnPhone = requiresPortraitOrientation.Value;

            if (lockCurrentOrientation)
            {
                if (!IsTablet && lockToPortraitOnPhone && !IsCurrentOrientationPortrait)
                    SetAllowedOrientations(GameOrientation.Portrait);
                else if (!IsTablet && !lockToPortraitOnPhone && IsCurrentOrientationPortrait)
                    SetAllowedOrientations(GameOrientation.Landscape);
                else
                {
                    // if the orientation is already portrait/landscape according to the game's specifications,
                    // then use Locked instead of Portrait/Landscape to handle the case where the device is
                    // in landscape-left or reverse-portrait.
                    SetAllowedOrientations(GameOrientation.Locked);
                }

                return;
            }

            if (!IsTablet && lockToPortraitOnPhone)
            {
                SetAllowedOrientations(GameOrientation.Portrait);
                return;
            }

            SetAllowedOrientations(null);
        }

        /// <summary>
        /// Sets the allowed orientations the device can rotate to.
        /// </summary>
        /// <param name="orientation">The allowed orientations, or null to return back to default.</param>
        protected abstract void SetAllowedOrientations(GameOrientation? orientation);
    }
}
