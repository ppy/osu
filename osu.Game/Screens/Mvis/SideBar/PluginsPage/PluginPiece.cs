using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.SideBar.Settings.Items;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.SideBar.PluginsPage
{
    public class PluginPiece : SettingsPieceBasePanel, IHasTooltip
    {
        public readonly MvisPlugin Plugin;
        private SpriteIcon unloadIcon;
        private OsuAnimatedButton unloadButton;

        public PluginPiece(MvisPlugin pl)
        {
            Plugin = pl;
        }

        [Resolved]
        private DialogOverlay dialog { get; set; }

        private readonly BindableBool disabled = new BindableBool();

        private readonly OsuSpriteText pluginNameText = new OsuSpriteText
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
        };

        protected override Drawable CreateSideDrawable() => pluginNameText;

        private Action action;

        [BackgroundDependencyLoader]
        private void load(MvisPluginManager manager)
        {
            Width = 270;
            pluginNameText.Text = string.IsNullOrEmpty(Plugin.Name) ? " " : Plugin.Name;

            FillFlow.AddRange(new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = string.IsNullOrEmpty(Plugin.Author) ? " " : Plugin.Author,
                    RelativeSizeAxes = Axes.X,
                    Truncate = true
                },
                new OsuSpriteText
                {
                    Text = string.IsNullOrEmpty(Plugin.Description) ? " " : Plugin.Description,
                    RelativeSizeAxes = Axes.X,
                    Truncate = true
                },
                unloadButton = new OsuAnimatedButton
                {
                    Margin = new MarginPadding { Right = 15 },
                    Size = new Vector2(30),
                    Child = unloadIcon = new SpriteIcon
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both
                    }
                },
                new OsuSpriteText
                {
                    Colour = Color4.Gold,
                    Text = Plugin.Version != manager.PluginVersion
                        ? Plugin.Version < manager.PluginVersion ? "为历史版本打造" : "为未来版本打造"
                        : " ",
                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                }
            });

            AddInternal(
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
                    Width = 0.8f,
                    Depth = 1
                });

            if (Plugin.Flags.Contains(MvisPlugin.PluginFlags.CanDisable))
            {
                disabled.BindTo(Plugin.Disabled);

                disabled.BindValueChanged(v =>
                {
                    var activeListContainsPlugin = manager.GetActivePlugins().Contains(Plugin);

                    switch (v.NewValue)
                    {
                        case true:

                            BgBox.FadeColour(ColourProvider.InActiveColor, 300, Easing.OutQuint);
                            FillFlow.FadeColour(Color4.White, 300, Easing.OutQuint);
                            pluginNameText.FadeColour(Color4.White, 300, Easing.OutQuint);

                            TooltipText = (!activeListContainsPlugin
                                              ? "启用该插件"
                                              : "该插件报告它已被禁用, 但我们在已启用的插件中找到了它。")
                                          + $"\n({Plugin.Description})";

                            break;

                        case false:

                            BgBox.FadeColour(ColourProvider.ActiveColor, 300, Easing.OutQuint);
                            FillFlow.FadeColour(Color4.Black, 300, Easing.OutQuint);
                            pluginNameText.FadeColour(Color4.Black, 300, Easing.OutQuint);

                            TooltipText = (activeListContainsPlugin
                                              ? "禁用该插件"
                                              : "该插件报告它已被启用, 但我们没有在已启用的插件中找到它。")
                                          + $"\n({Plugin.Description})";

                            break;
                    }
                }, true);

                action = () =>
                {
                    if (Plugin.Disabled.Value)
                        manager.ActivePlugin(Plugin);
                    else
                        manager.DisablePlugin(Plugin);
                };
            }
            else
            {
                TooltipText = "目前不能通过此面板禁用该插件"
                              + $"\n({Plugin.Description})";
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
        }

        protected override void OnColorChanged()
        {
            disabled.TriggerChange();
            base.OnColorChanged();
        }

        public override void Hide()
        {
            this.FadeOut(200, Easing.OutExpo);
            this.Delay(200).Expire();
        }

        public string TooltipText { get; set; }

        protected override bool OnClick(ClickEvent e)
        {
            action?.Invoke();
            return base.OnClick(e);
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
