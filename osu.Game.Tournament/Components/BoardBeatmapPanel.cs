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

    public static class StringExtensions
    {
        public static string TruncateWithEllipsis(this string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }
    }

    public partial class BoardBeatmapPanel : CompositeDrawable
    {
        public readonly IBeatmapInfo? Beatmap;

        private readonly string index;

        private readonly string mod;

        public const float HEIGHT = 150;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private Box backgroundAddition = null!;
        private Box flash = null!;

        private SpriteIcon icon = null!;
        private SpriteIcon statusIcon = null!;

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


            var displayTitle = Beatmap?.GetDisplayTitleRomanisable(false, false) ?? (LocalisableString)@"unknown";
            string songName = displayTitle.ToString().Split('-').Last().Trim();
            string truncatedSongName = songName.TruncateWithEllipsis(17);

            string displayDifficulty = Beatmap?.DifficultyName ?? "unknown";
            string difficultyName = displayDifficulty.ToString().Split('-').Last().Trim();
            string truncatedDifficultyName = difficultyName.TruncateWithEllipsis(19);

            Masking = true;

            AddRangeInternal(new Drawable[]
            {
                new NoUnloadBeatmapSetCover
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.5f),
                    OnlineInfo = (Beatmap as IBeatmapSetOnlineInfo),
                },
                backgroundAddition = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.1f
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Padding = new MarginPadding(15),
                    Direction = FillDirection.Vertical,
                
                    /* This section of code adds Beatmap Information to the Board grid. */
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Text = truncatedSongName,
                            Padding = new MarginPadding { Left = 0 },
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 18),
                            Margin = new MarginPadding { Left = -9, Top = -7 },
                        },
                        /* Disable text display
                        new TextFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            TextAnchor = Anchor.TopLeft,
                            Width = 1f, // Ensure the container takes up the full width
                            Margin = new MarginPadding { Left = -8 }, // Adjust padding as needed
                        }.With(t => t.AddParagraph(Beatmap?.GetDisplayTitleRomanisable(false, false) ?? (LocalisableString)@"unknown", s =>
                        {
                            s.Font = OsuFont.Torus.With(weight: FontWeight.Bold);
                        })),*/
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Margin = new MarginPadding { Left = -7 }, // Adjust this value to change the distance
                            Children = new Drawable[]
                            {
                                /* Disable text display
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
                                },*/
                            }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Margin = new MarginPadding { Left = -9, Top = 5 }, // Adjust this value to change the distance
                            Children = new Drawable[]
                            {
                                /* Disable "difficulty" display
                                new TournamentSpriteText
                                {
                                    Text = "difficulty",
                                    Padding = new MarginPadding { Right = 5 },
                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 14),
                                    Margin = new MarginPadding { Right = 10 }, // Adjusts the space to the right of the difficulty label
                                },*/
                                new TournamentSpriteText
                                {
                                    Text = truncatedDifficultyName,
                                    MaxWidth = 120,
                                    Font = OsuFont.Torus.With(weight: FontWeight.Medium, size: 14),
                                },
                            }
                        }
                    },
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Size = new osuTK.Vector2(0.4f),
                    Alpha = 0,
                },
                statusIcon = new SpriteIcon
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Colour = Color4.White,
                    Size = new osuTK.Vector2(24),
                    Position = new osuTK.Vector2(5, -5),
                    Alpha = 0,
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
            {
                match.OldValue.PicksBans.CollectionChanged -= picksBansOnCollectionChanged;
                match.OldValue.Protects.CollectionChanged -= picksBansOnCollectionChanged;
                match.OldValue.Traps.CollectionChanged -= picksBansOnCollectionChanged;
            }
            if (match.NewValue != null)
            {
                match.NewValue.PicksBans.CollectionChanged += picksBansOnCollectionChanged;
                match.NewValue.Protects.CollectionChanged += picksBansOnCollectionChanged;
                match.NewValue.Traps.CollectionChanged += picksBansOnCollectionChanged;
            }

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

            bool isProtected = currentMatch.Value.Protects.Any(p => p.BeatmapID == Beatmap?.OnlineID);

            bool isTrapped = currentMatch.Value.Traps.Any(p => p.BeatmapID == Beatmap?.OnlineID && !p.IsTriggered);

            bool isBothTrapped = currentMatch.Value.Traps.Any(p => (p.BeatmapID == Beatmap?.OnlineID && p.Team == TeamColour.Red && !p.IsTriggered))
                && currentMatch.Value.Traps.Any(p => (p.BeatmapID == Beatmap?.OnlineID && p.Team == TeamColour.Blue && !p.IsTriggered));

            var newChoice = currentMatch.Value.PicksBans.LastOrDefault(p => p.BeatmapID == Beatmap?.OnlineID);

            var nextPureChoice = newChoice;

            // Check: We need this?
            if (isProtected) nextPureChoice = currentMatch.Value.PicksBans.LastOrDefault(p => (p.BeatmapID == Beatmap?.OnlineID && p.Type != ChoiceType.Protect));
            else if (isTrapped) nextPureChoice = currentMatch.Value.PicksBans.LastOrDefault(p => (p.BeatmapID == Beatmap?.OnlineID && p.Type != ChoiceType.Trap));

            // newChoice = nextPureChoice ?? newChoice;

            bool shouldFlash = newChoice != choice;

            if (newChoice != null)
            {
                if (shouldFlash)
                {
                    flash.FadeOutFromOne(duration: 900, easing: Easing.OutSine).Loop(0, 3);
                }

                BorderThickness = 4;

                BorderColour = TournamentGame.GetTeamColour(newChoice.Team);

                switch (newChoice.Type)
                {
                    case ChoiceType.Pick:
                        Colour = Color4.White;
                        Alpha = 1f;
                        backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                        icon.FadeOut(duration: 100, easing: Easing.OutCubic);
                        break;

                    // Ban: All darker
                    case ChoiceType.Ban:
                        backgroundAddition.Colour = Color4.Gray;
                        backgroundAddition.FadeTo(newAlpha: 0.7f, duration: 150, easing: Easing.InCubic);
                        icon.Icon = FontAwesome.Solid.Ban;
                        icon.Colour = newChoice.Team == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky;
                        icon.FadeIn(duration: 200, easing: Easing.InCubic);
                        BorderColour = Color4.White;
                        BorderThickness = 0;
                        break;

                    case ChoiceType.Protect:
                        Alpha = 1f;
                        backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                        statusIcon.FadeIn(duration: 150, easing: Easing.InCubic);
                        statusIcon.Icon = FontAwesome.Solid.Lock;
                        statusIcon.Colour = newChoice.Team == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky;
                        break;

                    // Win: Background colour
                    case ChoiceType.RedWin:
                        backgroundAddition.Colour = Color4.Red;
                        backgroundAddition.FadeTo(newAlpha: 0.4f, duration: 150, easing: Easing.InCubic);
                        icon.FadeIn(duration: 150, easing: Easing.InCubic);
                        icon.Icon = FontAwesome.Solid.Trophy;
                        icon.Colour = new OsuColour().Red;
                        // icon.Colour = isProtected ? new OsuColour().Pink : Color4.Red;
                        /* Commented out this line, as it will cause some degree of visual distraction.
                        icon.Alpha = 0.73f; // Added this line to distinguish last win from other wins */
                        break;

                    case ChoiceType.BlueWin:
                        backgroundAddition.Colour = new OsuColour().Sky;
                        backgroundAddition.FadeTo(newAlpha: 0.5f, duration: 150, easing: Easing.InCubic);
                        icon.FadeIn(duration: 150, easing: Easing.InCubic);
                        icon.Icon = FontAwesome.Solid.Trophy;
                        icon.Colour = new OsuColour().Blue;
                        // icon.Colour = isProtected ? new OsuColour().Sky : Color4.Blue;
                        /* Commented out this line, as it will cause some degree of visual distraction.
                        icon.Alpha = 0.73f; // Added this line to distinguish last win from other wins */
                        break;

                    case ChoiceType.Trap:
                        Alpha = 1f;
                        backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                        statusIcon.FadeIn(duration: 150, easing: Easing.InCubic);
                        statusIcon.Icon = FontAwesome.Solid.ExclamationCircle;
                        statusIcon.Colour = isBothTrapped ? Color4.White : (newChoice.Team == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky);
                        BorderColour = Color4.White;
                        break;
                }
            }
            else
            {
                // Stop all transforms first, to make relative properties adjustable.
                icon.ClearTransforms();
                statusIcon.ClearTransforms();
                flash.ClearTransforms();

                // Then we can change them to the default state.
                BorderThickness = 0;
                flash.Alpha = 0;
                this.FadeIn(duration: 100, easing: Easing.InCubic);
                backgroundAddition.FadeOut(duration: 100, easing: Easing.OutCubic);
                icon.FadeOut(duration: 100, easing: Easing.OutCubic);
                statusIcon.FadeOut(duration: 100, easing: Easing.OutCubic);
                Colour = Color4.White;
                icon.Colour = Color4.White;
                backgroundAddition.Colour = Color4.White;
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
