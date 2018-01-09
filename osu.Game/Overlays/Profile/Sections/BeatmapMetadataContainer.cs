// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
namespace osu.Game.Overlays.Profile.Sections
{
    /// <summary>
    /// Display artist/title/mapper information, commonly used as the left portion of a profile or score display row (see <see cref="DrawableProfileRow"/>).
    /// </summary>
    public class BeatmapMetadataContainer : OsuHoverContainer, IHasTooltip
    {
        private readonly BeatmapInfo beatmap;

        public BeatmapMetadataContainer(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
            AutoSizeAxes = Axes.Both;
            TooltipText = $"{beatmap.Metadata.Artist} - {beatmap.Metadata.Title}";
        }

        public string TooltipText { get; }

        [BackgroundDependencyLoader(true)]
        private void load(LocalisationEngine locale, BeatmapSetOverlay beatmapSetOverlay)
        {
            Action = () =>
            {
                if (beatmap.OnlineBeatmapSetID.HasValue) beatmapSetOverlay?.ShowBeatmapSet(beatmap.OnlineBeatmapSetID.Value);
            };

            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Current = locale.GetUnicodePreference(
                            $"{beatmap.Metadata.TitleUnicode ?? beatmap.Metadata.Title} [{beatmap.Version}] ",
                            $"{beatmap.Metadata.Title ?? beatmap.Metadata.TitleUnicode} [{beatmap.Version}] "
                        ),
                        TextSize = 15,
                        Font = "Exo2.0-SemiBoldItalic",
                    },
                    new OsuSpriteText
                    {
                        Current = locale.GetUnicodePreference(beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist),
                        TextSize = 12,
                        Padding = new MarginPadding { Top = 3 },
                        Font = "Exo2.0-RegularItalic",
                    },
                },
            };
        }
    }
}
