using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Vitaru.Settings;

namespace osu.Game.Rulesets.Vitaru.Objects.Drawables.Pieces
{
    public class LaserPiece : BeatSyncedContainer
    {
        private readonly GraphicsPresets currentSkin = VitaruSettings.VitaruConfigManager.GetBindable<GraphicsPresets>(VitaruSetting.GraphicsPresets);

        public LaserPiece(DrawableLaser drawableLaser)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Masking = true;

            CornerRadius = 16;

            BorderThickness = 8;
            BorderColour = drawableLaser.AccentColour;

            Child = new Box
            {
                RelativeSizeAxes = Axes.Both
            };
        }
    }
}
