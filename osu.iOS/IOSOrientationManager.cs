// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Mobile;
using UIKit;

namespace osu.iOS
{
    public partial class IOSOrientationManager : OrientationManager
    {
        private readonly AppDelegate appDelegate;

        protected override bool IsCurrentOrientationPortrait => appDelegate.CurrentOrientation.IsPortrait();
        protected override bool IsTablet => UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;

        public IOSOrientationManager(AppDelegate appDelegate)
        {
            this.appDelegate = appDelegate;
        }

        protected override void SetAllowedOrientations(GameOrientation? orientation)
            => appDelegate.Orientations = orientation == null ? null : toUIInterfaceOrientationMask(orientation.Value);

        private UIInterfaceOrientationMask toUIInterfaceOrientationMask(GameOrientation orientation)
        {
            if (orientation == GameOrientation.Locked)
                return (UIInterfaceOrientationMask)(1 << (int)appDelegate.CurrentOrientation);

            if (orientation == GameOrientation.Portrait)
                return UIInterfaceOrientationMask.Portrait;

            if (orientation == GameOrientation.Landscape)
                return UIInterfaceOrientationMask.LandscapeRight;

            if (orientation == GameOrientation.FullPortrait)
                return UIInterfaceOrientationMask.Portrait | UIInterfaceOrientationMask.PortraitUpsideDown;

            return UIInterfaceOrientationMask.Landscape;
        }
    }
}
