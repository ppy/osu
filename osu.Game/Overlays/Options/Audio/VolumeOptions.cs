using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Audio
{
    public class VolumeOptions : OptionsSubsection
    {
        protected override string Header => "Volume";

        private CheckBoxOption ignoreHitsounds;

        public VolumeOptions()
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "Master: TODO slider" },
                new SpriteText { Text = "Music: TODO slider" },
                new SpriteText { Text = "Effect: TODO slider" },
                ignoreHitsounds = new CheckBoxOption { LabelText = "Ignore beatmap hitsounds" }
            };
        }
        
        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                ignoreHitsounds.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.IgnoreBeatmapSamples);
            }
        }
    }
}