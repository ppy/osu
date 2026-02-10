// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Team.Sections.Members;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Team.Sections
{
    public partial class MembersSection : ProfileSection
    {
        public readonly Bindable<TeamProfileData?> TeamData = new Bindable<TeamProfileData?>();

        private readonly LeaderGroup leaderGroup;

        public override LocalisableString Title => TeamsStrings.ShowSectionsMembers;

        public override string Identifier => @"members";

        public MembersSection()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    leaderGroup = new LeaderGroup(),
                    new MembersGroup { TeamData = { BindTarget = TeamData } },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            TeamData.ValueChanged += data =>
            {
                if (data.OldValue?.Team.Id == data.NewValue?.Team.Id)
                    return;

                leaderGroup.User = data.NewValue?.Team.Leader;
            };
        }
    }
}
