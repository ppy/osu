// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Drawings.Components;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Tournament.Screens.Drawings
{
    public class DrawingsScreen : OsuScreen
    {
        private const string results_filename = "drawings_results.txt";

        protected override bool HideOverlaysOnEnter => true;

        protected override BackgroundScreen CreateBackground() => null;

        private ScrollingTeamContainer teamsContainer;
        private GroupContainer groupsContainer;
        private OsuSpriteText fullTeamNameText;

        private readonly List<TournamentTeam> allTeams = new List<TournamentTeam>();

        private DrawingsConfigManager drawingsConfig;

        private Task writeOp;

        private Storage storage;

        public ITeamList TeamList;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, Storage storage)
        {
            RelativeSizeAxes = Axes.Both;

            this.storage = storage;

            if (TeamList == null)
                TeamList = new StorageBackedTeamList(storage);

            if (!TeamList.Teams.Any())
            {
                Exit();
                return;
            }

            drawingsConfig = new DrawingsConfigManager(storage);

            Children = new Drawable[]
            {
                // Main container
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fill,
                            Texture = textures.Get(@"Backgrounds/Drawings/background.png")
                        },
                        // Visualiser
                        new VisualiserContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(1, 10),

                            Colour = new Color4(255, 204, 34, 255),

                            Lines = 6
                        },
                        // Groups
                        groupsContainer = new GroupContainer(drawingsConfig.Get<int>(DrawingsConfig.Groups), drawingsConfig.Get<int>(DrawingsConfig.TeamsPerGroup))
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,

                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,

                            Padding = new MarginPadding
                            {
                                Top = 35f,
                                Bottom = 35f
                            }
                        },
                        // Scrolling teams
                        teamsContainer = new ScrollingTeamContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.X,
                        },
                        // Scrolling team name
                        fullTeamNameText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,

                            Position = new Vector2(0, 45f),

                            Colour = OsuColour.Gray(0.33f),

                            Alpha = 0,

                            Font = "Exo2.0-Light",
                            TextSize = 42f
                        }
                    }
                },
                // Control panel container
                new ControlPanel
                {
                    new TriangleButton
                    {
                        RelativeSizeAxes = Axes.X,

                        Text = "Begin random",
                        Action = teamsContainer.StartScrolling,
                    },
                    new TriangleButton
                    {
                        RelativeSizeAxes = Axes.X,

                        Text = "Stop random",
                        Action = teamsContainer.StopScrolling,
                    },
                    new TriangleButton
                    {
                        RelativeSizeAxes = Axes.X,

                        Text = "Reload",
                        Action = reloadTeams
                    },
                    new ControlPanel.Spacer(),
                    new TriangleButton
                    {
                        RelativeSizeAxes = Axes.X,

                        Text = "Reset",
                        Action = () => reset()
                    }
                }
            };

            teamsContainer.OnSelected += onTeamSelected;
            teamsContainer.OnScrollStarted += () => fullTeamNameText.FadeOut(200);

            reset(true);
        }

        private void onTeamSelected(TournamentTeam team)
        {
            groupsContainer.AddTeam(team);

            fullTeamNameText.Text = team.FullName;
            fullTeamNameText.FadeIn(200);

            writeResults(groupsContainer.GetStringRepresentation());
        }

        private void writeResults(string text)
        {
            void writeAction()
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
            }

            writeOp = writeOp?.ContinueWith(t => { writeAction(); }) ?? Task.Run((Action)writeAction);
        }

        private void reloadTeams()
        {
            teamsContainer.ClearTeams();
            allTeams.Clear();

            foreach (TournamentTeam t in TeamList.Teams)
            {
                if (groupsContainer.ContainsTeam(t.FullName))
                    continue;

                allTeams.Add(t);
                teamsContainer.AddTeam(t);
            }
        }

        private void reset(bool loadLastResults = false)
        {
            groupsContainer.ClearTeams();

            reloadTeams();

            if (!storage.Exists(results_filename))
                return;

            if (loadLastResults)
            {
                try
                {
                    // Read from drawings_results
                    using (Stream stream = storage.GetStream(results_filename, FileAccess.Read, FileMode.Open))
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        string line;
                        while ((line = sr.ReadLine()?.Trim()) != null)
                        {
                            if (string.IsNullOrEmpty(line))
                                continue;

                            if (line.ToUpperInvariant().StartsWith("GROUP"))
                                continue;

                            // ReSharper disable once AccessToModifiedClosure
                            TournamentTeam teamToAdd = allTeams.FirstOrDefault(t => t.FullName == line);

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
