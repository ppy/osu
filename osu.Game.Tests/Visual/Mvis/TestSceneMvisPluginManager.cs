// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.SideBar;
using osuTK;

namespace osu.Game.Tests.Visual.Mvis
{
    public class TestSceneMvisPluginManager : ScreenTestScene
    {
        private DependencyContainer dependencies;
        private Sidebar sidebar;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        private BasicMvisPlugin plugin = new BasicMvisPlugin();

        [BackgroundDependencyLoader]
        private void load()
        {
            var manager = new MvisPluginManager();
            dependencies.Cache(new CustomColourProvider(0, 0, 1));

            dependencies.Cache(sidebar = new Sidebar
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
            });

            Children = new Drawable[]
            {
                sidebar,
                manager
            };

            AddStep("Toggle Sidebar ", sidebar.ToggleVisibility);
            AddStep("Add Plugin To Manager", () =>
            {
                if (manager.AddPlugin(plugin))
                    Add(plugin);
            });
            AddStep("Enable Plugin", () => manager.ActivePlugin(plugin));
            AddStep("Disable Plugin", () => manager.DisablePlugin(plugin));
            AddStep("Remove All Plugin From Manager", () =>
            {
                foreach (var mvisPlugin in manager.GetAllPlugins())
                {
                    if (manager.UnLoadPlugin(mvisPlugin))
                        Remove(mvisPlugin);
                }

                plugin = new BasicMvisPlugin();
            });
        }

        private class BasicMvisPlugin : MvisPlugin
        {
            public BasicMvisPlugin()
            {
                page.Plugin = this;
                Size = new Vector2(200, 100);
            }

            private readonly OsuSpriteText text = new OsuSpriteText
            {
                Text = "这是一个插件！"
            };

            protected override Drawable CreateContent() => text;

            protected override bool OnContentLoaded(Drawable content) => true;

            protected override bool PostInit() => true;

            private readonly PluginSidebarPage page = new VoidSidebarContent(0.5f, "插件");
            public override PluginSidebarPage SidebarPage => page;

            public override bool Enable()
            {
                Logger.Log("插件启用");
                text.Alpha = 1;
                return base.Enable();
            }

            public override void Load()
            {
                Logger.Log("插件加载");
                base.Load();
            }

            public override bool Disable()
            {
                Logger.Log("插件禁用");
                text.Alpha = 0.5f;
                return base.Disable();
            }

            public override void UnLoad()
            {
                Logger.Log("插件卸载");
                base.UnLoad();
            }
        }

        private class VoidSidebarContent : PluginSidebarPage
        {
            private readonly OsuSpriteText t;

            public VoidSidebarContent(float reWidth, string tabTitle)
                : base(reWidth, tabTitle)
            {
                RelativeSizeAxes = Axes.Both;
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "num: " + tabTitle,
                        },
                        new OsuSpriteText
                        {
                            Text = "width: " + reWidth.ToString(CultureInfo.CurrentCulture),
                        },
                        t = new OsuSpriteText()
                    }
                };
            }

            protected override void InitContent(MvisPlugin plugin)
            {
                t.Text = plugin.ToString();
                base.InitContent(plugin);
            }
        }
    }
}
