using Mvis.Plugin.Yasp.Config;
using Mvis.Plugin.Yasp.UI;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.LLin.Misc;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.Plugins.Types;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.Yasp
{
    public class YaspPlugin : BindableControlledPlugin
    {
        private Drawable currentContent;

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.TargetLayer"/>
        /// </summary>
        public override TargetLayer Target => TargetLayer.Foreground;

        public override IPluginConfigManager CreateConfigManager(Storage storage)
            => new YaspConfigManager(storage);

        public override PluginSettingsSubSection CreateSettingsSubSection()
            => new YaspSettingsSubSection(this);

        public override PluginSidebarSettingsSection CreateSidebarSettingsSection()
            => new YaspSidebarSection(this);

        public override int Version => 8;

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

            AutoSizeAxes = Axes.Both;
        }

        private WorkingBeatmap currentWorkingBeatmap;

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.CreateContent()"/>
        /// </summary>
        protected override Drawable CreateContent() => new FillFlowContainer
        {
            Height = 90,
            AutoSizeAxes = Axes.X,
            Spacing = new Vector2(10),
            Direction = FillDirection.Horizontal,
            Margin = new MarginPadding(20),
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 3
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 90,
                    Masking = true,
                    CornerRadius = 5f,
                    Child = new BeatmapCover.Cover(currentWorkingBeatmap)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    }
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 30, weight: FontWeight.Bold),
                            Text = currentWorkingBeatmap.Metadata.TitleUnicode
                                   ?? currentWorkingBeatmap.Metadata.Title
                        },
                        new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 25),
                            Text = currentWorkingBeatmap.Metadata.ArtistUnicode
                                   ?? currentWorkingBeatmap.Metadata.Artist
                        },
                        new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 25),
                            Text = currentWorkingBeatmap.Metadata.Source
                        }
                    }
                }
            }
        }.WithEffect(new BlurEffect
        {
            Colour = Color4.Black.Opacity(0.7f),
            DrawOriginal = true
        });

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.OnContentLoaded(Drawable)"/>
        /// </summary>
        protected override bool OnContentLoaded(Drawable content)
        {
            content.MoveToX(10).FadeOut();
            currentContent?.MoveToX(-10, 300, Easing.OutQuint).FadeOut(300, Easing.OutQuint).Expire();
            content.MoveToX(0, 300, Easing.OutQuint).FadeIn(300, Easing.OutQuint);
            currentContent = content;
            return true;
        }

        public override bool Disable()
        {
            this.MoveToX(-10, 300, Easing.OutQuint).FadeOut(300, Easing.OutQuint);
            return base.Disable();
        }

        public override bool Enable()
        {
            bool result = base.Enable();

            this.MoveToX(0, 300, Easing.OutQuint).FadeIn(300, Easing.OutQuint);
            LLin?.OnBeatmapChanged(onBeatmapChanged, this, true);

            return result;
        }

        private Bindable<float> scaleBindable;

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (YaspConfigManager)Dependencies.Get<LLinPluginManager>().GetConfigManager(this);

            config.BindWith(YaspSettings.EnablePlugin, Value);
            scaleBindable = config.GetBindable<float>(YaspSettings.Scale);
            scaleBindable.BindValueChanged(v =>
            {
                this.ScaleTo(v.NewValue, 300, Easing.OutQuint);
            }, true);
        }

        protected override bool PostInit()
        {
            currentWorkingBeatmap ??= LLin.Beatmap.Value;
            return true;
        }

        private void refresh() => Load();

        private void onBeatmapChanged(WorkingBeatmap working)
        {
            if (Disabled.Value) return;

            if (currentWorkingBeatmap != working)
            {
                currentWorkingBeatmap = working;
                refresh();
            }
        }
    }
}
