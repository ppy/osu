using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Screens.Tournament
{
    public class Group : Container
    {
        public string GroupName;

        public int TeamsCount => topTeamsCount + bottomTeamsCount;

        private FlowContainer<GroupTeam> topTeams;
        private FlowContainer<GroupTeam> bottomTeams;

        private List<GroupTeam> allTeams = new List<GroupTeam>();

        private int topTeamsCount;
        private int bottomTeamsCount;

        public Group(string name)
        {
            GroupName = name;

            Size = new Vector2(176, 128);

            Masking = true;
            CornerRadius = 4;

            Children = new Drawable[]
            {
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(54, 54, 54, 255)
                },
                // Group name
                new SpriteText()
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,

                    Position = new Vector2(0, 7f),

                    Text = $"GROUP {name.ToUpper()}",
                    TextSize = 8f,
                    Font = @"Exo2.0-Bold",
                    Colour = new Color4(255, 204, 34, 255),
                },
                topTeams = new FlowContainer<GroupTeam>()
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,

                    Position = new Vector2(0, 21f),
                    Spacing = new Vector2(6f, 0),

                    Padding = new MarginPadding()
                    {
                        Left = 7f,
                        Right = 7f
                    },

                    Direction = FlowDirections.Horizontal
                },
                bottomTeams = new FlowContainer<GroupTeam>()
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,

                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,

                    Position = new Vector2(0, -7f),
                    Spacing = new Vector2(6f, 0),

                    Padding = new MarginPadding()
                    {
                        Left =  7f,
                        Right = 7f
                    },

                    Direction = FlowDirections.Horizontal
                }
            };
        }

        public void AddTeam(Team team)
        {

            GroupTeam gt = new GroupTeam(team);
            if (topTeamsCount < 4)
            {
                topTeams.Add(gt);
                allTeams.Add(gt);
                topTeamsCount++;
            }
            else if (bottomTeamsCount < 4)
            {
                bottomTeams.Add(gt);
                allTeams.Add(gt);
                bottomTeamsCount++;
            }
        }

        public bool ContainsTeam(string fullName)
        {
            return allTeams.Any(t => t.Team.FullName == fullName);
        }

        public bool RemoveTeam(Team team)
        {
            allTeams.RemoveAll(gt => gt.Team == team);

            if (topTeams.RemoveAll(gt => gt.Team == team) > 0)
            {
                topTeamsCount--;
                return true;
            }
            else if (bottomTeams.RemoveAll(gt => gt.Team == team) > 0)
            {
                bottomTeamsCount--;
                return true;
            }

            return false;
        }

        public void ClearTeams()
        {
            allTeams.Clear();
            topTeams.Clear();
            bottomTeams.Clear();

            topTeamsCount = 0;
            bottomTeamsCount = 0;
        }

        class GroupTeam : Container
        {
            public Team Team;

            private FlowContainer innerContainer;
            private Sprite flagSprite;

            public GroupTeam(Team team)
            {
                Team = team;

                Size = new Vector2(36, 0);
                AutoSizeAxes = Axes.Y;

                Children = new Drawable[]
                {
                    innerContainer = new FlowContainer()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,

                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,

                        Direction = FlowDirections.Vertical,
                        Spacing = new Vector2(0, 5f),

                        Scale = new Vector2(1.5f),

                        Children = new Drawable[]
                        {
                            flagSprite = new Sprite()
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,

                                FillMode = FillMode.Fit
                            },
                            new SpriteText()
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,

                                Text = team.Acronym.ToUpper(),
                                TextSize = 10f,
                                Font = @"Exo2.0-Bold"
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                innerContainer.ScaleTo(1f, 200);
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                flagSprite.Texture = textures.Get($@"Flags/{Team.FlagName}");
            }
        }
    }
}
