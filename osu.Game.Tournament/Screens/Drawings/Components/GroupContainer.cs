// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Drawings.Components
{
    public partial class GroupContainer : Container
    {
        private readonly List<Group> groups = new List<Group>();

        private readonly int maxTeams;
        private int currentGroup;

        public GroupContainer(int numGroups, int teamsPerGroup)
        {
            FlowContainer<Group> bottomGroups;
            FlowContainer<Group> topGroups;

            maxTeams = teamsPerGroup;

            char nextGroupName = 'A';

            Children = new[]
            {
                topGroups = new FillFlowContainer<Group>
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,

                    AutoSizeAxes = Axes.Both,

                    Spacing = new Vector2(7f, 0)
                },
                bottomGroups = new FillFlowContainer<Group>
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,

                    AutoSizeAxes = Axes.Both,

                    Spacing = new Vector2(7f, 0)
                }
            };

            for (int i = 0; i < numGroups; i++)
            {
                Group g = new Group(nextGroupName.ToString());

                groups.Add(g);
                nextGroupName++;

                if (i < (int)MathF.Ceiling(numGroups / 2f))
                    topGroups.Add(g);
                else
                    bottomGroups.Add(g);
            }
        }

        public void AddTeam(TournamentTeam team)
        {
            if (groups[currentGroup].TeamsCount == maxTeams)
                return;

            groups[currentGroup].AddTeam(team);

            currentGroup = (currentGroup + 1) % groups.Count;
        }

        public bool ContainsTeam(string fullName)
        {
            return groups.Any(g => g.ContainsTeam(fullName));
        }

        public void ClearTeams()
        {
            foreach (Group g in groups)
                g.ClearTeams();

            currentGroup = 0;
        }

        public string GetStringRepresentation()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Group g in groups)
            {
                if (g != groups.First())
                    sb.AppendLine();
                sb.AppendLine($"Group {g.GroupName}");
                sb.Append(g.GetStringRepresentation());
            }

            return sb.ToString();
        }
    }
}
