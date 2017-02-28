using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Tournament.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Input;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Screens.Tournament
{
    public class Drawings : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenDefault();
        internal override bool ShowOverlays => false;

        private ScrollingTeamContainer teamsContainer;
        private GroupsContainer groupsContainer;

        private DrawingsConfigManager drawingsConfig;

        public Drawings()
        {
        }

        [BackgroundDependencyLoader]
        private void load(Framework.Game game, TextureStore textures)
        {
            drawingsConfig = new DrawingsConfigManager(Game.Host.Storage);

            Container visualiserContainer;
            SpriteText st;

            Children = new Drawable[]
            {
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(77, 77, 77, 255)
                },
                new Sprite()
                {
                    FillMode = FillMode.Fill,
                    Texture = textures.Get(@"Backgrounds/Drawings/background.png")
                },
                new FlowContainer()
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FlowDirections.Horizontal,

                    Children = new Drawable[]
                    {
                        // Main container
                        new Container()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.85f,

                            Children = new Drawable[]
                            {
                                // Visualiser
                                visualiserContainer = new Container()
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,

                                    RelativeSizeAxes = Axes.X,
                                    Size = new Vector2(1, 10),

                                    Colour = new Color4(255, 204, 34, 255)
                                },
                                // Groups
                                groupsContainer = new GroupsContainer(drawingsConfig.Get<int>(DrawingsConfig.Groups), drawingsConfig.Get<int>(DrawingsConfig.TeamsPerGroup))
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,

                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,

                                    Padding = new MarginPadding()
                                    {
                                        Top = 35f,
                                        Bottom = 35f
                                    }
                                },
                                // Scrolling teams
                                teamsContainer = new ScrollingTeamContainer()
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,

                                    RelativeSizeAxes = Axes.X,
                                },
                                // Scrolling team name
                                st = new SpriteText()
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.TopCentre,

                                    Position = new Vector2(0, 45f),

                                    Alpha = 0,

                                    Font = "Exo2.0-Light",
                                    TextSize = 42f
                                }
                            }
                        },
                        // Control panel container
                        new Container()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.15f,

                            Children = new Drawable[]
                            {
                                new Box()
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = new Color4(54, 54, 54, 255)
                                },
                                new SpriteText()
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,

                                    Text = "Control Panel",
                                    TextSize = 22f,
                                    Font = "Exo2.0-Boldd"
                                },
                                new FlowContainer()
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,

                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 0.75f,

                                    Position = new Vector2(0, 35f),

                                    Direction = FlowDirections.Vertical,
                                    Spacing = new Vector2(0, 5f),

                                    Children = new Drawable[]
                                    {
                                        new OsuButton()
                                        {
                                            RelativeSizeAxes = Axes.X,

                                            Text = "Begin random",
                                            Action = teamsContainer.StartScrolling,
                                        },
                                        new OsuButton()
                                        {
                                            RelativeSizeAxes = Axes.X,

                                            Text = "Stop random",
                                            Action = teamsContainer.StopScrolling,
                                        },
                                        new OsuButton()
                                        {
                                            RelativeSizeAxes = Axes.X,

                                            Text = "Reload",
                                            Action = reloadTeams
                                        }
                                    }
                                },
                                new FlowContainer()
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,

                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 0.75f,

                                    Position = new Vector2(0, -5f),

                                    Direction = FlowDirections.Vertical,
                                    Spacing = new Vector2(0, 5f),

                                    Children = new Drawable[]
                                    {
                                        new OsuButton()
                                        {
                                            RelativeSizeAxes = Axes.X,

                                            Text = "Reset",
                                            Action = reset
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            float offset = 0;
            for (int i = 0; i < 6; i++)
            {
                visualiserContainer.Add(new VisualiserLine(2 * (float)Math.PI, offset, RNG.Next(10000, 12000))
                {
                    RelativeSizeAxes = Axes.Both,
                });

                offset += (float)Math.PI / 6f;
            }

            teamsContainer.OnSelected += t =>
            {
                groupsContainer.AddTeam(t.Team);

                st.Text = t.Team.FullName;
                st.FadeIn(200);
            };

            teamsContainer.OnScrollStarted += () => st.FadeOut(200);

            reloadTeams();
        }

        private void reloadTeams()
        {
            teamsContainer.ClearTeams();

            try
            {
                foreach (string s in File.ReadAllLines("drawings.txt"))
                {
                    string[] split = s.Split(':');
                    string flagName = split[0].Trim();
                    string name = split[1].Trim();

                    string acronym = name.Substring(0, Math.Min(3, name.Length));
                    if (split.Length >= 3)
                        acronym = split[2].Trim();

                    if (groupsContainer.ContainsTeam(name))
                        continue;

                    teamsContainer.AddTeam(new Team()
                    {
                        FlagName = flagName,
                        FullName = name,
                        Acronym = acronym
                    });
                }
            }
            catch { }
        }

        private void reset()
        {
            groupsContainer.ClearTeams();
            teamsContainer.ClearTeams();

            reloadTeams();
        }
    }
}
