using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Screens.Tournament
{
    public class GroupsContainer : Container
    {
        private FlowContainer<Group> topGroups;
        private FlowContainer<Group> bottomGroups;

        private List<Group> allGroups = new List<Group>();

        public GroupsContainer(int numGroups)
        {
            char nextGroupName = 'A';

            Children = new[]
            {
                    topGroups = new FlowContainer<Group>()
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,

                        AutoSizeAxes = Axes.Both,

                        Spacing = new Vector2(7f, 0)
                    },
                    bottomGroups = new FlowContainer<Group>()
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

                allGroups.Add(g);
                nextGroupName++;

                if (i < (int)Math.Ceiling(numGroups / 2f))
                    topGroups.Add(g);
                else
                    bottomGroups.Add(g);
            }
        }

        public void AddTeam(Team team)
        {
            for (int i = 0; i < allGroups.Count; i++)
            {
                if (allGroups[i].TeamsCount == 8)
                    continue;

                allGroups[i].AddTeam(team);
                break;
            }
        }

        public bool RemoveTeam(Team team)
        {
            for (int i = 0; i < allGroups.Count; i++)
            {
                if (allGroups[i].RemoveTeam(team))
                    return true;
            }

            return false;
        }

        public void ClearTeams()
        {
            for (int i = 0; i < allGroups.Count; i++)
            {
                allGroups[i].ClearTeams();
            }
        }
    }
}
