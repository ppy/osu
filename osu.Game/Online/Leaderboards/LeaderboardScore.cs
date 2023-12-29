// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Select;
using osu.Game.Scoring;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;
using osu.Game.Online.API;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderboardScore : OsuClickableContainer, IHasContextMenu, IHasCustomTooltip<ScoreInfo>
    {
        public const float HEIGHT = 60;

        public readonly ScoreInfo Score;

        private const float corner_radius = 5;
        private const float edge_margin = 5;
        private const float background_alpha = 0.25f;
        private const float rank_width = 35;

        protected Container RankContainer { get; private set; }

        private readonly int? rank;
        private readonly bool isOnlineScope;

        private Box background;
        private Container content;
        private Drawable avatar;
        private Drawable scoreRank;
        private OsuSpriteText nameLabel;

        public GlowingSpriteText ScoreText { get; private set; }

        private FillFlowContainer flagBadgeAndDateContainer;
        private FillFlowContainer<ModIcon> modsContainer;

        private List<ScoreComponentLabel> statisticsLabels;

        [Resolved(canBeNull: true)]
        private IDialogOverlay dialogOverlay { get; set; }

        [Resolved(canBeNull: true)]
        private SongSelect songSelect { get; set; }

        public ITooltip<ScoreInfo> GetCustomTooltip() => new LeaderboardScoreTooltip();
        public virtual ScoreInfo TooltipContent => Score;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        public LeaderboardScore(ScoreInfo score, int? rank, bool isOnlineScope = true)
        {
            Score = score;

            this.rank = rank;
            this.isOnlineScope = isOnlineScope;

            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, OsuColour colour)
        {
            var user = Score.User;

            statisticsLabels = GetStatistics(Score).Select(s => new ScoreComponentLabel(s)).ToList();

            ClickableAvatar innerAvatar;

            Children = new Drawable[]
            {
                new RankLabel(rank)
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = rank_width,
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = rank_width },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = corner_radius,
                            Masking = true,
                            Children = new[]
                            {
                                background = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = user.OnlineID == api.LocalUser.Value.Id && isOnlineScope ? colour.Green : Color4.Black,
                                    Alpha = background_alpha,
                                },
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(edge_margin),
                            Children = new[]
                            {
                                avatar = new DelayedLoadWrapper(
                                    innerAvatar = new ClickableAvatar(user)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        CornerRadius = corner_radius,
                                        Masking = true,
                                        EdgeEffect = new EdgeEffectParameters
                                        {
                                            Type = EdgeEffectType.Shadow,
                                            Radius = 1,
                                            Colour = Color4.Black.Opacity(0.2f),
                                        },
                                    })
                                {
                                    RelativeSizeAxes = Axes.None,
                                    Size = new Vector2(HEIGHT - edge_margin * 2, HEIGHT - edge_margin * 2),
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Position = new Vector2(HEIGHT - edge_margin, 0f),
                                    Children = new Drawable[]
                                    {
                                        nameLabel = new OsuSpriteText
                                        {
                                            Text = user.Username,
                                            Font = OsuFont.GetFont(size: 23, weight: FontWeight.Bold, italics: true)
                                        },
                                        new FillFlowContainer
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(10f, 0f),
                                            Children = new Drawable[]
                                            {
                                                flagBadgeAndDateContainer = new FillFlowContainer
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    RelativeSizeAxes = Axes.Y,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(5f, 0f),
                                                    Width = 87f,
                                                    Masking = true,
                                                    Children = new Drawable[]
                                                    {
                                                        new UpdateableFlag(user.CountryCode)
                                                        {
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Size = new Vector2(28, 20),
                                                        },
                                                        new DateLabel(Score.Date)
                                                        {
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                        },
                                                    },
                                                },
                                                new FillFlowContainer
                                                {
                                                    Origin = Anchor.CentreLeft,
                                                    Anchor = Anchor.CentreLeft,
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Margin = new MarginPadding { Left = edge_margin },
                                                    Children = statisticsLabels
                                                },
                                            },
                                        },
                                    },
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5f, 0f),
                                    Children = new Drawable[]
                                    {
                                        ScoreText = new GlowingSpriteText
                                        {
                                            TextColour = Color4.White,
                                            GlowColour = Color4Extensions.FromHex(@"83ccfa"),
                                            Current = scoreManager.GetBindableTotalScoreString(Score),
                                            Font = OsuFont.Numeric.With(size: 23),
                                        },
                                        RankContainer = new Container
                                        {
                                            Size = new Vector2(40f, 20f),
                                            Children = new[]
                                            {
                                                scoreRank = new UpdateableRank(Score.Rank)
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Size = new Vector2(40f)
                                                },
                                            },
                                        },
                                    },
                                },
                                modsContainer = new FillFlowContainer<ModIcon>
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    ChildrenEnumerable = Score.Mods.AsOrdered().Select(mod => new ModIcon(mod) { Scale = new Vector2(0.375f) })
                                },
                            },
                        },
                    },
                },
            };

            innerAvatar.OnLoadComplete += d => d.FadeInFromZero(200);
        }

        public override void Show()
        {
            foreach (var d in new[] { avatar, nameLabel, ScoreText, scoreRank, flagBadgeAndDateContainer, modsContainer }.Concat(statisticsLabels))
                d.FadeOut();

            Alpha = 0;

            content.MoveToY(75);
            avatar.MoveToX(75);
            nameLabel.MoveToX(150);

            this.FadeIn(200);
            content.MoveToY(0, 800, Easing.OutQuint);

            using (BeginDelayedSequence(100))
            {
                avatar.FadeIn(300, Easing.OutQuint);
                nameLabel.FadeIn(350, Easing.OutQuint);

                avatar.MoveToX(0, 300, Easing.OutQuint);
                nameLabel.MoveToX(0, 350, Easing.OutQuint);

                using (BeginDelayedSequence(250))
                {
                    ScoreText.FadeIn(200);
                    scoreRank.FadeIn(200);

                    using (BeginDelayedSequence(50))
                    {
                        var drawables = new Drawable[] { flagBadgeAndDateContainer, modsContainer }.Concat(statisticsLabels).ToArray();
                        for (int i = 0; i < drawables.Length; i++)
                            drawables[i].FadeIn(100 + i * 50);
                    }
                }
            }
        }

        protected virtual IEnumerable<LeaderboardScoreStatistic> GetStatistics(ScoreInfo model) => new[]
        {
            new LeaderboardScoreStatistic(FontAwesome.Solid.Link, BeatmapsetsStrings.ShowScoreboardHeadersCombo, $"{model.MaxCombo}/{model.GetMaximumAchievableCombo()}"),
            new LeaderboardScoreStatistic(FontAwesome.Solid.Crosshairs, BeatmapsetsStrings.ShowScoreboardHeadersAccuracy, model.DisplayAccuracy)
        };

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeTo(0.5f, 300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeTo(background_alpha, 200, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        private partial class ScoreComponentLabel : Container, IHasTooltip
        {
            private const float icon_size = 20;
            private readonly FillFlowContainer content;

            public override bool Contains(Vector2 screenSpacePos) => content.Contains(screenSpacePos);

            public LocalisableString TooltipText { get; }

            public ScoreComponentLabel(LeaderboardScoreStatistic statistic)
            {
                TooltipText = statistic.Name;
                AutoSizeAxes = Axes.Both;

                Child = content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Padding = new MarginPadding { Right = 10 },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(icon_size),
                                    Rotation = 45,
                                    Colour = Color4Extensions.FromHex(@"3087ac"),
                                    Icon = FontAwesome.Solid.Square,
                                    Shadow = true,
                                },
                                new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(icon_size - 6),
                                    Colour = Color4Extensions.FromHex(@"a4edff"),
                                    Icon = statistic.Icon,
                                },
                            },
                        },
                        new GlowingSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            TextColour = Color4.White,
                            GlowColour = Color4Extensions.FromHex(@"83ccfa"),
                            Text = statistic.Value,
                            Font = OsuFont.GetFont(size: 17, weight: FontWeight.Bold),
                        },
                    },
                };
            }
        }

        private partial class RankLabel : Container, IHasTooltip
        {
            public RankLabel(int? rank)
            {
                if (rank >= 1000)
                    TooltipText = $"#{rank:N0}";

                Child = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(size: 20, italics: true),
                    Text = rank == null ? "-" : rank.Value.FormatRank()
                };
            }

            public LocalisableString TooltipText { get; }
        }

        private partial class DateLabel : DrawableDate
        {
            public DateLabel(DateTimeOffset date)
                : base(date)
            {
                Font = OsuFont.GetFont(size: 17, weight: FontWeight.Bold, italics: true);
            }

            protected override string Format() => Date.ToShortRelativeTime(TimeSpan.FromSeconds(30));
        }

        public class LeaderboardScoreStatistic
        {
            public IconUsage Icon;
            public LocalisableString Value;
            public LocalisableString Name;

            public LeaderboardScoreStatistic(IconUsage icon, LocalisableString name, LocalisableString value)
            {
                Icon = icon;
                Name = name;
                Value = value;
            }
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                if (Score.Mods.Length > 0 && songSelect != null)
                    items.Add(new OsuMenuItem("Use these mods", MenuItemType.Highlighted, () => songSelect.Mods.Value = Score.Mods));

                if (Score.Files.Count > 0)
                {
                    items.Add(new OsuMenuItem(Localisation.CommonStrings.Export, MenuItemType.Standard, () => scoreManager.Export(Score)));
                    items.Add(new OsuMenuItem(CommonStrings.ButtonsDelete, MenuItemType.Destructive, () => dialogOverlay?.Push(new LocalScoreDeleteDialog(Score))));
                }

                return items.ToArray();
            }
        }
    }
}
