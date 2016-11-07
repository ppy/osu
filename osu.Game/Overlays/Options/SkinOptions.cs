using System;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class SkinOptions : OptionsSection
    {
        protected override string Header => "Skin";
        public override FontAwesome Icon => FontAwesome.fa_paint_brush;

        public SkinOptions()
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
                new BasicCheckBox { LabelText = "Ignore all beatmap skins" },
                new BasicCheckBox { LabelText = "Use skin's sound samples" },
                new BasicCheckBox { LabelText = "Use Taiko skin for Taiko mode" },
                new BasicCheckBox { LabelText = "Always use skin cursor" },
                new SpriteText { Text = "Cursor size: TODO slider" },
                new BasicCheckBox { LabelText = "Automatic cursor size" },
            };
        }
    }
}