// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Editors;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class DrawableMatchTeam : DrawableTournamentTeam, IHasContextMenu
    {
        private readonly TournamentMatch match;
        private readonly bool losers;
        private OsuSpriteText scoreText;
        private Box background;

        private readonly Bindable<int?> score = new Bindable<int?>();
        private readonly BindableBool completed = new BindableBool();

        private Color4 colourWinner;
        private Color4 colourNormal;

        private readonly Func<bool> isWinner;
        private LadderEditorScreen ladderEditor;

        [Resolved]
        private LadderInfo ladderInfo { get; set; }

        private void setCurrent()
        {
            //todo: tournamentgamebase?
            if (ladderInfo.CurrentMatch.Value != null)
                ladderInfo.CurrentMatch.Value.Current.Value = false;

            ladderInfo.CurrentMatch.Value = match;
            ladderInfo.CurrentMatch.Value.Current.Value = true;
        }

        [Resolved(CanBeNull = true)]
        private LadderEditorInfo editorInfo { get; set; }

        public DrawableMatchTeam(TournamentTeam team, TournamentMatch match, bool losers)
            : base(team)
        {
            this.match = match;
            this.losers = losers;
            Size = new Vector2(150, 40);

            Masking = true;
            CornerRadius = 5;

            Flag.Scale = new Vector2(0.9f);
            Flag.Anchor = Flag.Origin = Anchor.CentreLeft;

            AcronymText.Anchor = AcronymText.Origin = Anchor.CentreLeft;
            AcronymText.Padding = new MarginPadding { Left = 50 };
            AcronymText.Font = OsuFont.GetFont(size: 24);

            if (match != null)
            {
                isWinner = () => match.Winner == Team;

                completed.BindTo(match.Completed);
                if (team != null)
                    score.BindTo(team == match.Team1.Value ? match.Team1Score : match.Team2Score);
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, LadderEditorScreen ladderEditor)
        {
            this.ladderEditor = ladderEditor;

            colourWinner = losers ? colours.YellowDarker : colours.BlueDarker;
            colourNormal = OsuColour.Gray(0.2f);

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    Padding = new MarginPadding(5),
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        AcronymText,
                        Flag,
                        new Container
                        {
                            Masking = true,
                            CornerRadius = 5,
                            Width = 0.3f,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = OsuColour.Gray(0.1f),
                                    Alpha = 0.8f,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                scoreText = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 20),
                                }
                            }
                        }
                    }
                }
            };

            completed.BindValueChanged(_ => updateWinStyle());

            score.BindValueChanged(val =>
            {
                scoreText.Text = val.NewValue?.ToString() ?? string.Empty;
                updateWinStyle();
            }, true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Team == null || editorInfo != null) return false;

            if (!match.Current.Value)
            {
                setCurrent();
                return true;
            }

            if (e.Button == MouseButton.Left)
            {
                if (score.Value == null)
                {
                    match.StartMatch();
                }
                else if (!match.Completed.Value)
                    score.Value++;
            }
            else
            {
                if (match.Progression.Value?.Completed.Value == true)
                    // don't allow changing scores if the match has a progression. can cause large data loss
                    return false;

                if (match.Completed.Value && match.Winner != Team)
                    // don't allow changing scores from the non-winner
                    return false;

                if (score.Value > 0)
                    score.Value--;
                else
                    match.CancelMatchStart();
            }

            return false;
        }

        private void updateWinStyle()
        {
            bool winner = completed.Value && isWinner?.Invoke() == true;

            background.FadeColour(winner ? colourWinner : colourNormal, winner ? 500 : 0, Easing.OutQuint);

            scoreText.Font = AcronymText.Font = OsuFont.GetFont(weight: winner ? FontWeight.Bold : FontWeight.Regular);
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (editorInfo == null)
                    return new MenuItem[0];

                return new MenuItem[]
                {
                    new OsuMenuItem("Set as current", MenuItemType.Standard, setCurrent),
                    new OsuMenuItem("Join with", MenuItemType.Standard, () => ladderEditor.BeginJoin(match, false)),
                    new OsuMenuItem("Join with (loser)", MenuItemType.Standard, () => ladderEditor.BeginJoin(match, true)),
                    new OsuMenuItem("Remove", MenuItemType.Destructive, () => ladderEditor.Remove(match)),
                };
            }
        }
    }
}
