// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Logging;

namespace osu.Game.Screens.Tournament
{
    public class Drawings : OsuScreen
    {
        public const string TEAMS_FILENAME = "drawings.txt";
        private const string results_filename = "drawings_results.txt";

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenDefault();
        internal override bool ShowOverlays => false;

        private ScrollingTeamContainer teamsContainer;
        private GroupsContainer groupsContainer;
        private SpriteText fullTeamNameText;

        private List<Team> allTeams = new List<Team>();

        private DrawingsConfigManager drawingsConfig;

        private Task writeOp;

        private Storage storage;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, Storage storage)
        {
            this.storage = storage;

            if (!storage.Exists(TEAMS_FILENAME))
            {
                Exit();
                return;
            }

            drawingsConfig = new DrawingsConfigManager(storage);

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
                new FillFlowContainer()
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Right,

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
                                new VisualiserContainer()
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,

                                    RelativeSizeAxes = Axes.X,
                                    Size = new Vector2(1, 10),

                                    Colour = new Color4(255, 204, 34, 255),

                                    Lines = 6
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
                                fullTeamNameText = new SpriteText()
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
                                new FillFlowContainer()
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,

                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 0.75f,

                                    Position = new Vector2(0, 35f),

                                    Direction = FillDirection.Down,
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
                                new FillFlowContainer()
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,

                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 0.75f,

                                    Position = new Vector2(0, -5f),

                                    Direction = FillDirection.Down,
                                    Spacing = new Vector2(0, 5f),

                                    Children = new Drawable[]
                                    {
                                        new OsuButton()
                                        {
                                            RelativeSizeAxes = Axes.X,

                                            Text = "Reset",
                                            Action = () => reset(false)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            teamsContainer.OnSelected += onTeamSelected;
            teamsContainer.OnScrollStarted += () => fullTeamNameText.FadeOut(200);

            reset(true);
        }

        private void onTeamSelected(Team team)
        {
            groupsContainer.AddTeam(team);

            fullTeamNameText.Text = team.FullName;
            fullTeamNameText.FadeIn(200);

            writeResults(groupsContainer.ToStringRepresentation());
        }

        private void writeResults(string text)
        {
            Action writeAction = () =>
            {
                try
                {
                    // Write to drawings_results
                    using (Stream stream = storage.GetStream(results_filename, FileAccess.Write, FileMode.Create))
                    using (StreamWriter sw = new StreamWriter(stream))
                    {
                        sw.Write(text);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to write results.");
                }
            };

            if (writeOp == null)
                writeOp = Task.Run(writeAction);
            else
                writeOp = writeOp.ContinueWith(t => { writeAction(); });
        }

        private void reloadTeams()
        {
            teamsContainer.ClearTeams();
            allTeams.Clear();

            List<Team> newTeams = new List<Team>();

            try
            {
                using (Stream stream = storage.GetStream(TEAMS_FILENAME, FileAccess.Read, FileMode.Open))
                using (StreamReader sr = new StreamReader(stream))
                {
                    while (sr.Peek() != -1)
                    {
                        string line = sr.ReadLine().Trim();

                        if (string.IsNullOrEmpty(line))
                            continue;

                        string[] split = line.Split(':');

                        if (split.Length < 2)
                        {
                            Logger.Log($"Invalid team definition: {line}. Expected \"flag_name : team_name : team_acronym\".");
                            continue;
                        }

                        string flagName = split[0].Trim();
                        string teamName = split[1].Trim();

                        string acronym = split.Length >= 3 ? split[2].Trim() : teamName;
                        acronym = acronym.Substring(0, Math.Min(3, acronym.Length));

                        if (groupsContainer.ContainsTeam(teamName))
                            continue;

                        Team t = new Team()
                        {
                            FlagName = flagName,
                            FullName = teamName,
                            Acronym = acronym
                        };

                        newTeams.Add(t);
                    }
                }

                allTeams = newTeams;
                teamsContainer.AddTeams(allTeams);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to read teams.");
            }
        }

        private void reset(bool loadLastResults = false)
        {
            groupsContainer.ClearTeams();

            reloadTeams();

            if (loadLastResults)
            {
                try
                {
                    // Read from drawings_results
                    using (Stream stream = storage.GetStream(results_filename, FileAccess.Read, FileMode.Open))
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        while (sr.Peek() != -1)
                        {
                            string line = sr.ReadLine().Trim();

                            if (string.IsNullOrEmpty(line))
                                continue;

                            if (line.ToUpper().StartsWith("GROUP"))
                                continue;

                            Team teamToAdd = allTeams.FirstOrDefault(t => t.FullName == line);

                            if (teamToAdd == null)
                                continue;

                            groupsContainer.AddTeam(teamToAdd);
                            teamsContainer.RemoveTeam(teamToAdd);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to read last drawings results.");
                }

            }
            else
            {
                writeResults(string.Empty);
            }
        }
    }
}
