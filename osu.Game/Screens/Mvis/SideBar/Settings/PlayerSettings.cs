using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
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
            AutoSizeAxes = Axes.Both,
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            Spacing = new Vector2(5),
            Direction = FillDirection.Vertical
        };

        public string Title => "播放器设置";
        public IconUsage Icon { get; } = FontAwesome.Solid.Cog;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, MvisPluginManager pluginManager)
        {
            ScrollbarVisible = false;
            RelativeSizeAxes = Axes.Both;
            Add(fillFlow);

            AddSection(new BaseSettings());
            AddSection(new AudioSettings());

            foreach (var pl in pluginManager.GetAllPlugins(false))
            {
                var pluginSidebarSection = pl.CreateSidebarSettingsSection();

                if (pluginSidebarSection != null)
                    AddSection(pluginSidebarSection);
            }
        }

        public void AddSection(Section section) => fillFlow.Add(section);
    }
}
