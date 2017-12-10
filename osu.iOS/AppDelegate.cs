extern alias IOS;

using osu.Framework.Platform.iOS;
using osu.Game;
using IOS::Foundation;
using IOS::UIKit;
using OpenTK;
using IOS::System.Drawing;

namespace osu.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        private iOSPlatformGameView gameView;

        public override UIWindow Window
        {
            get;
            set;
        }

        private iOSGameHost host;

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            gameView = new iOSPlatformGameView(new IOS::System.Drawing.RectangleF(0.0f, 0.0f, (float)Window.Frame.Size.Width, (float)Window.Frame.Size.Height));

            UIViewController viewController = new UIViewController();
            viewController.View = gameView;

            Window.RootViewController = viewController;
            Window.MakeKeyAndVisible();

            gameView.Run(60.0);

            host = new iOSGameHost(gameView);
            host.Run(new OsuGame());

            return true;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
        {
            return UIInterfaceOrientationMask.Landscape;
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message)
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive.
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}

