using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Screens.Mvis.Plugins;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.SideBar
{
    public class SidebarPluginsPage : CompositeDrawable, ISidebarContent
    {
        public float ResizeWidth => 0.5f;
        public string Title => "插件";

        private MvisPluginManager manager;

        [BackgroundDependencyLoader]
        private void load(MvisPluginManager pluginManager)
        {
            RelativeSizeAxes = Axes.Both;

            manager = pluginManager;
            pluginManager.OnPluginListChanged += reloadPluginList;
        }

        protected override void LoadComplete()
        {
            reloadPluginList();
            base.LoadComplete();
        }

        private void reloadPluginList()
        {
            ClearInternal();
            var flow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Spacing = new Vector2(10),
                Padding = new MarginPadding(10)
            };

            foreach (var pl in manager.GetAllPlugins())
            {
                flow.Add(new PluginPiece(pl));
            }

            AddInternal(new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = flow
            });
        }
    }

    public class PluginPiece : Container
    {
        private readonly MvisPlugin plugin;
        private SpriteIcon icon;
        private OsuAnimatedButton toggleDisableButton;
        private SpriteIcon unloadIcon;
        private OsuAnimatedButton unloadButton;
        private Color4 defaultIconColor => Color4.White.Opacity(0.5f);

        public PluginPiece(MvisPlugin pl)
        {
            plugin = pl;

            Masking = true;
            CornerRadius = 10;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        [Resolved]
        private DialogOverlay dialog { get; set; }

        [BackgroundDependencyLoader]
        private void load(MvisPluginManager manager)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.1f,
                    Child = toggleDisableButton = new OsuAnimatedButton
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 1,
                        Child = icon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Color4.White.Opacity(0.5f),
                            Size = new Vector2(18)
                        }
                    },
                    Padding = new MarginPadding
                    {
                        Right = -5
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Width = 0.1f,
                    Child = unloadButton = new OsuAnimatedButton
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 1,
                        Child = unloadIcon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = defaultIconColor,
                            Size = new Vector2(18)
                        }
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 70,
                    Width = 0.8f,
                    Masking = true,
                    CornerRadius = 10,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background4
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = string.IsNullOrEmpty(plugin.Name) ? "未知名称" : plugin.Name,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre
                                },
                                new OsuSpriteText
                                {
                                    Text = plugin.Description,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre
                                }
                            }
                        },
                    }
                },
            };

            if (plugin.Flags.Contains(MvisPlugin.PluginFlags.CanDisable))
            {
                plugin.Disabled.BindValueChanged(v =>
                {
                    switch (v.NewValue)
                    {
                        case true:
                            if (!manager.GetActivePlugins().Contains(plugin))
                            {
                                toggleDisableButton.TooltipText = "启用该插件";
                                icon.Icon = FontAwesome.Solid.Times;
                                icon.FadeColour(defaultIconColor);
                            }
                            else
                            {
                                icon.Icon = FontAwesome.Solid.ExclamationTriangle;
                                icon.FadeColour(Color4.Gold);
                                toggleDisableButton.TooltipText = "该插件报告它已被禁用, 但我们在已启用的插件中找到了它。";
                            }

                            break;

                        case false:

                            if (manager.GetActivePlugins().Contains(plugin))
                            {
                                toggleDisableButton.TooltipText = "禁用该插件";
                                icon.Icon = FontAwesome.Solid.Check;
                                icon.FadeColour(defaultIconColor);
                            }
                            else
                            {
                                icon.Icon = FontAwesome.Solid.ExclamationTriangle;
                                icon.FadeColour(Color4.Gold);
                                toggleDisableButton.TooltipText = "该插件报告它已被启用, 但我们没有在已启用的插件中找到它。";
                            }

                            break;
                    }
                }, true);

                toggleDisableButton.Action = () =>
                {
                    if (plugin.Disabled.Value)
                        manager.ActivePlugin(plugin);
                    else
                        manager.DisablePlugin(plugin);
                };
            }
            else
            {
                toggleDisableButton.TooltipText = "目前不能通过此面板禁用该插件";
                icon.Icon = FontAwesome.Solid.Ban;
            }

            if (plugin.Flags.Contains(MvisPlugin.PluginFlags.CanUnload))
            {
                unloadButton.Action = () =>
                {
                    dialog.Push(new ConfirmDialog($"你确定要卸载{plugin.Name}吗?", () => manager.UnLoadPlugin(plugin))
                    {
                        BodyText = "卸载后该插件在本次Mvis会话中将不再可用!"
                    });
                };
                unloadButton.TooltipText = "卸载此插件";
                unloadIcon.Icon = FontAwesome.Solid.TrashAlt;
            }
            else
            {
                unloadButton.TooltipText = "目前不能通过此面板卸载该插件";
                unloadIcon.Icon = FontAwesome.Solid.Ban;
            }
        }
    }
}
