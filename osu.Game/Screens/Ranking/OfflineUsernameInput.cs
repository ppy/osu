// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public partial class OfflineUsernameInput : CompositeDrawable
    {
        public readonly Bindable<ScoreInfo?> Score = new Bindable<ScoreInfo?>();
        public ScoreInfo? AchievedScore { get; init; }
        public readonly Bindable<string> Username = new Bindable<string>(lastOfflineUsername);
        private OsuTextBox? usernameTextBox;
        private Container? container;

        private static string lastOfflineUsername = "Guest";

        [Resolved]
        private IAPIProvider? api { get; set; }

        [Resolved]
        private RealmAccess? realm { get; set; }

        public OfflineUsernameInput() => AutoSizeAxes = Axes.Both;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            updateVisibility();

            InternalChild = container = new Container
            {
                AutoSizeAxes = Axes.Both,
                Children =
                [
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Gray1.Opacity(0.8f),
                        Alpha = 0.8f
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0),
                        Padding = new MarginPadding(15),
                        Children =
                        [
                            new OsuSpriteText
                            {
                                Text = "Offline Username:",
                                Font = OsuFont.GetFont(size: 16, weight: FontWeight.Medium),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                            usernameTextBox = new OsuTextBox
                            {
                                Width = 200,
                                Height = 30,
                                Text = lastOfflineUsername,
                                PlaceholderText = "Enter username",
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            }
                        ]
                    }
                ]
            };

            container.CornerRadius = 8;
            container.Masking = true;

            usernameTextBox.OnCommit += (sender, newText) =>
            {
                string text = string.IsNullOrWhiteSpace(usernameTextBox.Text) ? "Guest" : usernameTextBox.Text;

                if (usernameTextBox.Text != text)
                    usernameTextBox.Text = text;

                Username.Value = text;
                updateScoreUsername(text);
                lastOfflineUsername = text;
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            api?.State.BindValueChanged(_ => updateVisibility());

            updateVisibility();
        }

        private void updateVisibility()
        {
            bool shouldBeVisible = AchievedScore != null &&
                                   api != null &&
                                   api.State.Value == APIState.Offline &&
                                   AchievedScore.UserID == APIUser.SYSTEM_USER_ID;
            Alpha = shouldBeVisible ? 1 : 0;
        }

        private void commitIfPending()
        {
            if (api == null || realm == null || AchievedScore == null) return;

            if (api.State.Value == APIState.Offline && AchievedScore.UserID == APIUser.SYSTEM_USER_ID)
            {
                string text = string.IsNullOrWhiteSpace(usernameTextBox?.Text) ? "Guest" : usernameTextBox.Text;

                if (text != "Guest")
                {
                    Username.Value = text;
                    updateScoreUsername(text);
                    lastOfflineUsername = text;
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
                commitIfPending();
            base.Dispose(isDisposing);
        }

        private void updateScoreUsername(string newUsername)
        {
            if (realm == null || AchievedScore == null) return;

            if (string.IsNullOrWhiteSpace(newUsername))
                newUsername = "Guest";

            realm.Write(r =>
            {
                ScoreInfo? databaseScore = r.Find<ScoreInfo>(AchievedScore.ID);

                if (databaseScore != null)
                {
                    databaseScore.User.Username = newUsername;
                    databaseScore.RealmUser.Username = newUsername;
                }
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            container.FadeColour(Colour4.White, 200);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (usernameTextBox?.HasFocus != true)
                container.FadeColour(Colour4.White.Opacity(0.5f), 200);
            base.OnHoverLost(e);
        }
    }
}
