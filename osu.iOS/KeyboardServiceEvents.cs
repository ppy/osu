// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using CoreGraphics;
using Foundation;
using UIKit;

namespace osu.iOS
{
    public class KeyboardServiceEvents
    {
        public double KeyboardHeight { get; private set; } = 0;

        public KeyboardServiceEvents()
        {
            // https://code.4noobz.net/xamarin-ios-inputview-taking-into-account-the-keyboard-height/
            // Keyboard popup
            NSNotificationCenter.DefaultCenter.AddObserver
            (UIKeyboard.DidShowNotification, keyboardEvent);

            // Keyboard Down
            NSNotificationCenter.DefaultCenter.AddObserver
            (UIKeyboard.WillHideNotification, keyboardEvent);
        }

        private void keyboardEvent(NSNotification notification)
        {
            // get the keyboard size
            CGRect r = UIKeyboard.BoundsFromNotification(notification);

            KeyboardHeight = r.Height;
        }
    }
}
