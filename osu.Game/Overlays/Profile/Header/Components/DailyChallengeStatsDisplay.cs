// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class DailyChallengeStatsDisplay : CompositeDrawable, IHasCustomTooltip<DailyChallengeTooltipData>
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        public DailyChallengeTooltipData? TooltipContent { get; private set; }

        private OsuSpriteText dailyPlayCount = null!;
        private Container content = null!;
        private CircularContainer completionMark = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            OsuTextFlowContainer label;

            InternalChildren = new Drawable[]
            {
                content = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 6,
                    BorderThickness = 2,
                    BorderColour = colourProvider.Background4,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background4,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Padding = new MarginPadding(3f),
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                label = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 12))
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Horizontal = 5f, Bottom = 2f },
                                },
                                new Container
                                {
                                    AutoSizeAxes = Axes.X,
                                    RelativeSizeAxes = Axes.Y,
                                    CornerRadius = 3,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colourProvider.Background6,
                                        },
                                        dailyPlayCount = new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            UseFullGlyphHeight = false,
                                            Colour = colourProvider.Content2,
                                            Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                        },
                                    }
                                },
                            }
                        },
                    }
                },
                completionMark = new CircularContainer
                {
                    Alpha = 0,
                    Size = new Vector2(16),
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.Centre,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Lime1,
                        },
                        new SpriteIcon
                        {
                            Size = new Vector2(8),
                            Colour = colourProvider.Background6,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Icon = FontAwesome.Solid.Check,
                        }
                    }
                },
            };

            // can't use this because osu-web does weird stuff with \\n.
            // Text = UsersStrings.ShowDailyChallengeTitle.,
            label.AddParagraph("Daily\nChallenge");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            User.BindValueChanged(_ => updateDisplay(), true);
        }

        private void updateDisplay()
        {
            if (User.Value == null)
            {
                Hide();
                return;
            }

            APIUserDailyChallengeStatistics stats = User.Value.User.DailyChallengeStatistics;

            if (stats.PlayCount == 0)
            {
                Hide();
                return;
            }

            dailyPlayCount.Text = DailyChallengeStatsDisplayStrings.UnitDay(stats.PlayCount.ToLocalisableString("N0"));
            dailyPlayCount.Colour = OsuColour.ForRankingTier(DailyChallengeStatsTooltip.TierForPlayCount(stats.PlayCount));

            bool playedToday = stats.LastUpdate?.Date == DateTimeOffset.UtcNow.Date;
            bool userIsOnOwnProfile = stats.UserID == api.LocalUser.Value.Id;

            if (playedToday && userIsOnOwnProfile)
            {
                if (completionMark.Alpha > 0.8f)
                {
                    completionMark.ScaleTo(1.2f).ScaleTo(1, 800, Easing.OutElastic);
                }
                else
                {
                    completionMark.FadeIn(500, Easing.OutExpo);
                    completionMark.ScaleTo(1.6f).ScaleTo(1, 500, Easing.OutExpo);
                }

                content.BorderColour = colours.Lime1;
            }
            else
            {
                completionMark.FadeOut(50);
                content.BorderColour = colourProvider.Background4;
            }

            TooltipContent = new DailyChallengeTooltipData(colourProvider, stats);

            Show();
        }

        public ITooltip<DailyChallengeTooltipData> GetCustomTooltip() => new DailyChallengeStatsTooltip();
    }
}
