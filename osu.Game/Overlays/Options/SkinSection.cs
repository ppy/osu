using OpenTK;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class SkinSection : OptionsSection
    {
        public override string Header => "Skin";
        public override FontAwesome Icon => FontAwesome.fa_paint_brush;

        private CheckBoxOption ignoreSkins, useSkinSoundSamples, useTaikoSkin, useSkinCursor, autoCursorSize;

        public SkinSection()
        {
            content.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                new SpriteText { Text = "TODO: Skin preview textures" },
                new SpriteText { Text = "Current skin: TODO dropdown" },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Preview gameplay",
                },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Open skin folder",
                },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Export as .osk",
                },
                ignoreSkins = new CheckBoxOption { LabelText = "Ignore all beatmap skins" },
                useSkinSoundSamples = new CheckBoxOption { LabelText = "Use skin's sound samples" },
                useTaikoSkin = new CheckBoxOption { LabelText = "Use Taiko skin for Taiko mode" },
                useSkinCursor = new CheckBoxOption { LabelText = "Always use skin cursor" },
                new SpriteText { Text = "Cursor size: TODO slider" },
                autoCursorSize = new CheckBoxOption { LabelText = "Automatic cursor size" },
            };
        }
        
        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                ignoreSkins.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.IgnoreBeatmapSkins);
                useSkinSoundSamples.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.SkinSamples);
                useTaikoSkin.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.UseTaikoSkin);
                useSkinCursor.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.UseSkinCursor);
                autoCursorSize.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.AutomaticCursorSizing);
            }
        }
    }
}