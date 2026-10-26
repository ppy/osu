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
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.RankingV2.Argon
{
    public partial class ArgonPlayerInfoPanel : CompositeDrawable, ISerialisableDrawable, IAnimatableSkinnable
    {
        private const float height = 60;
        private const float spacing = 12;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        private CancellationTokenSource? userLookupCancellation;

        private Container content = null!;
        private OsuSpriteText positionText = null!;
        private UpdateableAvatar userAvatar = null!;
        private CoverBackground userCover = null!;
        private OsuSpriteText usernameText = null!;
        private OsuSpriteText achievedOnText = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Width = ArgonBeatmapInfoPanel.WIDTH;
            Height = height;
            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Shear = OsuGame.SHEAR,
                CornerRadius = ShearedButton.CORNER_RADIUS,
                EdgeEffect = ArgonBeatmapInfoPanel.CreateShadowEdgeEffect(),
                Masking = true,
                Children =
                [
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background4,
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColumnDimensions =
                        [
                            new Dimension(GridSizeMode.AutoSize, minSize: 150),
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(),
                        ],
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children =
                                    [
                                        positionText = new OsuSpriteText
                                        {
                                            Font = OsuFont.Style.Heading2.With(size: 22),
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Shear = -OsuGame.SHEAR,
                                            Margin = new MarginPadding { Right = spacing, },
                                        },
                                    ],
                                },
                                new Container
                                {
                                    Size = new Vector2(height),
                                    Masking = true,
                                    CornerRadius = ShearedButton.CORNER_RADIUS,
                                    Child = userAvatar = new UpdateableAvatar
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Shear = -OsuGame.SHEAR,
                                        // scaled up to cover sides which have extra space due to the shearing
                                        Scale = new Vector2(1.15f),
                                    },
                                    // depth set so that this is in front of the next cell, which has the user cover
                                    Depth = float.MinValue,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children =
                                    [
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding
                                            {
                                                // negative padding so that the background can underlap the user avatar where the rounded corners are
                                                Left = -ShearedButton.CORNER_RADIUS,
                                            },
                                            Children =
                                            [
                                                userCover = new CoverBackground
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                },
                                                // instead of applying 0.25 alpha to the cover, a box with 0.75 alpha is drawn on top of it.
                                                // this is because the cover background can internally have multiple covers in it when in transition due to a model change
                                                // and that looks bad with how framework does alpha (it is applied to each layer individually, flattening is not performed).
                                                // the alternative would be to use a buffered container, but that has negative performance implications
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = colourProvider.Background4,
                                                    Alpha = 0.75f,
                                                },
                                            ],
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Shear = -OsuGame.SHEAR,
                                            Padding = new MarginPadding { Left = spacing, },
                                            Children =
                                            [
                                                usernameText = new OsuSpriteText
                                                {
                                                    Font = OsuFont.Style.Heading2.With(size: 24),
                                                },
                                                achievedOnText = new OsuSpriteText
                                                {
                                                    Font = OsuFont.Style.Body.With(size: 20),
                                                    Colour = colourProvider.Content2,
                                                },
                                            ],
                                        },
                                    ],
                                },
                            },
                        },
                    },
                ],
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            int? position = (score.Value as IScoreWithPosition)?.Position;
            positionText.Alpha = position != null ? 1 : 0;
            positionText.Text = position?.FormatRank().Insert(0, "#") ?? "";

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

            usernameText.Text = score.Value.User.Username;
            achievedOnText.Text = ResultsScreenStrings.AchievedOnDate(score.Value.Date);
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
            userAvatar.User = apiUser;
            userCover.Model = apiUser;
        }

        #region ISerialisableDrawable

        public bool UsesFixedAnchor { get; set; }

        // TODO: temporary to avoid `SkinDeserialisationTest` failures.
        // Remove when the argon skin supplies a default layout of results with this component included.
        public bool IsEditable => false;

        #endregion

        #region IAnimatableSkinnable

        [SettingSource(nameof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.AnimationSequence))]
        public BindableInt GroupNumber { get; } = new BindableInt(1)
        {
            MinValue = 0,
            MaxValue = 10,
        };

        int IAnimatableSkinnable.GroupNumber => GroupNumber.Value;

        public double StartAnimating(double startTime)
        {
            const double transition_duration = 500;

            FinishAnimating();

            content.FadeOut()
                   .MoveToOffset(new Vector2(-50, 0));

            using (BeginAbsoluteSequence(startTime))
            {
                content.FadeIn(transition_duration, Easing.OutQuint)
                       .MoveToOffset(new Vector2(50, 0), transition_duration, Easing.OutQuint);

                return content.LatestTransformEndTime;
            }
        }

        public void FinishAnimating()
        {
            content.FinishTransforms();
        }

        #endregion
    }
}
