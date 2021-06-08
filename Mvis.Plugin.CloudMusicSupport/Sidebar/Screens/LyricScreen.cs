using System.Collections.Generic;
using System.Linq;
using Mvis.Plugin.CloudMusicSupport.Misc;
using Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis;
using osuTK;
using osuTK.Graphics;

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

        protected readonly OsuScrollContainer LyricScroll;
        protected readonly FillFlowContainer<DrawableLyric> LyricFlow;
        //private readonly FillFlowContainer placeholder;

        protected LyricScreen()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                LyricScroll = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = LyricFlow = new FillFlowContainer<DrawableLyric>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(5),
                        Padding = new MarginPadding(5)
                    }
                },
                //placeholder = new FillFlowContainer
                //{
                //    AutoSizeAxes = Axes.Both,
                //    Direction = FillDirection.Vertical,
                //    Anchor = Anchor.TopRight,
                //    Origin = Anchor.TopRight,
                //    Colour = Color4.White.Opacity(0.6f),
                //    Margin = new MarginPadding(40),
                //    Children = new Drawable[]
                //    {
                //        new SpriteIcon
                //        {
                //            Icon = FontAwesome.Solid.HorseHead,
                //            Size = new Vector2(60),
                //            Anchor = Anchor.Centre,
                //            Origin = Anchor.Centre,
                //        },
                //        new OsuSpriteText
                //        {
                //            Text = "没有歌词",
                //            Font = OsuFont.GetFont(size: 45, weight: FontWeight.Bold),
                //            Anchor = Anchor.Centre,
                //            Origin = Anchor.Centre,
                //        }
                //    }
                //}
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
                p.Value.Equals(plugin.Lyrics.FindLast(l => plugin.GetCurrentTrack().CurrentTime >= l.Time)))?.Y ?? 0;

            if (pos + LyricScroll.DrawHeight > LyricFlow.Height)
                LyricScroll.ScrollToEnd();
            else
                LyricScroll.ScrollTo(pos);
        }

        protected virtual void RefreshLrcInfo(List<Lyric> lyrics)
        {
            LyricFlow.Clear();
            LyricScroll.ScrollToStart();

            foreach (var t in lyrics)
            {
                LyricFlow.Add(CreateDrawableLyric(t));
            }
        }
    }
}
