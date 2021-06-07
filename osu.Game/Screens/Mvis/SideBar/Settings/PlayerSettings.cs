using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Screens.Mvis.SideBar.Settings.Sections;
using osuTK;

namespace osu.Game.Screens.Mvis.SideBar.Settings
{
    public class PlayerSettings : CompositeDrawable, ISidebarContent
    {
        public string Title => "播放器设置";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Width = 0.3f,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Spacing = new Vector2(10),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new BaseSettings(),
                    new AudioSettings()
                }
            };
        }
    }
}
