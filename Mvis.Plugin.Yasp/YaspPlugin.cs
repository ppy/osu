using System;
using M.Resources.Localisation.LLin;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.Yasp.Config;
using Mvis.Plugin.Yasp.Panels;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.Plugins.Types;
using osu.Game.Screens.LLin.Plugins.Types.SettingsItems;

namespace Mvis.Plugin.Yasp
{
    public partial class YaspPlugin : BindableControlledPlugin
    {
        private Drawable? currentContent;

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.TargetLayer"/>
        /// </summary>
        public override TargetLayer Target => TargetLayer.Foreground;

        public override IPluginConfigManager CreateConfigManager(Storage storage)
            => new YaspConfigManager(storage);

        public override SettingsEntry[] GetSettingEntries(IPluginConfigManager pluginConfigManager)
        {
            var config = (YaspConfigManager)pluginConfigManager;

            return new SettingsEntry[]
            {
                new NumberSettingsEntry<float>
                {
                    Icon = FontAwesome.Solid.ExpandArrowsAlt,
                    Name = YaspStrings.Scale,
                    Bindable = config.GetBindable<float>(YaspSettings.Scale),
                    DisplayAsPercentage = true,
                },
                new BooleanSettingsEntry
                {
                    Name = LLinGenericStrings.EnablePlugin,
                    Bindable = config.GetBindable<bool>(YaspSettings.EnablePlugin)
                },
                new EnumSettingsEntry<PanelType>
                {
                    Name = "面板样式",
                    Bindable = config.GetBindable<PanelType>(YaspSettings.PanelType)
                }
            };
        }

        public override int Version => 10;

        public YaspPlugin()
        {
            Name = "YASP";
            Description = "另一个简单的播放器面板";
            Author = "MATRIX-夜翎";

            Flags.AddRange(new[]
            {
                PluginFlags.CanDisable,
                PluginFlags.CanUnload
            });

            RelativeSizeAxes = Axes.Both;
        }

        private readonly Bindable<PanelType> panelType = new Bindable<PanelType>();

        private WorkingBeatmap? currentWorkingBeatmap;

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.CreateContent()"/>
        /// </summary>
        protected override Drawable CreateContent()
        {
            Drawable target;

            switch (panelType.Value)
            {
                case PanelType.Classic:
                    target = new ClassicPanel();
                    break;

                case PanelType.SongCover:
                    target = new NsiPanel();
                    break;

                default:
                    throw new InvalidOperationException($"未知的PanelType: {panelType.Value}");
            }

            return target;
        }

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.OnContentLoaded(Drawable)"/>
        /// </summary>
        protected override bool OnContentLoaded(Drawable content)
        {
            currentContent?.Hide();
            currentContent?.Expire();

            currentContent = content;

            if (!Enabled.Value) return true;

            content.Show();
            refresh();

            return true;
        }

        public override bool Disable()
        {
            currentContent?.Hide();
            return base.Disable();
        }

        public override bool Enable()
        {
            bool result = base.Enable();

            currentContent?.Show();
            LLin!.OnBeatmapChanged(onBeatmapChanged, this, true);

            return result;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (YaspConfigManager)Dependencies.Get<LLinPluginManager>().GetConfigManager(this);
            config.BindWith(YaspSettings.EnablePlugin, Enabled);
            config.BindWith(YaspSettings.PanelType, panelType);

            panelType.BindValueChanged(v =>
            {
                Load();
            }, true);
        }

        protected override bool PostInit()
        {
            currentWorkingBeatmap ??= LLin?.Beatmap.Value;
            return true;
        }

        private void refresh()
        {
            if (currentContent is IPanel panel)
                panel.Refresh(currentWorkingBeatmap!);
        }

        private void onBeatmapChanged(WorkingBeatmap working)
        {
            Logger.Log($"CHANGE! {working} :: {currentWorkingBeatmap}");
            if (Disabled.Value) return;

            if (currentWorkingBeatmap != working)
            {
                currentWorkingBeatmap = working;
                refresh();
            }
        }
    }
}
