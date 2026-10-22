// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.RankingV2.Extended
{
    public partial class PlayerInfoPanel : CompositeDrawable
    {
        private CoverBackground userCover = null!;
        private OsuSpriteText positionText = null!;
        private UpdateableAvatar avatar = null!;
        private OsuSpriteText usernameSpriteText = null!;
        private UpdateableFlag userFlag = null!;
        private OsuSpriteText dateSpriteText = null!;

        private CancellationTokenSource? userLookupCancellation;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 20;

            InternalChildren =
            [
                userCover = new CoverBackground
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                    Alpha = 0.75f,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ColumnDimensions =
                    [
                        new Dimension(GridSizeMode.Absolute, 200),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    ],
                    RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            positionText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Font = OsuFont.Style.Subtitle,
                                Margin = new MarginPadding { Right = 10, },
                            },
                            avatar = new UpdateableAvatar
                            {
                                Size = new Vector2(50),
                                Masking = true,
                                CornerRadius = 5,
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(5),
                                Padding = new MarginPadding { Horizontal = 10, },
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Children =
                                [
                                    usernameSpriteText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Font = OsuFont.Style.Heading2,
                                    },
                                    userFlag = new UpdateableFlag
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(19, 14),
                                    },
                                ],
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Padding = new MarginPadding { Right = 30, },
                                Children =
                                [
                                    new OsuSpriteText
                                    {
                                        Font = OsuFont.Style.Caption1,
                                        Colour = colourProvider.Content2,
                                        Text = ResultsScreenStrings.AchievedOnDate(null),
                                    },
                                    dateSpriteText = new OsuSpriteText
                                    {
                                        Font = OsuFont.Style.Body,
                                    }
                                ]
                            }
                        },
                    },
                },
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            int? position = (score.Value as IScoreWithPosition)?.Position;
            var user = score.Value.User;

            positionText.Text = position != null ? LocalisableString.Interpolate($"#{position:N0}") : default;
            usernameSpriteText.Text = user.Username;
            userFlag.CountryCode = user.CountryCode;
            dateSpriteText.Text = score.Value.Date.ToString(@"g");

            userLookupCancellation?.Cancel();

            if (userHasUsableAssets(score.Value.User, out var apiUser))
                setAPIUser(apiUser);
            else
            {
                userLookupCancellation = new CancellationTokenSource();

                setAPIUser(null);
                userLookupCache.GetUserAsync(score.Value.User.OnlineID, userLookupCancellation.Token)
                               .ContinueWith(t => Schedule(() => setAPIUser(t.IsCompletedSuccessfully ? t.GetResultSafely() : null)));
            }
        }

        private bool userHasUsableAssets(IUser user, [NotNullWhen(true)] out APIUser? result)
        {
            var apiUser = user as APIUser;
            bool assetsPresent = !string.IsNullOrEmpty(apiUser?.AvatarUrl) && !string.IsNullOrEmpty(apiUser.CoverUrl);
            result = assetsPresent ? apiUser : null;
            return assetsPresent;
        }

        private void setAPIUser(APIUser? apiUser)
        {
            userCover.Model = apiUser;
            avatar.User = apiUser;
        }
    }
}
