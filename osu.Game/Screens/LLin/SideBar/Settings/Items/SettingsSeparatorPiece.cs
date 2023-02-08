using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Screens.LLin.SideBar.Settings.Items
{
    public partial class SettingsSeparatorPiece : SettingsPieceBasePanel
    {
        public SettingsSeparatorPiece()
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Icon = new IconUsage();

            RelativeSizeAxes = Axes.X;
            Width = 1;

            SpriteText.Anchor = SpriteText.Origin = SpriteIcon.Anchor = SpriteIcon.Origin = Anchor.BottomRight;
            FillFlow.Direction = FillDirection.Horizontal;

            SpriteIcon.Hide();
        }

        protected override void OnColorChanged()
        {
            BgBox.Colour = Color4.Black.Opacity(0);
        }
    }
}
