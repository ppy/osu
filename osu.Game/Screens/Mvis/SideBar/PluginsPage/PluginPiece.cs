using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Screens.Mvis.Plugins;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.SideBar.PluginsPage
{
    public class PluginPiece : Container
    {
        public readonly MvisPlugin Plugin;
        private SpriteIcon unloadIcon;
        private OsuAnimatedButton unloadButton;
        private OsuClickableContainer indicator;
        private Box maskBox;
        private Box bgBox;
        private Color4 defaultIconColor => colourProvider.Light1.Opacity(0.5f);
        private Color4 indicatorActiveColor => colourProvider.Light1;
        private Color4 indicatorInActiveColor => colourProvider.Background1;

        public PluginPiece(MvisPlugin pl)
        {
            Plugin = pl;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Masking = true;
            CornerRadius = 10;

            Masking = true;
            BorderThickness = 3f;
        }

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        [Resolved]
        private DialogOverlay dialog { get; set; }

        private readonly BindableBool disabled = new BindableBool();

        [BackgroundDependencyLoader]
        private void load(MvisPluginManager manager)
        {
            BorderColour = colourProvider.Light3;
            FillFlowContainer fillFlow;

            InternalChildren = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                new DelayedLoadUnloadWrapper(() =>
                {
                    var coverName = Plugin.GetType().Namespace?.Replace(".", "") ?? string.Empty;
                    var s = new PluginBackgroundSprite(coverName)
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Alpha = 0
                    };

                    s.OnLoadComplete += d => d.FadeIn(300);

                    return s;
                }, 0)
                {
                    Colour = ColourInfo.GradientHorizontal(
                        Color4.White,
                        Color4.White.Opacity(0)),
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.8f
                },
                maskBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3.Opacity(0.65f)
                },
                indicator = new OsuClickableContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 5 + 7 + 5, //向左扩展3.5, 向右扩展8.5, 这样能让插件启用、禁用没那么难点
                    Colour = indicatorInActiveColor,
                    Height = 0.8f,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding { Left = 11f },
                    Padding = new MarginPadding { Vertical = 10, Left = 5f, Right = 7f },
                    Child = new Circle
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                },
                fillFlow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding { Left = 30, Vertical = 17 },
                    Padding = new MarginPadding { Right = 30 + 60 },
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = string.IsNullOrEmpty(Plugin.Author) ? " " : Plugin.Author,
                            RelativeSizeAxes = Axes.X,
                            Truncate = true
                        },
                        new OsuSpriteText
                        {
                            Text = string.IsNullOrEmpty(Plugin.Name) ? " " : Plugin.Name,
                            RelativeSizeAxes = Axes.X,
                            Truncate = true
                        },
                        new OsuSpriteText
                        {
                            Text = string.IsNullOrEmpty(Plugin.Description) ? " " : Plugin.Description,
                            RelativeSizeAxes = Axes.X,
                            Truncate = true
                        }
                    }
                },
                unloadButton = new OsuAnimatedButton
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 15 },
                    Size = new Vector2(30),
                    Child = unloadIcon = new SpriteIcon
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Colour = defaultIconColor,
                        RelativeSizeAxes = Axes.Both
                    }
                }
            };

            if (Plugin.Version != manager.PLUGIN_VERSION)
            {
                fillFlow.Add(new OsuSpriteText
                {
                    Colour = Color4.Gold,
                    Text = "插件版本不匹配",
                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                });
            }

            if (Plugin.Flags.Contains(MvisPlugin.PluginFlags.CanDisable))
            {
                disabled.BindTo(Plugin.Disabled);

                disabled.BindValueChanged(v =>
                {
                    var activeListContainsPlugin = manager.GetActivePlugins().Contains(Plugin);

                    switch (v.NewValue)
                    {
                        case true:
                            indicator.TooltipText = !activeListContainsPlugin
                                ? "启用该插件"
                                : "该插件报告它已被禁用, 但我们在已启用的插件中找到了它。";

                            indicator.FadeColour(!activeListContainsPlugin
                                ? indicatorInActiveColor
                                : Color4.Gold, 300, Easing.OutQuint);
                            break;

                        case false:

                            indicator.TooltipText = activeListContainsPlugin
                                ? "禁用该插件"
                                : "该插件报告它已被启用, 但我们没有在已启用的插件中找到它。";

                            indicator.FadeColour(activeListContainsPlugin
                                ? indicatorActiveColor
                                : Color4.Gold, 300, Easing.OutQuint);
                            break;
                    }
                }, true);

                indicator.Action = () =>
                {
                    if (Plugin.Disabled.Value)
                        manager.ActivePlugin(Plugin);
                    else
                        manager.DisablePlugin(Plugin);
                };
            }
            else
            {
                indicator.TooltipText = "目前不能通过此面板禁用该插件";
            }

            if (Plugin.Flags.Contains(MvisPlugin.PluginFlags.CanUnload))
            {
                unloadButton.Action = () =>
                {
                    dialog.Push(new ConfirmDialog($"你确定要卸载{Plugin.Name}吗?", () => manager.UnLoadPlugin(Plugin))
                    {
                        BodyText = "卸载后该插件在本次osu!会话中将不再可用!"
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

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                disabled.TriggerChange();
                maskBox.FadeColour(colourProvider.Background3.Opacity(0.65f));
                bgBox.FadeColour(colourProvider.Background4);
                BorderColour = colourProvider.Light3;
                unloadButton.Colour = defaultIconColor;
            }, true);
        }

        private class PluginBackgroundSprite : Sprite
        {
            private readonly string target;

            public PluginBackgroundSprite(string target = null)
            {
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fill;

                this.target = target;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures)
            {
                Texture = textures.Get(target);
            }
        }
    }
}
