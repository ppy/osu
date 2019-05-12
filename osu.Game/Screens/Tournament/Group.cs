// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osu.Game.Screens.Tournament.Teams;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Screens.Tournament
{
    public class Group : Container
    {
        public readonly string GroupName;

        public int TeamsCount { get; private set; }

        private readonly FlowContainer<GroupTeam> teams;

        private readonly List<GroupTeam> allTeams = new List<GroupTeam>();

        public Group(string name)
        {
            GroupName = name;

            Size = new Vector2(176, 128);

            Masking = true;
            CornerRadius = 4;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(54, 54, 54, 255)
                },
                // Group name
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,

                    Position = new Vector2(0, 7f),

                    Text = $"GROUP {name.ToUpperInvariant()}",
                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 8),
                    Colour = new Color4(255, 204, 34, 255),
                },
                teams = new FillFlowContainer<GroupTeam>
                {
                    RelativeSizeAxes = Axes.Both,

                    Spacing = new Vector2(6f, 22),

                    Margin = new MarginPadding
                    {
                        Top = 21f,
                        Bottom = 7f,
                        Left = 7f,
                        Right = 7f
                    }
                }
            };
        }

        public void AddTeam(DrawingsTeam team)
        {
            GroupTeam gt = new GroupTeam(team);

            if (TeamsCount < 8)
            {
                teams.Add(gt);
                allTeams.Add(gt);

                TeamsCount++;
            }
        }

        public bool ContainsTeam(string fullName)
        {
            return allTeams.Any(t => t.Team.FullName == fullName);
        }

        public bool RemoveTeam(DrawingsTeam team)
        {
            allTeams.RemoveAll(gt => gt.Team == team);

            if (teams.RemoveAll(gt => gt.Team == team) > 0)
            {
                TeamsCount--;
                return true;
            }

            return false;
        }

        public void ClearTeams()
        {
            allTeams.Clear();
            teams.Clear();

            TeamsCount = 0;
        }

        public string GetStringRepresentation()
        {
            StringBuilder sb = new StringBuilder();
            foreach (GroupTeam gt in allTeams)
                sb.AppendLine(gt.Team.FullName);
            return sb.ToString();
        }

        private class GroupTeam : Container
        {
            public readonly DrawingsTeam Team;

            private readonly FillFlowContainer innerContainer;
            private readonly Sprite flagSprite;

            public GroupTeam(DrawingsTeam team)
            {
                Team = team;

                Width = 36;
                AutoSizeAxes = Axes.Y;

                Children = new Drawable[]
                {
                    innerContainer = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,

                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,

                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5f),

                        Children = new Drawable[]
                        {
                            flagSprite = new Sprite
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                FillMode = FillMode.Fit
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,

                                Text = team.Acronym.ToUpperInvariant(),
                                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 10),
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                innerContainer.ScaleTo(1.5f);
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
