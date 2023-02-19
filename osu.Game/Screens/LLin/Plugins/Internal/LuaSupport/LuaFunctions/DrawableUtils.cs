using osu.Framework.Graphics;

namespace osu.Game.Screens.LLin.Plugins.Internal.LuaSupport.LuaFunctions
{
    public class DrawableUtils
    {
        public void MoveToX(Drawable drawable, float dest, float duration = 0, Easing easing = Easing.None)
        {
            drawable.MoveToX(dest, duration, easing);
        }

        public void MoveToY(Drawable drawable, float dest, float duration = 0, Easing easing = Easing.None)
        {
            drawable.MoveToY(dest, duration, easing);
        }

        public void FadeTo(Drawable drawable, float target, float duration = 0, Easing easing = Easing.None)
        {
            drawable.FadeTo(target, duration, easing);
        }

        public void ScaleTo(Drawable drawable, float target, float duration = 0, Easing easing = Easing.None)
        {
            drawable.ScaleTo(target, duration, easing);
        }

        public void FadeIn(Drawable drawable, float duration = 0, Easing easing = Easing.None)
        {
            drawable.FadeIn(duration, easing);
        }

        public void FadeOut(Drawable drawable, float duration = 0, Easing easing = Easing.None)
        {
            drawable.FadeOut(duration, easing);
        }
    }
}
