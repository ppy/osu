// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osu.Game.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Menu
{
    public partial class SongTicker : Container
    {
        private const int fade_duration = 800;

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; } = null!;

        private readonly OsuSpriteText title, artist;

        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

        public SongTicker()
        {
            AutoSizeAxes = Axes.Both;
            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 3),
                Children = new Drawable[]
                {
                    title = new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Font = OsuFont.GetFont(size: 24, weight: FontWeight.Light, italics: true)
                    },
                    artist = new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Font = OsuFont.GetFont(size: 16)
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(_ => Scheduler.AddOnce(show), true);
        }

        private void show()
        {
            var metadata = beatmap.Value.Metadata;

            title.Text = new RomanisableString(metadata.TitleUnicode, metadata.Title);
            artist.Text = new RomanisableString(metadata.ArtistUnicode, metadata.Artist);

            this.FadeInFromZero(fade_duration / 2f)
                .Delay(4000)
                .Then().FadeOut(fade_duration);
        }
    }
}
