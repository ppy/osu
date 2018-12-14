// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using Android.Content.PM;

namespace osu.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Landscape, SupportsPictureInPicture = false)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }
        public override void OnBackPressed()
        {
            
        }
    }
}
