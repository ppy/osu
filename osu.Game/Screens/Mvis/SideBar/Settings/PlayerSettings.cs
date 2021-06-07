using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.SideBar.Settings.Sections;
using osuTK;

namespace osu.Game.Screens.Mvis.SideBar.Settings
{
    public class PlayerSettings : OsuScrollContainer, ISidebarContent
    {
        private readonly FillFlowContainer<Section> fillFlow = new FillFlowContainer<Section>
        {
            AutoSizeAxes = Axes.Y,
            RelativeSizeAxes = Axes.X,
            Width = 0.3f,
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            Spacing = new Vector2(10),
            Direction = FillDirection.Vertical
        };

        public string Title => "播放器设置";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, MvisPluginManager pluginManager)
        {
            ScrollbarVisible = false;
            RelativeSizeAxes = Axes.Both;
            Add(fillFlow);

            AddSection(new BaseSettings());
            AddSection(new AudioSettings());

            foreach (var pl in pluginManager.GetActivePlugins())
            {
                var pluginSidebarSection = pl.CreateSidebarSettingsSection();

                if (pluginSidebarSection != null)
                    AddSection(pluginSidebarSection);
            }
        }

        public void AddSection(Section section) => fillFlow.Add(section);
    }
}
