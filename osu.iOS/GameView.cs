extern alias IOS;

using System;
using System.Diagnostics;
using IOS::System.Drawing;

using IOS::Foundation;
using IOS::GLKit;
using IOS::OpenGLES;
using IOS::ObjCRuntime;
using IOS::CoreAnimation;
using IOS::CoreGraphics;
using IOS::UIKit;
using OpenTK;
using OpenTK.Graphics.ES20;
using OpenTK.Platform.iPhoneOS;

namespace osu.iOS
{
    [Register("GameView")]
    public class GameView : iPhoneOSGameView
    {

        [Export("layerClass")]
        static Class LayerClass()
        {
            return iPhoneOSGameView.GetLayerClass();
        }

        protected override void ConfigureLayer(CAEAGLLayer eaglLayer)
        {
            eaglLayer.Opaque = true;
        }

        [Export("initWithFrame:")]
        public GameView(IOS::System.Drawing.RectangleF frame) : base(frame)
        {
            LayerRetainsBacking = false;
            LayerColorFormat = EAGLColorFormat.RGBA8;
            ContextRenderingApi = EAGLRenderingAPI.OpenGLES3;
        }
    }
}
