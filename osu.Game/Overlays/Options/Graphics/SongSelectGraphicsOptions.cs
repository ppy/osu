using osu.Framework;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class SongSelectGraphicsOptions : OptionsSubsection
    {
        protected override string Header => "Song Select";

        private CheckBoxOption showThumbs;
    
        public SongSelectGraphicsOptions()
        {
            Children = new[]
            {
                showThumbs = new CheckBoxOption { LabelText = "Show thumbnails" }
            };
        }
        
        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                showThumbs.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.SongSelectThumbnails);
            }
        }
    }
}