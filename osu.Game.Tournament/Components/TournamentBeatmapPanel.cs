// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public class TournamentBeatmapPanel : CompositeDrawable
    {
        public readonly BeatmapInfo Beatmap;
        private readonly string mods;

        private const float horizontal_padding = 10;
        private const float vertical_padding = 5;

        public const float HEIGHT = 50;

        private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();
        private Box flash;

        public TournamentBeatmapPanel(BeatmapInfo beatmap, string mods = null)
        {
            if (beatmap == null) throw new ArgumentNullException(nameof(beatmap));

            Beatmap = beatmap;
            this.mods = mods;
            Width = 400;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, TextureStore textures)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);

            CornerRadius = HEIGHT / 2;
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
                                $"{Beatmap.Metadata.ArtistUnicode ?? Beatmap.Metadata.Artist} - {Beatmap.Metadata.TitleUnicode ?? Beatmap.Metadata.Title}",
                                $"{Beatmap.Metadata.Artist} - {Beatmap.Metadata.Title}")),
                            Font = OsuFont.GetFont(weight: FontWeight.Bold, italics: true),
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
                                    Padding = new MarginPadding { Right = 5 },
                                    Font = OsuFont.GetFont(italics: true, weight: FontWeight.Regular, size: 14)
                                },
                                new OsuSpriteText
                                {
                                    Text = Beatmap.Metadata.AuthorString,
                                    Padding = new MarginPadding { Right = 20 },
                                    Font = OsuFont.GetFont(italics: true, weight: FontWeight.Bold, size: 14)
                                },
                                new OsuSpriteText
                                {
                                    Text = "difficulty",
                                    Padding = new MarginPadding { Right = 5 },
                                    Font = OsuFont.GetFont(italics: true, weight: FontWeight.Regular, size: 14)
                                },
                                new OsuSpriteText
                                {
                                    Text = Beatmap.Version,
                                    Font = OsuFont.GetFont(italics: true, weight: FontWeight.Bold, size: 14)
                                },
                            }
                        }
                    },
                },
                flash = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
            });

            if (!string.IsNullOrEmpty(mods))
                AddInternal(new Sprite
                {
                    Texture = textures.Get($"mods/{mods}"),
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding(20),
                    Scale = new Vector2(0.5f)
                });
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch> match)
        {
            if (match.OldValue != null)
                match.OldValue.PicksBans.CollectionChanged -= picksBansOnCollectionChanged;
            match.NewValue.PicksBans.CollectionChanged += picksBansOnCollectionChanged;
            updateState();
        }

        private void picksBansOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            => updateState();

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
    }
}
