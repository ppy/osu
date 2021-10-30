using M.Resources.Localisation.LLin.Plugins;
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
using osu.Game.Screens.LLin;
using osuTK;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Screens
{
    public class LyricViewScreenWithDrawablePool : LyricScreenWithDrawablePool<LyricPiece>
    {
        [Resolved]
        private IImplementLLin mvisScreen { get; set; }

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
                Action = () => dialog.Push
                (
                    new ConfirmDialog(CloudMusicStrings.RefetchLyric.ToString(),
                        () => plugin.RefreshLyric(true))
                )
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.AngleDown,
                Size = new Vector2(45),
                TooltipText = CloudMusicStrings.ScrollToCurrent,
                Action = ScrollToCurrent
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
            TooltipText = CloudMusicStrings.Save
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
