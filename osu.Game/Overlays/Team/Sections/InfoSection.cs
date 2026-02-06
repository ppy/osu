// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Team.Sections.Info;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays.Team.Sections
{
    public partial class InfoSection : ProfileSection
    {
        public readonly Bindable<TeamProfileData?> TeamData = new Bindable<TeamProfileData?>();

        private InfoValueDisplay createdAt = null!;
        private RulesetValueDisplay defaultRuleset = null!;
        private InfoValueDisplay teamApplication = null!;
        private LeaderValueDisplay leader = null!;
        private InfoValueDisplay rank = null!;
        private InfoValueDisplay performance = null!;
        private InfoValueDisplay rankedScore = null!;
        private InfoValueDisplay playCount = null!;
        private InfoValueDisplay members = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        public override LocalisableString Title => TeamsStrings.ShowSectionsInfo;

        public override string Identifier => @"info";

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Children = new[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ColumnDimensions = new[]
                    {
                        new Dimension(maxSize: 280),
                        new Dimension(GridSizeMode.Absolute, size: 2),
                        new Dimension(),
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 10),
                                Margin = new MarginPadding { Right = 20 },
                                Children = new Drawable[]
                                {
                                    createdAt = new InfoValueDisplay(size: InfoValueSize.Small) { Title = TeamsStrings.ShowInfoCreated },
                                    defaultRuleset = new RulesetValueDisplay { Title = ModelValidationStrings.TeamAttributesDefaultRulesetId },
                                    teamApplication = new InfoValueDisplay(size: InfoValueSize.Small) { Title = ModelValidationStrings.TeamAttributesIsOpen },
                                    leader = new LeaderValueDisplay { Title = TeamsStrings.ShowMembersOwner },
                                },
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background6,
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 10),
                                Margin = new MarginPadding { Left = 20 },
                                Children = new[]
                                {
                                    rank = new InfoValueDisplay(size: InfoValueSize.Large) { Title = TeamsStrings.ShowStatisticsRank },
                                    performance = new InfoValueDisplay(size: InfoValueSize.Small) { Title = RankingsStrings.StatPerformance },
                                    rankedScore = new InfoValueDisplay(size: InfoValueSize.Small) { Title = RankingsStrings.StatRankedScore },
                                    playCount = new InfoValueDisplay(size: InfoValueSize.Small) { Title = RankingsStrings.StatPlayCount },
                                    members = new InfoValueDisplay(size: InfoValueSize.Small) { Title = RankingsStrings.StatMembers },
                                },
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            TeamData.BindValueChanged(onTeamChanged, true);
        }

        private void onTeamChanged(ValueChangedEvent<TeamProfileData?> teamData)
        {
            createdAt.Content.Text = (TeamData.Value?.Team.CreatedAt.Date ?? new DateTime(2025, 2, 20)) // teams release date as per lazer updates video
                .ToLocalisableString(@"MMMM yyyy");
            defaultRuleset.Ruleset = rulesets.GetRuleset(TeamData.Value?.Team.DefaultRulesetId ?? 0);
            // TOOD: add empty slot count once pluralization is merged framework-side
            teamApplication.Content.Text = (TeamData.Value?.Team.IsOpen ?? false)
                ? TeamsStrings.EditSettingsApplicationStateState1
                : TeamsStrings.EditSettingsApplicationStateState0;
            leader.User = TeamData.Value?.Team.Leader;

            rank.Content.Text = $"#{TeamData.Value?.Team.Statistics.Rank ?? 0}";
            performance.Content.Text = (TeamData.Value?.Team.Statistics.Performance ?? 0).ToLocalisableString(@"N0");
            rankedScore.Content.Text = (TeamData.Value?.Team.Statistics.RankedScore ?? 0).ToLocalisableString(@"N0");
            playCount.Content.Text = (TeamData.Value?.Team.Statistics.PlayCount ?? 0).ToLocalisableString(@"N0");
            members.Content.Text = (TeamData.Value?.Team.MembersCount ?? 0).ToLocalisableString(@"N0");
        }
    }
}
