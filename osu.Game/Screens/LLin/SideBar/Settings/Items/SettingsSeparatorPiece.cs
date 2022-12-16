using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.LLin.SideBar.Settings.Items
{
    public partial class SettingsSeparatorPiece : SettingsPieceBasePanel
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Icon = new IconUsage();

            AddInternal(new SpriteIcon
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Size = new Vector2(20),
                Icon = FontAwesome.Solid.ArrowRight,
                Margin = new MarginPadding(10),
            });
        }

        protected override void OnColorChanged()
        {
            BgBox.Colour = Color4.Black.Opacity(0);
        }
    }
}
