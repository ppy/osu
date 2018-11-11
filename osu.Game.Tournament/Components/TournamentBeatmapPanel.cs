// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Screens.Ladder.Components;
using OpenTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public class TournamentBeatmapPanel : CompositeDrawable
    {
        public readonly BeatmapInfo Beatmap;

        private const float horizontal_padding = 10;
        private const float vertical_padding = 5;

        public const float HEIGHT = 50;

        private readonly Bindable<MatchPairing> currentMatch = new Bindable<MatchPairing>();
        private Box flash;

        public TournamentBeatmapPanel(BeatmapInfo beatmap)
        {
            Beatmap = beatmap;
            Width = 400;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);

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
                    BeatmapSet = Beatmap.BeatmapSet,
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
                                $"{Beatmap.Metadata.ArtistUnicode} - {Beatmap.Metadata.TitleUnicode}",
                                $"{Beatmap.Metadata.Artist} - {Beatmap.Metadata.Title}")),
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
                                    Text = Beatmap.Metadata.AuthorString,
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
                                    Text = Beatmap.Version,
                                    Font = @"Exo2.0-BoldItalic",
                                    TextSize = 14
                                },
                            }
                        }
                    },
                },
                flash = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray,
                    Blending = BlendingMode.Additive,
                    Alpha = 0,
                },
            });
        }

        private void matchChanged(MatchPairing match)
        {
            match.PicksBans.CollectionChanged += picksBansOnCollectionChanged;
            updateState();
        }

        private BeatmapChoice choice;

        private void updateState()
        {
            var found = currentMatch.Value.PicksBans.FirstOrDefault(p => p.BeatmapID == Beatmap.OnlineBeatmapID);

            bool doFlash = found != choice;
            choice = found;

            if (found != null)
            {
                if (doFlash)
                    flash?.FadeOutFromOne(500).Loop(0, 10);

                BorderThickness = 6;

                switch (found.Team)
                {
                    case TeamColour.Red:
                        BorderColour = Color4.Red;
                        break;
                    case TeamColour.Blue:
                        BorderColour = Color4.Blue;
                        break;
                }

                switch (found.Type)
                {
                    case ChoiceType.Pick:
                        Colour = Color4.White;
                        Alpha = 1;
                        break;
                    case ChoiceType.Ban:
                        Colour = Color4.Gray;
                        Alpha = 0.5f;
                        break;
                }
            }
            else
            {
                Colour = Color4.White;
                BorderThickness = 0;
                Alpha = 1;
            }
        }

        private void picksBansOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var list = (ObservableCollection<BeatmapChoice>)sender;
            if (sender != currentMatch.Value.PicksBans)
            {
                // todo: we need a last attribute in bindable valuechanged events badly.
                list.CollectionChanged -= picksBansOnCollectionChanged;
                return;
            }

            updateState();
        }
    }
}
