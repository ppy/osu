// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public class TournamentBeatmapPanel : CompositeDrawable
    {
        private readonly BeatmapInfo beatmap;
        private const float horizontal_padding = 10;
        private const float vertical_padding = 5;

        public TournamentBeatmapPanel(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
            Width = 400;
            Height = 50;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            CornerRadius = 25;
            Masking = true;

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                new UpdateableBeatmapSetCover
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.5f),
                    BeatmapSet = beatmap.BeatmapSet,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding(vertical_padding),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = new LocalisedString((
                                $"{beatmap.Metadata.ArtistUnicode} - {beatmap.Metadata.TitleUnicode}",
                                $"{beatmap.Metadata.Artist} - {beatmap.Metadata.Title}")),
                            Font = @"Exo2.0-BoldItalic",
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Padding = new MarginPadding(vertical_padding),
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "mapper",
                                    Font = @"Exo2.0-RegularItalic",
                                    Padding = new MarginPadding { Right = 5 },
                                    TextSize = 14
                                },
                                new OsuSpriteText
                                {
                                    Text = beatmap.Metadata.AuthorString,
                                    Font = @"Exo2.0-BoldItalic",
                                    Padding = new MarginPadding { Right = 20 },
                                    TextSize = 14
                                },
                                new OsuSpriteText
                                {
                                    Text = "difficulty",
                                    Font = @"Exo2.0-RegularItalic",
                                    Padding = new MarginPadding { Right = 5 },
                                    TextSize = 14
                                },
                                new OsuSpriteText
                                {
                                    Text = beatmap.Version,
                                    Font = @"Exo2.0-BoldItalic",
                                    TextSize = 14
                                },
                            }
                        }
                    },
                },
            });
        }
    }
}
