using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using OpenTK;
using OpenTK.Graphics;
using Symcol.Core.Graphics.Containers;
using Symcol.osu.Mods.Caster.Pieces;
using Symcol.osu.Mods.Caster.CasterScreens.TeamsPieces.Drawables;
using System.Linq;
using System;
using System.IO;
using osu.Framework.Logging;

namespace Symcol.osu.Mods.Caster.CasterScreens.TeamsPieces
{
    public class TeamBox : SymcolContainer
    {
        private const string file_name = "teams.mango";

        private readonly Box box;
        private readonly Triangles triangles;

        private readonly FillFlowContainer<DrawablePlayer> players;
        private readonly FillFlowContainer<DrawableTeam> teams;

        private readonly SettingsDropdown<Team> teamsDropdown;

        private readonly OsuScrollContainer bottomScrollContainer;
        private readonly OsuScrollContainer teamScrollContainer;

        public TeamBox(CasterControlPanel controlPanel)
        { 
            SymcolClickableContainer addPlayer;
            SymcolClickableContainer addTeam;

            OsuColour osu = new OsuColour();

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.48f, 0.96f);

            Masking = true;
            CornerRadius = 8;

            BorderColour = Color4.White;
            BorderThickness = 4;

            Children = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = osu.Gray5,
                },
                triangles = new Triangles
                {
                    RelativeSizeAxes = Axes.Both,
                    ColourLight = osu.Gray1,
                    ColourDark = osu.Gray8,
                    TriangleScale = 2,
                },
                teamScrollContainer = new OsuScrollContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,

                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.96f, 0.48f),
                    Position = new Vector2(0, 6),

                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,

                            Children = new Drawable[]
                            {
                                new SymcolContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,

                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Colour = osu.Yellow,
                                            Text = "Teams",
                                            TextSize = 40
                                        },
                                        addTeam = new SymcolClickableContainer
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.Centre,

                                            Position = new Vector2(-12, 0),
                                            Size = new Vector2(24),
                                            Rotation = 45,
                                            Action = () => teams.Add(new DrawableTeam(new Team
                                            {
                                                Players = new List<Player> { new Player() }
                                            }, teams, controlPanel.Editable)),

                                            Child = new SpriteIcon
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                RelativeSizeAxes = Axes.Both,
                                                Icon = FontAwesome.fa_osu_cross_o,
                                                Colour = Color4.Cyan
                                            }
                                        }
                                    }
                                },
                                teams = new FillFlowContainer<DrawableTeam>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                }
                            }
                        }
                    }
                },
                bottomScrollContainer = new OsuScrollContainer
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,

                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.96f, 0.88f),
                    Position = new Vector2(0, -6),

                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,

                            Children = new Drawable[]
                            {
                                new SymcolContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                   
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Colour = osu.Yellow,
                                            Text = "Players",
                                            TextSize = 40
                                        },
                                        addPlayer = new SymcolClickableContainer
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.Centre,

                                            Alpha = 0,
                                            Position = new Vector2(-12, 0),
                                            Size = new Vector2(24),
                                            Rotation = 45,
                                            Action = () => players.Add(new DrawablePlayer(new Player(), players, controlPanel.Editable)),

                                            Child = new SpriteIcon
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                RelativeSizeAxes = Axes.Both,
                                                Icon = FontAwesome.fa_osu_cross_o,
                                                Colour = Color4.Cyan
                                            }
                                        }
                                    }
                                },
                                players = new FillFlowContainer<DrawablePlayer>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                }
                            }
                        }
                    }
                },
                teamsDropdown = new SettingsDropdown<Team>
                {
                    LabelText = "Team",
                    Position = new Vector2(0, 6),
                    Bindable = new Bindable<Team>()
                }
            };

            teamsDropdown.Bindable.ValueChanged += team =>
            {
                players.Children = new DrawablePlayer[]{};

                foreach (Player p in team.Players)
                    players.Add(new DrawablePlayer(p, players, controlPanel.Editable));
            };


            controlPanel.Stage.ValueChanged += stage =>
            {
                if (controlPanel.Cup == "None" || controlPanel.Year == "None" || controlPanel.Stage == "None") return;
                controlPanel.Editable.TriggerChange();
            };

            controlPanel.Editable.ValueChanged += edit =>
            {
                teamsDropdown.Alpha = edit ? 0 : 1;
                addPlayer.Alpha = edit ? 1 : 0;
                teamScrollContainer.Alpha = edit ? 1 : 0;
                bottomScrollContainer.Height = edit ? 0.48f : 0.88f;

                if (!edit)
                {
                    List<KeyValuePair<string, Team>> ts = new List<KeyValuePair<string, Team>>
                    {
                        new KeyValuePair<string, Team>("None", new Team { Name = "None" })
                    };

                    foreach (Team t in loadTeams(controlPanel))
                        ts.Add(new KeyValuePair<string, Team>(t.Name, t));

                    foreach (DrawableTeam t in teams)
                    {
                        bool b = true;
                        foreach (KeyValuePair<string, Team> pair in ts)
                            if (pair.Value.Name == t.Team.Name)
                                b = false;
                        if (b && t.Team.Name != "None")
                            ts.Add(new KeyValuePair<string, Team>(t.Team.Name, t.Team));

                    }

                    if (teamsDropdown.Bindable.Value != null)
                        foreach (KeyValuePair<string, Team> team in ts)
                            if (team.Value.Name == teamsDropdown.Bindable.Value.Name)
                            {
                                team.Value.Players = new List<Player>();
                                foreach (DrawablePlayer player in players)
                                    team.Value.Players.Add(player.Player);
                            }

                    teamsDropdown.Items = ts;
                    if (teamsDropdown.Bindable.Value == null)
                        teamsDropdown.Bindable.Value = ts.First().Value;

                    save(controlPanel);
                }
                else
                {
                    teams.Children = new DrawableTeam[]{};
                    foreach (KeyValuePair<string, Team> pair in teamsDropdown.Items)
                        teams.Add(new DrawableTeam(pair.Value, teams, controlPanel.Editable));
                }
            };
            controlPanel.Editable.TriggerChange();
        }

        private List<Team> loadTeams(CasterControlPanel controlPanel)
        {
            List<Team> teamsList = new List<Team>();

            if (controlPanel.Cup == "None" || controlPanel.Year == "None" || controlPanel.Stage == "None")
            {
                return teamsList;
            }

            string[] teamsRaw = null;

            try
            {
                using (Stream stream = controlPanel.GetStream(controlPanel.GetStreamPath(file_name), FileAccess.Read, FileMode.Open))
                using (StreamReader reader = controlPanel.GetStreamReader(stream))
                    teamsRaw = reader.ReadToEnd().Split('\n');
            }
            catch { Logger.Log("Teams file doesn't exist!", LoggingTarget.Database, LogLevel.Important); return teamsList; }

            foreach (string teamRaw in teamsRaw)
            {
                string[] teamArgs = teamRaw.Split('|');

                Team team = new Team();

                foreach (string teamArgRaw in teamArgs)
                {
                    string[] teamArgArgsRaw = teamArgRaw.Split(':');

                    for (int i = 0; i < teamArgArgsRaw.Count(); i++)
                    {
                        string arg = teamArgArgsRaw[i];

                        if (arg == "Name")
                            team.Name = teamArgArgsRaw[i + 1];

                        if (arg == "Players")
                        {
                            string[] playersRaw = teamArgArgsRaw[i + 1].Split(',');

                            List<Player> playersList = new List<Player>();

                            foreach (string playerRaw in playersRaw)
                            {
                                string[] playerArgsRaw = playerRaw.Split('.');

                                playersList.Add(new Player
                                {
                                    Name = playerArgsRaw[0],
                                    PlayerID = Int32.Parse(playerArgsRaw[1])
                                });
                            }

                            team.Players = playersList;
                        }
                    }
                }

                teamsList.Add(team);
            }

            return teamsList;
        }

        private void save(CasterControlPanel controlPanel)
        {
            if (controlPanel.Cup == "None" || controlPanel.Year == "None" || controlPanel.Stage == "None")
            {
                return;
            }

            string teamsRaw = "Name:Symcol|Players:Shawdooow.7726082";

            bool first = true;

            foreach (KeyValuePair<string, Team> pair in teamsDropdown.Items)
                if (pair.Value.Name != "None")
                {
                    Team team = pair.Value;

                    //TODO: simplify this code?
                    if (first)
                    {
                        teamsRaw = $"Name:{team.Name}|Players:";

                        for (int i = 0; i < team.Players.Count; i++)
                        {
                            Player p = team.Players[i];

                            teamsRaw = teamsRaw + $"{p.Name}.{p.PlayerID}";

                            if (team.Players.Count > i + 1)
                                teamsRaw = teamsRaw + ",";
                        }

                        first = false;
                    }
                    else
                    {
                        teamsRaw = teamsRaw + $"\nName:{team.Name}|Players:";

                        for (int i = 0; i < team.Players.Count; i++)
                        {
                            Player p = team.Players[i];

                            teamsRaw = teamsRaw + $"{p.Name}.{p.PlayerID}";

                            if (team.Players.Count > i + 1)
                                teamsRaw = teamsRaw + ",";
                        }
                    }
                }

            using (Stream stream = controlPanel.GetStream(controlPanel.GetStreamPath(file_name), FileAccess.Write, FileMode.OpenOrCreate))
            using (StreamWriter writer = controlPanel.GetStreamWriter(stream))
                writer.Write(teamsRaw);
        }
    }
}
