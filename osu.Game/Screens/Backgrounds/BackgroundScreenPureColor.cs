using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osuTK.Graphics;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenPureColor : BackgroundScreen
    {
        public readonly Color4 BackgroundColor;

        public BackgroundScreenPureColor(Color4 color = new Color4())
        {
            InternalChild = new Box
            {
                Colour = color,
                RelativeSizeAxes = Axes.Both,
            };

            BackgroundColor = color;
        }

        public override bool Equals(BackgroundScreen other)
        {
            if (!(other is BackgroundScreenPureColor otherBeatmapBackground)) return false;

            return BackgroundColor == otherBeatmapBackground.BackgroundColor;
        }

        public override void OnSuspending(IScreen next)
        {
            this.FadeOut(500, Easing.OutExpo);
            base.OnSuspending(next);
        }
    }
}
