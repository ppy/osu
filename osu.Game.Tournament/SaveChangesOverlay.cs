// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Tournament
{
    internal partial class SaveChangesOverlay : CompositeDrawable
    {
        [Resolved]
        private TournamentGame tournamentGame { get; set; } = null!;

        private string? lastSerialisedLadder;
        private readonly TourneyButton saveChangesButton;

        public SaveChangesOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new Container
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Position = new Vector2(5),
                CornerRadius = 10,
                Masking = true,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = OsuColour.Gray(0.2f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    saveChangesButton = new TourneyButton
                    {
                        Text = "Save Changes",
                        Width = 140,
                        Height = 50,
                        Padding = new MarginPadding
                        {
                            Top = 10,
                            Left = 10,
                        },
                        Margin = new MarginPadding
                        {
                            Right = 10,
                            Bottom = 10,
                        },
                        Action = saveChanges,
                        // Enabled = { Value = false },
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            scheduleNextCheck();
        }

        private async Task checkForChanges()
        {
            string serialisedLadder = await Task.Run(() => tournamentGame.GetSerialisedLadder()).ConfigureAwait(true);

            // If a save hasn't been triggered by the user yet, populate the initial value
            lastSerialisedLadder ??= serialisedLadder;

            if (lastSerialisedLadder != serialisedLadder && !saveChangesButton.Enabled.Value)
            {
                saveChangesButton.Enabled.Value = true;
                saveChangesButton.Background
                                 .FadeColour(saveChangesButton.BackgroundColour.Lighten(0.5f), 500, Easing.In).Then()
                                 .FadeColour(saveChangesButton.BackgroundColour, 500, Easing.Out)
                                 .Loop();
            }

            scheduleNextCheck();
        }

        private void scheduleNextCheck() => Scheduler.AddDelayed(() => checkForChanges().FireAndForget(), 1000);

        private void saveChanges()
        {
            tournamentGame.SaveChanges();
            lastSerialisedLadder = tournamentGame.GetSerialisedLadder();

            saveChangesButton.Enabled.Value = false;
            saveChangesButton.Background.FadeColour(saveChangesButton.BackgroundColour, 500);
        }
    }
}
