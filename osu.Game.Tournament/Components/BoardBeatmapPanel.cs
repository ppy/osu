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

        public readonly string Index;

        public readonly string Mod;

        public const float HEIGHT = 150;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private FillFlowContainer textArea = null!;

        private Box backgroundAddition = null!;
        private Box flash = null!;

        private SpriteIcon icon = null!;
        private SpriteIcon swapIcon = null!;
        private SpriteIcon protectIcon = null!;
        private SpriteIcon trapIcon = null!;

        // Real X and Y positions on the board, distinct from RoundBeatmap.BoardX and BoardY.
        public int RealX;
        public int RealY;

        public BoardBeatmapPanel(IBeatmapInfo? beatmap, string mod = "", string index = "", int initX = 1, int initY = 1)
        {
            Beatmap = beatmap;
            Index = index;
            Mod = mod;
            RealX = initX;
            RealY = initY;

            Width = HEIGHT;
            Height = HEIGHT;
        }


        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);

            var displayTitle = Beatmap?.GetDisplayTitleRomanisable(false, false) ?? (LocalisableString)@"unknown";

            string[] songNameList = displayTitle.ToString().Split(' ');

            int firstHyphenIndex = 0;

            // Find the first " - " (Hopefully it isn't in the Artists field)
            for (int i = 0; i < songNameList.Count(); i++)
            {
                string obj = songNameList.ElementAt(i);
                if (obj == "-")
                {
                    firstHyphenIndex = i;
                    break;
                }
            }

            var TitleList = songNameList.Skip(firstHyphenIndex + 1);

            // Re-construct
            string songName = string.Empty;
            for (int i = 0; i < TitleList.Count(); i++)
            {
                songName += TitleList.ElementAt(i).Trim();
                if (i != TitleList.Count() - 1) songName += ' ';
            }

            string truncatedSongName = songName.Trim().TruncateWithEllipsis(17);

            string displayDifficulty = Beatmap?.DifficultyName ?? "unknown";
            string truncatedDifficultyName = displayDifficulty.TruncateWithEllipsis(19);

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
                textArea = new FillFlowContainer
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
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Margin = new MarginPadding { Left = -9, Top = 5 }, // Adjust this value to change the distance
                            Child = new TournamentSpriteText
                            {
                                Text = truncatedDifficultyName,
                                MaxWidth = 120,
                                Font = OsuFont.Torus.With(weight: FontWeight.Medium, size: 14),
                            },
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
                swapIcon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Yellow,
                    Size = new osuTK.Vector2(0.4f),
                    Icon = FontAwesome.Solid.ExchangeAlt,
                    Alpha = 0,
                },
                protectIcon = new SpriteIcon
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Icon = FontAwesome.Solid.Lock,
                    Colour = Color4.White,
                    Size = new osuTK.Vector2(24),
                    Position = new osuTK.Vector2(5, -5),
                    Alpha = 0,
                },
                trapIcon = new SpriteIcon
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Icon = FontAwesome.Solid.ExclamationCircle,
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

            if (!string.IsNullOrEmpty(Mod))
            {
                AddInternal(new TournamentModIcon(Index.IsNull() ? Mod : Mod + Index)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding(10),
                    Width = 60,
                    Size = new osuTK.Vector2(80),
                    RelativeSizeAxes = Axes.Y,
                    Position = new osuTK.Vector2(34, -18) // (x, y). Increment of x = Move right; Decrement of y = Move upwards. 
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
                match.OldValue.PendingSwaps.CollectionChanged -= picksBansOnCollectionChanged;
            }
            if (match.NewValue != null)
            {
                match.NewValue.PicksBans.CollectionChanged += picksBansOnCollectionChanged;
                match.NewValue.Protects.CollectionChanged += picksBansOnCollectionChanged;
                match.NewValue.Traps.CollectionChanged += picksBansOnCollectionChanged;
                match.NewValue.PendingSwaps.CollectionChanged += picksBansOnCollectionChanged;
            }

            Scheduler.AddOnce(updateState);
        }

        private void picksBansOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => Scheduler.AddOnce(updateState);

        private BeatmapChoice? bpChoice, pChoice;
        private TrapInfo? tChoice;

        private void updateState()
        {
            if (currentMatch.Value == null)
            {
                return;
            }

            bool isBothTrapped = currentMatch.Value.Traps.Any(p => p.BeatmapID == Beatmap?.OnlineID && p.Team == TeamColour.Red)
                && currentMatch.Value.Traps.Any(p => p.BeatmapID == Beatmap?.OnlineID && p.Team == TeamColour.Blue);

            var newBPChoice = currentMatch.Value.PicksBans.LastOrDefault(p => p.BeatmapID == Beatmap?.OnlineID);

            var protectChoice = currentMatch.Value.Protects.LastOrDefault(p => p.BeatmapID == Beatmap?.OnlineID);

            var trapChoice = currentMatch.Value.Traps.LastOrDefault(p => p.BeatmapID == Beatmap?.OnlineID);

            var swapChoice = currentMatch.Value.PendingSwaps.LastOrDefault(p => p.BeatmapID == Beatmap?.OnlineID);

            var pickerChoice = currentMatch.Value.PicksBans.LastOrDefault(p => p.BeatmapID == Beatmap?.OnlineID && p.Type == ChoiceType.Pick);

            TeamColour borderColor = newBPChoice != null ? newBPChoice.Team : TeamColour.Neutral;
            TeamColour protectColor = protectChoice != null ? protectChoice.Team : TeamColour.Neutral;
            TeamColour trapColor = trapChoice != null ? trapChoice.Team : TeamColour.Neutral;

            // Flash when new changes are made.
            bool shouldFlash = newBPChoice != bpChoice || protectChoice != pChoice || trapChoice != tChoice || swapChoice != null;

            if (newBPChoice != null || protectChoice != null || trapChoice != null || swapChoice != null)
            {
                // Auto selecting is bothering us! Fight back!
                if (newBPChoice != null && !newBPChoice.Token)
                {
                    currentMatch.Value.PicksBans.Remove(newBPChoice);
                    return;
                }

                if (shouldFlash)
                {
                    flash.FadeOutFromOne(duration: 900, easing: Easing.OutSine).Loop(0, 3);
                }

                if (protectChoice != null && trapChoice != null)
                {
                    trapIcon.MoveTo(newPosition: new osuTK.Vector2(30, -5), duration: 200, easing: Easing.OutCubic);
                }
                else
                {
                    trapIcon.MoveTo(newPosition: new osuTK.Vector2(5, -5), duration: 150, easing: Easing.OutCubic);
                }

                protectIcon.FadeTo(newAlpha: protectChoice != null ? 1 : 0, duration: 200, easing: Easing.OutCubic);
                trapIcon.FadeTo(newAlpha: trapChoice != null ? 1 : 0, duration: 200, easing: Easing.OutCubic);

                BorderThickness = 4;

                if (trapChoice != null)
                {
                    Alpha = 1f;
                    backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                    trapIcon.FadeIn(duration: 150, easing: Easing.InCubic);
                    trapIcon.Colour = isBothTrapped ? Color4.White : (trapColor == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky);
                    BorderThickness = 0;
                }

                if (protectChoice != null)
                {
                    Alpha = 1f;
                    backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                    protectIcon.FadeIn(duration: 150, easing: Easing.InCubic);
                    protectIcon.Colour = protectColor == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky;
                    BorderThickness = 0;
                }

                if (pickerChoice != null)
                {
                    BorderColour = TournamentGame.GetTeamColour(pickerChoice.Team);
                }
                else
                {
                    BorderColour = Color4.White;
                }

                if (swapChoice != null)
                {
                    swapIcon.FadeInFromZero(duration: 800, easing: Easing.InCubic);
                }

                if (newBPChoice != null)
                {

                    if (newBPChoice.Type == ChoiceType.Ban)
                    {
                        textArea.FadeTo(newAlpha: 0.5f, duration: 200, easing: Easing.OutCubic);
                    }
                    else
                    {
                        textArea.FadeTo(newAlpha: 1f, duration: 200, easing: Easing.OutCubic);
                    }

                    switch (newBPChoice.Type)
                    {
                        case ChoiceType.Pick:
                            Colour = Color4.White;
                            Alpha = 1f;
                            backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                            icon.FadeOut(duration: 100, easing: Easing.OutCubic);
                            BorderColour = TournamentGame.GetTeamColour(newBPChoice.Team);
                            BorderThickness = 4;
                            break;

                        // Ban: All darker
                        case ChoiceType.Ban:
                            backgroundAddition.Colour = Color4.Black;
                            backgroundAddition.FadeTo(newAlpha: 0.7f, duration: 150, easing: Easing.InCubic);
                            icon.Icon = FontAwesome.Solid.Ban;
                            icon.Colour = newBPChoice.Team == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky;
                            icon.FadeTo(newAlpha: 0.6f, duration: 200, easing: Easing.InCubic);
                            BorderThickness = 0;
                            break;

                        // Win: Background colour
                        case ChoiceType.RedWin:
                            backgroundAddition.Colour = Color4.Red;
                            backgroundAddition.FadeTo(newAlpha: 0.4f, duration: 150, easing: Easing.InCubic);
                            icon.FadeIn(duration: 150, easing: Easing.InCubic);
                            icon.Icon = FontAwesome.Solid.Trophy;
                            icon.Colour = new OsuColour().Red;
                            BorderThickness = 4;
                            break;

                        case ChoiceType.BlueWin:
                            backgroundAddition.Colour = new OsuColour().Sky;
                            backgroundAddition.FadeTo(newAlpha: 0.5f, duration: 150, easing: Easing.InCubic);
                            icon.FadeIn(duration: 150, easing: Easing.InCubic);
                            icon.Icon = FontAwesome.Solid.Trophy;
                            icon.Colour = new OsuColour().Blue;
                            BorderThickness = 4;
                            break;

                        default:
                            icon.FadeOut(duration: 100, easing: Easing.OutCubic);
                            BorderThickness = 0;
                            break;
                    }
                }
            }
            else
            {
                // Stop all transforms first, to make relative properties adjustable.
                icon.ClearTransforms();
                protectIcon.ClearTransforms();
                trapIcon.ClearTransforms();
                flash.ClearTransforms();
                textArea.ClearTransforms();

                // Then we can change them to the default state.
                BorderThickness = 0;
                flash.FadeOut(duration: 200, easing: Easing.OutCubic);
                this.FadeIn(duration: 100, easing: Easing.InCubic);
                backgroundAddition.FadeOut(duration: 100, easing: Easing.OutCubic);
                icon.FadeOut(duration: 100, easing: Easing.OutCubic);
                protectIcon.FadeOut(duration: 100, easing: Easing.OutCubic);
                trapIcon.FadeOut(duration: 100, easing: Easing.OutCubic);
                textArea.FadeTo(newAlpha: 1f, duration: 200, easing: Easing.OutCubic);
                Colour = Color4.White;
                icon.Colour = Color4.White;
                backgroundAddition.Colour = Color4.White;
            }
            bpChoice = newBPChoice;
            pChoice = protectChoice;
            tChoice = trapChoice;
        }

        public void Flash(int count = 1)
        {
            if (count <= 0) return;
            if (count == 1) flash.FadeOutFromOne(duration: 900, easing: Easing.OutSine);
            else flash.FadeOutFromOne(duration: 900, easing: Easing.OutSine).Loop(0, count);

            swapIcon.FadeOutFromOne(1000, Easing.InCubic);
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
