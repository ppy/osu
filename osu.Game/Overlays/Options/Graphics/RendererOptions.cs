using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class RendererOptions : OptionsSubsection
    {
        protected override string Header => "Renderer";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            // NOTE: Compatability mode omitted
            Children = new Drawable[]
            {
                new SpriteText { Text = "Frame limiter: TODO dropdown" },
                new CheckBoxOption
                {
                    LabelText = "Show FPS counter",
                    Bindable = config.GetBindable<bool>(OsuConfig.FpsCounter),
                },
                new CheckBoxOption
                {
                    LabelText = "Reduce dropped frames",
                    Bindable = config.GetBindable<bool>(OsuConfig.ForceFrameFlush),
                },
                new CheckBoxOption
                {
                    LabelText = "Detect performance issues",
                    Bindable = config.GetBindable<bool>(OsuConfig.DetectPerformanceIssues),
                },
            };
        }
    }
}