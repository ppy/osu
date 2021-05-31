using Mvis.Plugin.CloudMusicSupport.Misc;
using Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Screens.Mvis;
using osuTK;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Screens
{
    public class LyricViewScreen : LyricScreen<LyricPiece>
    {
        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        [Resolved]
        private LyricPlugin plugin { get; set; }

        [Resolved]
        private DialogOverlay dialog { get; set; }

        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private Storage storage { get; set; }

        [Resolved]
        private LyricSidebarSectionContainer sectionContainer { get; set; }

        protected override LyricPiece CreateDrawableLyric(Lyric lyric)
            => new LyricPiece(lyric)
            {
                Action = l => mvisScreen.SeekTo(l.Time + 1)
            };

        public override Drawable[] Entries => new Drawable[]
        {
            saveButton,
            new IconButton
            {
                Icon = FontAwesome.Solid.Undo,
                Size = new Vector2(45),
                TooltipText = "刷新",
                Action = () => plugin.RefreshLyric()
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.CloudDownloadAlt,
                Size = new Vector2(45),
                TooltipText = "重新获取歌词",
                Action = () => dialog.Push
                (
                    new ConfirmDialog("重新获取歌词",
                        () => plugin.RefreshLyric(true))
                )
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.Edit,
                Size = new Vector2(45),
                TooltipText = "编辑",
                Action = pushEditScreen
            }
        };

        private void pushEditScreen()
        {
            plugin.RequestControl(() => this.Push(new LyricEditScreen()));
        }

        private readonly IconButton saveButton = new IconButton
        {
            Icon = FontAwesome.Solid.Save,
            Size = new Vector2(45),
            TooltipText = "保存为lrc"
        };

        protected override void LoadComplete()
        {
            saveButton.Action = plugin.WriteLyricToDisk;

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

        private readonly BindableFloat followCooldown = new BindableFloat();

        protected override void Update()
        {
            //if (followCooldown.Value == 0) ScrollToCurrent();
            base.Update();
        }

        protected override bool OnHover(HoverEvent e)
        {
            followCooldown.Value = 1;
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            this.TransformBindableTo(followCooldown, 0, 3000);
            base.OnHoverLost(e);
        }

        public override void OnEntering(IScreen last)
        {
            this.MoveToX(0, 200, Easing.OutQuint).FadeInFromZero(200, Easing.OutQuint);
            base.OnEntering(last);
        }

        public override void OnSuspending(IScreen next)
        {
            this.MoveToX(10, 200, Easing.OutQuint).FadeOut(200, Easing.OutQuint);
            base.OnSuspending(next);
        }

        public override void OnResuming(IScreen last)
        {
            this.MoveToX(0, 200, Easing.OutQuint).FadeIn(200, Easing.OutQuint);
            base.OnResuming(last);
        }
    }
}
