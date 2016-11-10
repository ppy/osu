using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Gameplay
{
    public class GeneralGameplayOptions : OptionsSubsection
    {
        protected override string Header => "General";

        [Initializer]
        private void Load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "Background dim: TODO slider" },
                new SpriteText { Text = "Progress display: TODO dropdown" },
                new SpriteText { Text = "Score meter type: TODO dropdown" },
                new SpriteText { Text = "Score meter size: TODO slider" },
                new CheckBoxOption
                {
                    LabelText = "Always show key overlay",
                    Bindable = config.GetBindable<bool>(OsuConfig.KeyOverlay)
                },
                new CheckBoxOption
                {
                    LabelText = "Show approach circle on first \"Hidden\" object",
                    Bindable = config.GetBindable<bool>(OsuConfig.HiddenShowFirstApproach)
                },
                new CheckBoxOption
                {
                    LabelText = "Scale osu!mania scroll speed with BPM",
                    Bindable = config.GetBindable<bool>(OsuConfig.ManiaSpeedBPMScale)
                },
                new CheckBoxOption
                {
                    LabelText = "Remember osu!mania scroll speed per beatmap",
                    Bindable = config.GetBindable<bool>(OsuConfig.UsePerBeatmapManiaSpeed)
                },
            };
        }
    }
}