using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.Plugins;
using osuTK;

namespace osu.Game.Screens.Mvis.SideBar
{
    public class SidebarPluginsPage : FillFlowContainer, ISidebarContent
    {
        public float ResizeWidth => 0.5f;
        public string Title => "插件";

        [BackgroundDependencyLoader]
        private void load(MvisPluginManager pluginManager)
        {
            Spacing = new Vector2(5);
            Direction = FillDirection.Vertical;

            foreach (var avaliablePlugin in pluginManager.GetAvaliablePlugins())
            {
                Add(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = avaliablePlugin.Name
                        },
                        new OsuSpriteText
                        {
                            Text = avaliablePlugin.Description
                        }
                    }
                });
            }
        }
    }
}
