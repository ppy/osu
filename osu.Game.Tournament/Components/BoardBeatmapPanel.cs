// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class BoardBeatmapPanel : CompositeDrawable
    {
        public readonly IBeatmapInfo? Beatmap;

        private readonly string index;

        private readonly string mod;

        public const float HEIGHT = 150;

        private const float CONTAINER_HEIGHT = 50;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private Box flash = null!;

        private SpriteIcon icon = null!;

        public BoardBeatmapPanel(IBeatmapInfo? beatmap, string mod = "", string index = "")
        {
            Beatmap = beatmap;
            this.index = index;
            this.mod = mod;

            Width = HEIGHT;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
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
                    Alpha = 0.5f
                },
                new NoUnloadBeatmapSetCover
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.5f),
                    OnlineInfo = (Beatmap as IBeatmapSetOnlineInfo),
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Padding = new MarginPadding(15),
                    Direction = FillDirection.Vertical,
                
                    /* This section of code adds Beatmap Information to the Board grid. */
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = CONTAINER_HEIGHT, // Set a fixed height for consistency
                            Width = 0.87f, // Ensure the container takes up the full width
                            Children = new Drawable[]
                            {
                                new TextFlowContainer
                                {
                                    AutoSizeAxes = Axes.Y,
                                    RelativeSizeAxes = Axes.X,
                                    TextAnchor = Anchor.TopLeft,
                                    Width = 1f, // Ensure the container takes up the full width
                                    Margin = new MarginPadding { Top = -30, Left = -8 }, // Adjust padding as needed
                                }.With(t => t.AddParagraph(Beatmap?.GetDisplayTitleRomanisable(false, false) ?? (LocalisableString)@"unknown", s =>
                                {
                                    s.Font = OsuFont.Torus.With(weight: FontWeight.Bold);
                                }))
                            }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Margin = new MarginPadding { Left = -7 }, // Adjust this value to change the distance
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Text = "mapper",
                                    Padding = new MarginPadding { Right = 5 },
                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 14),
                                    Margin = new MarginPadding { Right = 10 }, // Adjusts the space to the right of the mapper label
                                },
                                new TournamentSpriteText
                                {
                                    Text = Beatmap?.Metadata.Author.Username ?? "unknown",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 14),
                                    MaxWidth = 79,
                                    Margin = new MarginPadding { Right = 20 }, // Adjusts the space to the right of the mapper name
                                },
                            }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Margin = new MarginPadding { Left = -7 }, // Adjust this value to change the distance
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Text = "difficulty",
                                    Padding = new MarginPadding { Right = 5 },
                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 14),
                                    Margin = new MarginPadding { Right = 10 }, // Adjusts the space to the right of the difficulty label
                                },
                                new TournamentSpriteText
                                {
                                    Text = Beatmap?.DifficultyName ?? "unknown",
                                    MaxWidth = 75,
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 14),
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
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Size = new osuTK.Vector2(0.4f),
                    Alpha = 0
                }
            });

            if (!string.IsNullOrEmpty(mod))
            {
                AddInternal(new TournamentModIcon(index.IsNull() ? mod : mod + index)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding(10),
                    Width = 60,
                    RelativeSizeAxes = Axes.Y,
                    Position = new osuTK.Vector2(40, -17) // (x, y). Increment of x = Move right; Increment of y = Move upwards. 
                });
            }
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            if (match.OldValue != null)
                match.OldValue.PicksBans.CollectionChanged -= picksBansOnCollectionChanged;
            if (match.NewValue != null)
                match.NewValue.PicksBans.CollectionChanged += picksBansOnCollectionChanged;

            Scheduler.AddOnce(updateState);
        }

        private void picksBansOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => Scheduler.AddOnce(updateState);

        private BeatmapChoice? choice;

        private void updateState()
        {
            if (currentMatch.Value == null)
            {
                return;
            }

            var newChoice = currentMatch.Value.PicksBans.FirstOrDefault(p => p.BeatmapID == Beatmap?.OnlineID);

            bool shouldFlash = newChoice != choice;

            if (newChoice != null)
            {
                if (shouldFlash)
                {
                    flash.FadeOutFromOne(900).Loop(0, 3);
                    icon.FadeInFromZero(500);
                }

                BorderThickness = 6;

                BorderColour = TournamentGame.GetTeamColour(newChoice.Team);

                switch (newChoice.Type)
                {
                    case ChoiceType.Pick:
                        Colour = Color4.White;
                        Alpha = 0.7f;
                        icon.Icon = FontAwesome.Solid.Check;
                        break;

                    case ChoiceType.Ban:
                        Colour = Color4.Gray;
                        Alpha = 0.3f;
                        icon.Icon = FontAwesome.Solid.Ban;
                        break;

                    case ChoiceType.Protect:
                        Alpha = 0.4f;
                        icon.Icon = FontAwesome.Solid.Lock;
                        icon.Colour = new OsuColour().Cyan;
                        icon.Alpha = 1;
                        break;

                    case ChoiceType.RedWin:
                        Alpha = 1f;
                        icon.Icon = FontAwesome.Solid.Trophy;
                        icon.Colour = new OsuColour().Red;
                        icon.Alpha = 0.73f; // Added this line to distinguish last win from other wins
                        break;

                    case ChoiceType.BlueWin:
                        Alpha = 1f;
                        icon.Icon = FontAwesome.Solid.Trophy;
                        icon.Colour = new OsuColour().Blue;
                        icon.Alpha = 0.73f; // Added this line to distinguish last win from other wins
                        break;

                    case ChoiceType.Trap:
                        Alpha = 0.4f;
                        icon.Icon = FontAwesome.Solid.ExclamationCircle;
                        icon.Colour = Color4.White;
                        icon.Alpha = 1;
                        break;

                    case ChoiceType.Draw:
                        Alpha = 0.4f;
                        icon.Icon = FontAwesome.Solid.BalanceScale;
                        icon.Colour = Color4.White;
                        break;

                }
            }
            else
            {
                Colour = Color4.White;
                BorderThickness = 0;
                Alpha = 1;
                icon.Alpha = 0;
                icon.Colour = Color4.White;
            }

            choice = newChoice;
        }

        private partial class NoUnloadBeatmapSetCover : UpdateableOnlineBeatmapSetCover
        {
            // As covers are displayed on stream, we want them to load as soon as possible.
            protected override double LoadDelay => 0;

            // Use DelayedLoadWrapper to avoid content unloading when switching away to another screen.
            protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Func<Drawable> createContentFunc, double timeBeforeLoad)
                => new DelayedLoadWrapper(createContentFunc(), timeBeforeLoad);
        }
    }
}
