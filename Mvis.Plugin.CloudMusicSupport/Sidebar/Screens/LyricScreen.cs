using System.Collections.Generic;
using System.Linq;
using Mvis.Plugin.CloudMusicSupport.Misc;
using Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis;
using osuTK;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Screens
{
    public abstract class LyricScreen : SidebarScreen
    {
        protected abstract DrawableLyric CreateDrawableLyric(Lyric lyric);

        [Resolved]
        private LyricPlugin plugin { get; set; }

        protected LyricPlugin Plugin => plugin;

        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        private readonly OsuScrollContainer scroll;
        protected readonly FillFlowContainer<DrawableLyric> LyricFlow;

        protected LyricScreen()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = scroll = new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = LyricFlow = new FillFlowContainer<DrawableLyric>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(5),
                    Padding = new MarginPadding(5)
                }
            };
        }

        protected override void LoadComplete()
        {
            plugin.CurrentStatus.ValueChanged += onPluginStatusChanged;
            UpdateStatus(plugin.CurrentStatus.Value);

            base.LoadComplete();
        }

        protected override void Dispose(bool isDisposing)
        {
            plugin.CurrentStatus.ValueChanged -= onPluginStatusChanged;
            base.Dispose(isDisposing);
        }

        protected virtual void UpdateStatus(LyricPlugin.Status status)
        {
            switch (status)
            {
                case LyricPlugin.Status.Finish:
                    RefreshLrcInfo(plugin.Lyrics);
                    break;

                case LyricPlugin.Status.Failed:
                    break;

                default:
                    LyricFlow.Clear();
                    break;
            }
        }

        private void onPluginStatusChanged(ValueChangedEvent<LyricPlugin.Status> v)
            => UpdateStatus(v.NewValue);

        protected virtual void ScrollToCurrent()
        {
            var pos = LyricFlow.Children.FirstOrDefault(p =>
                p.Value == plugin.Lyrics.FindLast(l => plugin.GetCurrentTrack().CurrentTime >= l.Time))?.Y ?? 0;

            if (pos + scroll.DrawHeight > LyricFlow.Height)
                scroll.ScrollToEnd();
            else
                scroll.ScrollTo(pos);
        }

        protected virtual void RefreshLrcInfo(List<Lyric> lyrics)
        {
            LyricFlow.Clear();
            scroll.ScrollToStart();

            foreach (var t in lyrics)
            {
                LyricFlow.Add(CreateDrawableLyric(t));
            }
        }
    }
}
