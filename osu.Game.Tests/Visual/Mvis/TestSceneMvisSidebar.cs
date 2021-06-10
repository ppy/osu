// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis;
using osu.Game.Screens.Mvis.SideBar;

namespace osu.Game.Tests.Visual.Mvis
{
    public class TestSceneMvisSidebar : ScreenTestScene
    {
        private DependencyContainer dependencies;
        private Sidebar sidebar;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(new CustomColourProvider(0, 0, 1));

            Child = sidebar = new Sidebar
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
            };

            AddStep("Toggle Sidebar ", sidebar.ToggleVisibility);
            AddStep("Clear Sidebar", sidebar.Clear);
            AddStep("Random Resize", () => addRandom());
            AddStep("Try Resize to 20%x20%", resize);
            AddStep("Add Tabs", () =>
            {
                for (int i = 0; i < 20; i++)
                {
                    addRandom(false);
                }
            });
            AddStep("Very Long Tab", () => sidebar.Add(new VoidSidebarContent(1, $"OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOH")));
        }

        private void resize()
        {
            var sc = new VoidSidebarContent(0.2f, "???");
            sidebar.Add(sc);

            try
            {
                sidebar.ShowComponent(sc);
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
            }

            sidebar.Remove(sc);
        }

        private Drawable prevTab;
        private int num;

        private void addRandom(bool removePreviousTab = true)
        {
            float width = RNG.NextSingle(0.3f, 1);
            num++;
            var newTab = new VoidSidebarContent(width, $"{num}");
            sidebar.Add(newTab);

            if (prevTab != null && removePreviousTab)
            {
                sidebar.ShowComponent(newTab);
                sidebar.Remove(prevTab);
            }

            prevTab = newTab;
        }

        private class VoidSidebarContent : Container, ISidebarContent
        {
            public VoidSidebarContent(float reWidth, string tabTitle)
            {
                ReWidth = reWidth;
                TabTitle = tabTitle;
                RelativeSizeAxes = Axes.Both;
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
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
                    }
                };
            }

            public readonly float ReWidth;
            public readonly string TabTitle;

            public float ResizeWidth => ReWidth;
            public string Title => TabTitle;
            public IconUsage Icon { get; } = FontAwesome.Regular.Bell;
            public float ResizeHeight => 0.8f;
        }
    }
}
