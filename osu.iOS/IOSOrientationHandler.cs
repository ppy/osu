// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game;
using osu.Game.Screens.Play;
using UIKit;

namespace osu.iOS
{
    public partial class IOSOrientationHandler : Component
    {
        private readonly AppDelegate appDelegate;

        [Resolved]
        private OsuGame game { get; set; } = null!;

        [Resolved]
        private ILocalUserPlayInfo localUserPlayInfo { get; set; } = null!;

        private IBindable<bool> requiresPortraitOrientation = null!;
        private IBindable<LocalUserPlayingState> localUserPlaying = null!;

        public IOSOrientationHandler(AppDelegate appDelegate)
        {
            this.appDelegate = appDelegate;
        }

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
            UIInterfaceOrientation currentOrientation = appDelegate.CurrentOrientation;
            bool lockCurrentOrientation = localUserPlaying.Value == LocalUserPlayingState.Playing;
            bool lockToPortrait = requiresPortraitOrientation.Value;
            bool isPhone = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone;

            if (lockCurrentOrientation)
            {
                if (lockToPortrait && !currentOrientation.IsPortrait())
                    currentOrientation = UIInterfaceOrientation.Portrait;
                else if (!lockToPortrait && currentOrientation.IsPortrait() && isPhone)
                    currentOrientation = UIInterfaceOrientation.LandscapeRight;

                appDelegate.Orientations = (UIInterfaceOrientationMask)(1 << (int)currentOrientation);
                return;
            }

            if (lockToPortrait)
            {
                UIInterfaceOrientationMask portraitOrientations = UIInterfaceOrientationMask.Portrait;

                if (!isPhone)
                    portraitOrientations |= UIInterfaceOrientationMask.PortraitUpsideDown;

                appDelegate.Orientations = portraitOrientations;
                return;
            }

            appDelegate.Orientations = null;
        }
    }
}
