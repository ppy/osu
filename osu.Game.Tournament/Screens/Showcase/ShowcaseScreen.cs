// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform.Windows;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Tournament.Components;
using OpenTK;

namespace osu.Game.Tournament.Screens.Showcase
{
    public class ShowcaseScreen : OsuScreen
    {
        private readonly Container panel;
        private readonly Container panelContents;

        [Resolved]
        private APIAccess api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private int lastBeatmapId;
        private int lastMods;

        [BackgroundDependencyLoader]
        private void load()
        {
            var stable = new StableStorage();

            const string file_ipc_filename = "ipc.txt";

            if (stable.Exists(file_ipc_filename))
            {
                Scheduler.AddDelayed(delegate
                {
                    try
                    {
                        using (var stream = stable.GetStream(file_ipc_filename))
                        using (var sr = new StreamReader(stream))
                        {
                            var beatmapId = int.Parse(sr.ReadLine());
                            var mods = int.Parse(sr.ReadLine());

                            if (lastBeatmapId == beatmapId)
                                return;

                            lastMods = mods;
                            lastBeatmapId = beatmapId;

                            var req = new GetBeatmapRequest(new BeatmapInfo { OnlineBeatmapID = beatmapId });
                            req.Success += success;
                            api.Queue(req);
                        }
                    }
                    catch
                    {
                        // file might be in use.
                    }
                }, 250, true);
            }
        }

        private void success(APIBeatmap apiBeatmap)
        {
            panel.FadeInFromZero(300, Easing.OutQuint);

            var beatmap = apiBeatmap.ToBeatmap(rulesets);

            var legacyMods = (LegacyMods)lastMods;
            var bpm = beatmap.BeatmapSet.OnlineInfo.BPM;
            var length = beatmap.OnlineInfo.Length;
            string extra = "";

            var ar = beatmap.BaseDifficulty.ApproachRate;
            if ((legacyMods & LegacyMods.HardRock) > 0)
            {
                //ar *= 1.4f;
                extra = "*";
            }

            if ((legacyMods & LegacyMods.DoubleTime) > 0)
            {
                //ar *= 1.5f;
                bpm *= 1.5f;
                length /= 1.5f;
                extra = "*";
            }

            panelContents.Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = $"Length {length}s",
                    Margin = new MarginPadding { Horizontal = 15, Vertical = 5 },
                    Colour = OsuColour.Gray(0.33f),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                new OsuSpriteText
                {
                    Text = $"BPM {bpm:0.#}",
                    Margin = new MarginPadding { Horizontal = 15, Vertical = 5 },
                    Colour = OsuColour.Gray(0.33f),
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft
                },
                new OsuSpriteText
                {
                    Text = $"AR {ar:0.#}{extra}",
                    Margin = new MarginPadding { Horizontal = 15, Vertical = 5 },
                    Colour = OsuColour.Gray(0.33f),
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight
                },
                new OsuSpriteText
                {
                    Text = $"Star Rating {beatmap.StarDifficulty:0.#}{extra}",
                    Margin = new MarginPadding { Horizontal = 15, Vertical = 5 },
                    Colour = OsuColour.Gray(0.33f),
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight
                },
                new TournamentBeatmapPanel(beatmap)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };
        }

        public ShowcaseScreen()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new Container
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = -10,
                    Width = 0.95f,
                    Height = TournamentBeatmapPanel.HEIGHT,
                    CornerRadius = TournamentBeatmapPanel.HEIGHT / 2,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.Gray(0.93f),
                        },
                        panel = new Container
                        {
                            Masking = true,
                            CornerRadius = TournamentBeatmapPanel.HEIGHT / 2,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.7f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.Gray(0.86f),
                                },
                                panelContents = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                }
                            }
                        },
                        new OsuLogo
                        {
                            Triangles = false,
                            Colour = OsuColour.Gray(0.33f),
                            Scale = new Vector2(0.08f),
                            Margin = new MarginPadding(50),
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                        }
                    }
                }
            };
        }

        /// <summary>
        /// A method of accessing an osu-stable install in a controlled fashion.
        /// </summary>
        private class StableStorage : WindowsStorage
        {
            protected override string LocateBasePath()
            {
                bool checkExists(string p) => Directory.Exists(Path.Combine(p, "Songs"));

                string stableInstallPath;

                try
                {
                    stableInstallPath = "E:\\osu!mappool";

                    if (checkExists(stableInstallPath))
                        return stableInstallPath;
                }
                catch
                {
                }

                stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"osu!");
                if (checkExists(stableInstallPath))
                    return stableInstallPath;

                stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".osu");
                if (checkExists(stableInstallPath))
                    return stableInstallPath;

                return null;
            }

            public StableStorage()
                : base(string.Empty, null)
            {
            }
        }
    }
}
