// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select.Leaderboards;
using System.Linq;
using OpenTK.Graphics;
using System.Diagnostics;
using osu.Framework.Input;
using osu.Framework.Localisation;
using System.Globalization;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class DrawablePlay : Container
    {
        private readonly FillFlowContainer<OsuSpriteText> stats;
        private readonly FillFlowContainer metadata;
        private readonly ModContainer modContainer;
        private readonly Play play;
        private readonly double weight;

        public DrawablePlay(Play play, double weight = -1)
        {
            this.play = play;
            this.weight = weight;

            Children = new Drawable[]
            {
                new DrawableRank(play.Rank)
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 60,
                    FillMode = FillMode.Fit,
                },
                stats = new FillFlowContainer<OsuSpriteText>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Direction = FillDirection.Vertical,
                },
                metadata = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding { Left = 70 },
                    Direction = FillDirection.Vertical,
                    Child = new OsuSpriteText
                    {
                        Text = play.Date.LocalDateTime.ToShortDateString(),
                        TextSize = 11,
                        Colour = OsuColour.Gray(0xAA),
                        Depth = -1,
                    },
                },
                modContainer = new ModContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Width = 60,
                    Margin = new MarginPadding{ Right = 140 }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, LocalisationEngine locale)
        {
            stats.Add(new OsuSpriteText {
                Text = play.PerformancePoints + "pp",
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                TextSize = 18,
                Font = "Exo2.0-BoldItalic",
            });
            if(weight != -1)
            {
                stats.Add(new OsuSpriteText
                {
                    Text = $"weighted: {(int)(play.PerformancePoints * weight)}pp ({weight.ToString("0%", CultureInfo.CurrentCulture)})",
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Colour = colour.GrayA,
                    TextSize = 11,
                    Font = "Exo2.0-RegularItalic",
            });
            }
            stats.Add(new OsuSpriteText {
                Text = "accuracy: " + play.Accuracy.ToString("0.00%"),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Colour = colour.GrayA,
                TextSize = 11,
                Font = "Exo2.0-RegularItalic",
            });

            metadata.Add(new LinkContainer
            {
                AutoSizeAxes = Axes.Both,
                Url = $"https://osu.ppy.sh/beatmaps/{play.Beatmap.OnlineBeatmapID}",
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Current = locale.GetUnicodePreference($"{play.Beatmap.Metadata.TitleUnicode ?? play.Beatmap.Metadata.Title} [{play.Beatmap.Version}] ", $"{play.Beatmap.Metadata.Title ?? play.Beatmap.Metadata.TitleUnicode} [{play.Beatmap.Version}] "),
                            TextSize = 15,
                            Font = "Exo2.0-SemiBoldItalic",
                        },
                        new OsuSpriteText
                        {
                            Current = locale.GetUnicodePreference(play.Beatmap.Metadata.ArtistUnicode, play.Beatmap.Metadata.Artist),
                            TextSize = 12,
                            Padding = new MarginPadding { Top = 3 },
                            Font = "Exo2.0-RegularItalic",
                        },
                    },
                },
            });

            foreach (Mod mod in play.Mods)
                modContainer.Add(new ModIcon(mod.Icon, colour.Yellow));
        }

        private class ModContainer : FlowContainer<ModIcon>
        {
            protected override IEnumerable<Vector2> ComputeLayoutPositions()
            {
                int count = FlowingChildren.Count();
                for (int i = 0; i < count; i++)
                    yield return new Vector2(DrawWidth * i * (count == 1 ? 0 : 1f / (count - 1)), 0);
            }
        }

        private class ModIcon : Container
        {
            public ModIcon(FontAwesome icon, Color4 colour)
            {
                AutoSizeAxes = Axes.Both;

                Children = new[]
                {
                    new TextAwesome
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Icon = FontAwesome.fa_osu_mod_bg,
                        Colour = colour,
                        Shadow = true,
                        TextSize = 30,
                        UseFullGlyphHeight = false,
                    },
                    new TextAwesome
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Icon = icon,
                        Colour = OsuColour.Gray(84),
                        TextSize = 18,
                        Position = new Vector2(0f, 2f),
                        UseFullGlyphHeight = false,
                    },
                };
            }
        }

        private class LinkContainer : OsuClickableContainer
        {
            public string Url;

            private Color4 hoverColour;

            public LinkContainer()
            {
                Action = () => Process.Start(Url);
            }

            protected override bool OnHover(InputState state)
            {
                this.FadeColour(hoverColour, 500, Easing.OutQuint);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                this.FadeColour(Color4.White, 500, Easing.OutQuint);
                base.OnHoverLost(state);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                hoverColour = colours.Yellow;
            }
        }
    }
}
