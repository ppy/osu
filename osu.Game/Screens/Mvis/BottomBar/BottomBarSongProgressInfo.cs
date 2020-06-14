using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Mvis.UI
{
    public class BottomBarSongProgressInfo : FillFlowContainer
    {
        public OsuSpriteText timeCurrent;
        public OsuSpriteText timeTotal;
        public float FontSize = 19;

        [Resolved]
        private IBindable<WorkingBeatmap> b { get; set; }

        private string formatTime(TimeSpan timeSpan) => $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{Math.Floor(timeSpan.Duration().TotalMinutes)}:{timeSpan.Duration().Seconds:D2}";

        public BottomBarSongProgressInfo()
        {
            Spacing = new Vector2(5);
            Children = new Drawable[]
            {
                timeCurrent = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: FontSize),
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                },
                new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: FontSize),
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    Text = "/",
                },
                timeTotal = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: FontSize),
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            b.ValueChanged += _  => UpdateEndTime();

            UpdateEndTime();
        }

        protected override void Update()
        {
            var Track = b.Value?.TrackLoaded ?? false ? b.Value.Track : null;
            if (Track?.IsDummyDevice == false)
            {
                int currentSecond = (int)Math.Floor(Track.CurrentTime / 1000.0);
                timeCurrent.Text = formatTime(TimeSpan.FromSeconds(currentSecond));
            }
            else
            {
                timeCurrent.Text = "???";
                timeTotal.Text = "???";
            }
        }

        private void UpdateEndTime()
        {
            this.Schedule(() =>
            {
                timeTotal.Text = formatTime(TimeSpan.FromMilliseconds(b.Value.Track.Length));
            });
        }
    }
}