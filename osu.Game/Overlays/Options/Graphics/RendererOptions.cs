using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class RendererOptions : OptionsSubsection
    {
        protected override string Header => "Renderer";

        private CheckBoxOption fpsCounter, reduceDroppedFrames, detectPerformanceIssues;

        public RendererOptions()
        {
            // NOTE: Compatability mode omitted
            Children = new Drawable[]
            {
                new SpriteText { Text = "Frame limiter: TODO dropdown" },
                fpsCounter = new CheckBoxOption { LabelText = "Show FPS counter" },
                reduceDroppedFrames = new CheckBoxOption { LabelText = "Reduce dropped frames" },
                detectPerformanceIssues = new CheckBoxOption { LabelText = "Detect performance issues" },
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                fpsCounter.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.FpsCounter);
                reduceDroppedFrames.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.ForceFrameFlush);
                detectPerformanceIssues.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.DetectPerformanceIssues);
            }
        }
    }
}