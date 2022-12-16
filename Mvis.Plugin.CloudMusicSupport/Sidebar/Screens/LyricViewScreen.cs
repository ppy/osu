using System.Linq;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.CloudMusicSupport.Config;
using Mvis.Plugin.CloudMusicSupport.Misc;
using Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.LLin;
using osu.Game.Screens.LLin.Plugins;
using osuTK;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Screens
{
    public partial class LyricViewScreen : LyricScreen<LyricPiece>
    {
        [Resolved]
        private IImplementLLin llin { get; set; }

        [Resolved]
        private LyricPlugin plugin { get; set; }

        protected override LyricPiece CreatePiece(Lyric lyric)
            => new LyricPiece(lyric);

        public override IconButton[] Entries => new[]
        {
            saveButton,
            new IconButton
            {
                Icon = FontAwesome.Solid.Undo,
                Size = new Vector2(45),
                TooltipText = CloudMusicStrings.Refresh,
                Action = () => plugin.RefreshLyric()
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.CloudDownloadAlt,
                Size = new Vector2(45),
                TooltipText = CloudMusicStrings.RefetchLyric,
                Action = () => llin.PushDialog(plugin,
                    FontAwesome.Solid.ExclamationTriangle,
                    CloudMusicStrings.RefetchLyric,
                    "要继续吗?", new[]
                    {
                        new DialogOption
                        {
                            Text = "是的，我确定",
                            Action = () => plugin.RefreshLyric(true),
                            Type = OptionType.Confirm
                        },
                        new DialogOption
                        {
                            Text = "我需要再想想",
                            Type = OptionType.Cancel
                        }
                    })
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.AngleDown,
                Size = new Vector2(45),
                TooltipText = CloudMusicStrings.ScrollToCurrent,
                Action = ScrollToCurrent
            }
        };

        private readonly IconButton saveButton = new IconButton
        {
            Icon = FontAwesome.Solid.Save,
            Size = new Vector2(45),
            TooltipText = CloudMusicStrings.Save
        };

        private Bindable<bool> allowAutoScroll;

        [BackgroundDependencyLoader]
        private void load(LyricConfigManager config)
        {
            allowAutoScroll = config.GetBindable<bool>(LyricSettings.AutoScrollToCurrent);
        }

        protected override void LoadComplete()
        {
            saveButton.Action = () => plugin.WriteLyricToDisk();

            plugin.CurrentStatus.BindValueChanged(v =>
            {
                switch (v.NewValue)
                {
                    case LyricPlugin.Status.Finish:
                        saveButton.FadeIn(300, Easing.OutQuint);
                        break;

                    case LyricPlugin.Status.Failed:
                        saveButton.FadeOut(300, Easing.OutQuint);
                        break;
                }
            }, true);

            base.LoadComplete();
        }

        protected override void Update()
        {
            if (followCooldown.Value == 0 && allowAutoScroll.Value)
                ScrollToCurrent();

            base.Update();
        }

        protected override void ScrollToCurrent()
        {
            float pos = AvaliablePieces.FirstOrDefault(p =>
                p.Value.Equals(plugin.CurrentLine))?.CurrentY ?? 0;

            if (pos + DrawHeight > LyricScroll.ScrollContent.Height)
                LyricScroll.ScrollToEnd();
            else
                LyricScroll.ScrollTo(pos);
        }

        private readonly BindableFloat followCooldown = new BindableFloat();

        protected override bool OnHover(HoverEvent e)
        {
            this.TransformBindableTo(followCooldown, 1);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            this.TransformBindableTo(followCooldown, 0, 3000);
            base.OnHoverLost(e);
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            this.MoveToX(0, 200, Easing.OutQuint).FadeInFromZero(200, Easing.OutQuint);
            base.OnEntering(e);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.MoveToX(10, 200, Easing.OutQuint).FadeOut(200, Easing.OutQuint);
            base.OnSuspending(e);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            this.MoveToX(0, 200, Easing.OutQuint).FadeIn(200, Easing.OutQuint);
            base.OnResuming(e);
        }
    }
}
