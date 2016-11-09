using OpenTK;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class EditorSection : OptionsSection
    {
        protected override string Header => "Editor";
        public override FontAwesome Icon => FontAwesome.fa_pencil;

        private CheckBoxOption backgroundVideo, defaultSkin, snakingSliders, hitAnimations, followPoints, stacking;

        public EditorSection()
        {
            content.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                backgroundVideo = new CheckBoxOption { LabelText = "Background video" },
                defaultSkin = new CheckBoxOption { LabelText = "Always use default skin" },
                snakingSliders = new CheckBoxOption { LabelText = "Snaking sliders" },
                hitAnimations = new CheckBoxOption { LabelText = "Hit animations" },
                followPoints = new CheckBoxOption { LabelText = "Follow points" },
                stacking = new CheckBoxOption { LabelText = "Stacking" },
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                backgroundVideo.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.VideoEditor);
                defaultSkin.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.EditorDefaultSkin);
                snakingSliders.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.EditorSnakingSliders);
                hitAnimations.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.EditorHitAnimations);
                followPoints.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.EditorFollowPoints);
                stacking.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.EditorStacking);
            }
        }
    }
}

