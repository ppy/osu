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
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentBeatmapPanel : CompositeDrawable
    {
        public readonly IBeatmapInfo? Beatmap;

        private readonly string mod;

        public const float HEIGHT = 50;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private Box flash = null!;
        private Container borderBox = null!;
        private TournamentProtectIcon protectIcon = null!;
        private FillFlowContainer modIconContainer = null!;

        public TournamentBeatmapPanel(IBeatmapInfo? beatmap, string mod = "")
        {
            Beatmap = beatmap;
            this.mod = mod;

            Width = 400;
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
                borderBox = new Container
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
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
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Text = Beatmap?.GetDisplayTitleRomanisable(false, false) ?? (LocalisableString)@"unknown",
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
                                            Text = Beatmap?.Metadata.Author.Username ?? "unknown",
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
                                            Text = Beatmap?.DifficultyName ?? "unknown",
                                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 14)
                                        },
                                    }
                                }
                            },
                        },
                    }
                },
                modIconContainer = new FillFlowContainer
                {
                    Name = "Mod icon container",
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding(16),
                    Width = 60,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                },
                protectIcon = new TournamentProtectIcon
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Alpha = 1,
                    Width = Height,
                    Height = Height,
                },
                flash = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
            });

            if (!string.IsNullOrEmpty(mod))
            {
                modIconContainer.Insert(-1, new TournamentModIcon(mod)
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Both
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

            // protected?
            var protectedChoice = currentMatch.Value.PicksBans.FirstOrDefault(p => p.BeatmapID == Beatmap?.OnlineID && p.Type == ChoiceType.Protect);

            if (protectedChoice != null)
                protectIcon.TeamColour = protectedChoice.Team;
            else
                protectIcon.TeamColour = null;

            var newChoice = currentMatch.Value.PicksBans.LastOrDefault(p => p.BeatmapID == Beatmap?.OnlineID && p.Type != ChoiceType.Protect);

            bool shouldFlash = newChoice != choice;

            if (newChoice != null)
            {
                if (shouldFlash)
                    flash.FadeOutFromOne(500).Loop(0, 10);

                borderBox.BorderThickness = 6;
                borderBox.BorderColour = TournamentGame.GetTeamColour(newChoice.Team);

                switch (newChoice.Type)
                {
                    case ChoiceType.Pick:
                        borderBox.Colour = Color4.White;
                        borderBox.Alpha = 1;
                        break;

                    case ChoiceType.Ban:
                        borderBox.Colour = Color4.Gray;
                        borderBox.Alpha = 0.5f;
                        break;
                }
            }
            else
            {
                borderBox.Colour = Color4.White;
                borderBox.BorderThickness = 0;
                borderBox.Alpha = 1;
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
