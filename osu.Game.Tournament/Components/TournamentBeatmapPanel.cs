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
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public class TournamentBeatmapPanel : CompositeDrawable
    {
        public readonly BeatmapInfo Beatmap;
        private readonly string mods;

        private const float horizontal_padding = 10;
        private const float vertical_padding = 10;

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
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Padding = new MarginPadding(15),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Text = new LocalisedString((
                                $"{Beatmap.Metadata.ArtistUnicode ?? Beatmap.Metadata.Artist} - {Beatmap.Metadata.TitleUnicode ?? Beatmap.Metadata.Title}",
                                $"{Beatmap.Metadata.Artist} - {Beatmap.Metadata.Title}")),
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold),
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Text = "mapper",
                                    Padding = new MarginPadding { Right = 5 },
                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 14)
                                },
                                new TournamentSpriteText
                                {
                                    Text = Beatmap.Metadata.AuthorString,
                                    Padding = new MarginPadding { Right = 20 },
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 14)
                                },
                                new TournamentSpriteText
                                {
                                    Text = "difficulty",
                                    Padding = new MarginPadding { Right = 5 },
                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 14)
                                },
                                new TournamentSpriteText
                                {
                                    Text = Beatmap.Version,
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 14)
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
            {
                AddInternal(new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 60,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding(10),
                    Child = new Sprite
                    {
                        FillMode = FillMode.Fit,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Texture = textures.Get($"mods/{mods}"),
                    }
                });
            }
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

                BorderColour = TournamentGame.GetTeamColour(found.Team);

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
