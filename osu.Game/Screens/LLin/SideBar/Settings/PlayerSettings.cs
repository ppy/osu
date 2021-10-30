using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.SideBar.Settings.Sections;
using osu.Game.Screens.LLin.SideBar.Tabs;
using osuTK;

namespace osu.Game.Screens.LLin.SideBar.Settings
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

        private Bindable<TabControlPosition> currentTabPosition;

        public string Title => "播放器设置";
        public IconUsage Icon { get; } = FontAwesome.Solid.Cog;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, LLinPluginManager pluginManager)
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

            currentTabPosition = config.GetBindable<TabControlPosition>(MSetting.MvisTabControlPosition);
            currentTabPosition.BindValueChanged(onTabPositionChanged, true);
        }

        private void onTabPositionChanged(ValueChangedEvent<TabControlPosition> v)
        {
            switch (v.NewValue)
            {
                case TabControlPosition.Left:
                    fillFlow.Anchor = fillFlow.Origin = Anchor.TopLeft;
                    break;

                case TabControlPosition.Right:
                    fillFlow.Anchor = fillFlow.Origin = Anchor.TopRight;
                    break;

                case TabControlPosition.Top:
                    fillFlow.Anchor = fillFlow.Origin = Anchor.TopCentre;
                    break;
            }
        }

        public void AddSection(Section section) => fillFlow.Add(section);
    }
}
