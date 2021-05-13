using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Screens.Mvis.Plugins;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.SideBar.PluginsPage
{
    public class PluginPiece : CompositeDrawable, IHasTooltip
    {
        public readonly MvisPlugin Plugin;
        private SpriteIcon unloadIcon;
        private OsuAnimatedButton unloadButton;
        private Indicator indicator;
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
        }

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        [Resolved]
        private DialogOverlay dialog { get; set; }

        private readonly BindableBool disabled = new BindableBool();
        private Container content;

        [BackgroundDependencyLoader]
        private void load(MvisPluginManager manager)
        {
            FillFlowContainer fillFlow;

            InternalChild = content = new Container
            {
                BorderColour = colourProvider.Light3,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 7,
                BorderThickness = 3f,
                Children = new Drawable[]
                {
                    bgBox = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background4
                    },
                    new DelayedLoadUnloadWrapper(() =>
                    {
                        var coverName = Plugin.GetType().Namespace?.Replace(".", "") ?? "Plugin";
                        var s = new PluginBackgroundSprite($"{coverName}/{Plugin.GetType().Name}")
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
                            Color4.White.Opacity(0),
                            Color4.White),
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Width = 0.8f
                    },
                    maskBox = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background3.Opacity(0.65f)
                    },
                    indicator = new Indicator
                    {
                        Colour = indicatorInActiveColor
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
                }
            };

            if (Plugin.Version != manager.PluginVersion)
            {
                fillFlow.Add(new OsuSpriteText
                {
                    Colour = Color4.Gold,
                    Text = Plugin.Version < manager.PluginVersion ? "为历史版本打造" : "为未来版本打造",
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

                            changeColor(!activeListContainsPlugin
                                ? indicatorInActiveColor
                                : Color4.Gold);

                            break;

                        case false:

                            indicator.TooltipText = activeListContainsPlugin
                                ? "禁用该插件"
                                : "该插件报告它已被启用, 但我们没有在已启用的插件中找到它。";

                            changeColor(activeListContainsPlugin
                                ? indicatorActiveColor
                                : Color4.Gold);

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
                changeColor(Color4.Gold);
            }

            if (Plugin.Flags.Contains(MvisPlugin.PluginFlags.CanUnload))
            {
                unloadButton.Action = () =>
                {
                    dialog.Push(new PluginRemoveConfirmDialog($"你确定要卸载{Plugin.Name}吗?", blockPlugin => manager.UnLoadPlugin(Plugin, blockPlugin)));
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
                unloadButton.Colour = defaultIconColor;
            }, true);
        }

        private void changeColor(Color4 target)
        {
            indicator.FadeColour(target, 300, Easing.OutQuint);
            content.BorderColour = target;
        }

        public override void Hide()
        {
            content.MoveToX(50, 200, Easing.OutSine).FadeOut(200, Easing.OutExpo);
            this.Delay(200).Expire();
        }

        public string TooltipText => Plugin.Description;

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

        private class Indicator : ClickableContainer, IHasTooltip
        {
            public string TooltipText { get; set; }

            public Indicator()
            {
                RelativeSizeAxes = Axes.Y;
                Width = 5 + 7 + (5 + 11); //向左扩展16, 向右扩展7, 这样能让插件启用、禁用没那么难点
                Height = 0.8f;
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                //Margin = new MarginPadding { Left = 11f };
                Padding = new MarginPadding { Vertical = 10, Left = 5 + 11, Right = 7 };
                Child = new Circle
                {
                    RelativeSizeAxes = Axes.Both
                };
            }

            private Sample sampleHover;
            private Sample sampleClick;

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                sampleClick = audio.Samples.Get("UI/generic-select-soft");
                sampleHover = audio.Samples.Get("UI/generic-hover-soft");
            }

            protected override bool OnHover(HoverEvent e)
            {
                sampleHover?.Play();
                return base.OnHover(e);
            }

            protected override bool OnClick(ClickEvent e)
            {
                sampleClick?.Play();
                return base.OnClick(e);
            }
        }
    }
}
