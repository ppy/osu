// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Tournament.Components;
using osu.Game.Screens.Tournament.Teams;
using osuTK;
using osuTK.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;

namespace osu.Game.Screens.Tournament
{
    public class Drawings : OsuScreen
    {
        private const string results_filename = "drawings_results.txt";

        public override bool HideOverlaysOnEnter => true;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenDefault();

        private ScrollingTeamContainer teamsContainer;
        private GroupContainer groupsContainer;
        private OsuSpriteText fullTeamNameText;

        private readonly List<DrawingsTeam> allTeams = new List<DrawingsTeam>();

        private DrawingsConfigManager drawingsConfig;

        private Task writeOp;

        private Storage storage;

        public ITeamList TeamList;

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, Storage storage)
        {
            this.storage = storage;

            TextureStore flagStore = new TextureStore();
            // Local flag store
            flagStore.AddStore(new TextureLoaderStore(new NamespacedResourceStore<byte[]>(new StorageBackedResourceStore(storage), "Drawings")));
            // Default texture store
            flagStore.AddStore(textures);

            dependencies.Cache(flagStore);

            if (TeamList == null)
                TeamList = new StorageBackedTeamList(storage);

            if (!TeamList.Teams.Any())
            {
                this.Exit();
                return;
            }

            drawingsConfig = new DrawingsConfigManager(storage);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(77, 77, 77, 255)
                },
                new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                    Texture = textures.Get(@"Backgrounds/Drawings/background.png")
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,

                    Children = new Drawable[]
                    {
                        // Main container
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.85f,

                            Children = new Drawable[]
                            {
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

                                    Alpha = 0,

                                    Font = OsuFont.GetFont(weight: FontWeight.Light, size: 42),
                                }
                            }
                        },
                        // Control panel container
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.15f,

                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = new Color4(54, 54, 54, 255)
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,

                                    Text = "Control Panel",
                                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 22),
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,

                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 0.75f,

                                    Position = new Vector2(0, 35f),

                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5f),

                                    Children = new Drawable[]
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
                                        }
                                    }
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,

                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 0.75f,

                                    Position = new Vector2(0, -5f),

                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5f),

                                    Children = new Drawable[]
                                    {
                                        new TriangleButton
                                        {
                                            RelativeSizeAxes = Axes.X,

                                            Text = "Reset",
                                            Action = () => reset()
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

        private void onTeamSelected(DrawingsTeam team)
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

            foreach (DrawingsTeam t in TeamList.Teams)
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
                            DrawingsTeam teamToAdd = allTeams.Find(t => t.FullName == line);

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
