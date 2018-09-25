// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class DrawableMatchTeam : DrawableTournamentTeam, IHasContextMenu
    {
        private readonly MatchPairing pairing;
        private readonly bool losers;
        private OsuSpriteText scoreText;
        private Box background;

        private readonly Bindable<int?> score = new Bindable<int?>();
        private readonly BindableBool completed = new BindableBool();

        private Color4 colourWinner;
        private Color4 colourNormal;

        private readonly Func<bool> isWinner;
        private LadderManager manager;

        [Resolved(CanBeNull = true)]
        private LadderEditorInfo editorInfo { get; set; } = null;

        public DrawableMatchTeam(Bindable<TournamentTeam> team, MatchPairing pairing, bool losers)
            : base(team)
        {
            this.pairing = pairing;
            this.losers = losers;
            Size = new Vector2(150, 40);

            Masking = true;
            CornerRadius = 5;

            Flag.Scale = new Vector2(0.9f);
            Flag.Anchor = Flag.Origin = Anchor.CentreLeft;

            AcronymText.Anchor = AcronymText.Origin = Anchor.CentreLeft;
            AcronymText.Padding = new MarginPadding { Left = 50 };
            AcronymText.TextSize = 24;

            if (pairing != null)
            {
                isWinner = () => pairing.Winner == Team;

                completed.BindTo(pairing.Completed);
                if (team.Value != null)
                    score.BindTo(team.Value == pairing.Team1.Value ? pairing.Team1Score : pairing.Team2Score);
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, LadderManager manager)
        {
            this.manager = manager;

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
                                    TextSize = 20,
                                }
                            }
                        }
                    }
                }
            };

            completed.BindValueChanged(_ => updateWinStyle());

            score.BindValueChanged(val =>
            {
                scoreText.Text = val?.ToString() ?? string.Empty;
                updateWinStyle();
            }, true);
        }

        //TODO: use OnClick instead once we have per-button clicks.
        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (Team == null || editorInfo.EditingEnabled) return false;

            if (args.Button == MouseButton.Left)
            {
                if (score.Value == null)
                {
                    pairing.StartMatch();
                }
                else if (!pairing.Completed)
                    score.Value++;
            }
            else
            {
                if (pairing.Progression.Value?.Completed.Value != false)
                    // don't allow changing scores if the match has a progression. can cause large data loss
                    return false;

                if (pairing.Completed && pairing.Winner != Team)
                    // don't allow changing scores from the non-winner
                    return false;

                if (score.Value > 0)
                    score.Value--;
                else
                    pairing.CancelMatchStart();
            }

            return false;
        }

        private void updateWinStyle()
        {
            bool winner = completed && isWinner?.Invoke() == true;

            background.FadeColour(winner ? colourWinner : colourNormal, winner ? 500 : 0, Easing.OutQuint);

            scoreText.Font = AcronymText.Font = winner ? "Exo2.0-Bold" : "Exo2.0-Regular";
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (!editorInfo.EditingEnabled)
                    return new MenuItem[0];

                return new MenuItem[]
                {
                    new OsuMenuItem("Join with", MenuItemType.Standard, () => manager.RequestJoin(pairing, false)),
                    new OsuMenuItem("Join with (loser)", MenuItemType.Standard, () => manager.RequestJoin(pairing, true)),
                    new OsuMenuItem("Remove", MenuItemType.Destructive, () => manager.Remove(pairing)),
                };
            }
        }
    }
}
