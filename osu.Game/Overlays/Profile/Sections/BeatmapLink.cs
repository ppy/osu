// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Profile.Sections
{
    public class BeatmapLink : ClickableLink
    {
        public BeatmapLink(BeatmapInfo beatmap, BeatmapLinkType type = BeatmapLinkType.TitleVersionAuthor, int textSize = 20)
                : base(beatmap)
        {
            switch (type)
            {
                case BeatmapLinkType.TitleVersionAuthor:
                    TextContent.AddRange(new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = new LocalisedString((
                                $"{Beatmap.Metadata.TitleUnicode ?? Beatmap.Metadata.Title} [{Beatmap.Version}] ",
                                $"{Beatmap.Metadata.Title ?? Beatmap.Metadata.TitleUnicode} [{Beatmap.Version}] ")),
                            Font = OsuFont.GetFont(size: textSize, weight: FontWeight.Bold)
                        },
                        new OsuSpriteText
                        {
                            Text = "by " + new LocalisedString((Beatmap.Metadata.ArtistUnicode, Beatmap.Metadata.Artist)),
                            Font = OsuFont.GetFont(size: textSize, weight: FontWeight.Regular)
                        },
                    });
                    break;
                case BeatmapLinkType.TitleAuthor:
                    TextContent.AddRange(new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = new LocalisedString((
                                $"{Beatmap.Metadata.TitleUnicode ?? Beatmap.Metadata.Title} ",
                                $"{Beatmap.Metadata.Title ?? Beatmap.Metadata.TitleUnicode} ")),
                            Font = OsuFont.GetFont(size: textSize, weight: FontWeight.Regular, italics: true)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Text = "by " + new LocalisedString((Beatmap.Metadata.ArtistUnicode, Beatmap.Metadata.Artist)),
                            Font = OsuFont.GetFont(size: textSize - 5, weight: FontWeight.Regular, italics: true)
                        },
                    });
                    break;
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapSetOverlay beatmapSetOverlay)
        {
            ClickAction = () =>
            {
                if (Beatmap.OnlineBeatmapID != null)
                    beatmapSetOverlay?.FetchAndShowBeatmap(Beatmap.OnlineBeatmapID.Value);
                else if (Beatmap.BeatmapSet?.OnlineBeatmapSetID != null)
                    beatmapSetOverlay?.FetchAndShowBeatmapSet(Beatmap.BeatmapSet.OnlineBeatmapSetID.Value);
            };
        }
    }

    public enum BeatmapLinkType
    {
        TitleVersionAuthor = 0,
        TitleAuthor = 1
    }
}
