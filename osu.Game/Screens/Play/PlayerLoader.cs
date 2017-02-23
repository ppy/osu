// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Menu;
using OpenTK;

namespace osu.Game.Screens.Play
{
    public class PlayerLoader : OsuScreen
    {
        private readonly Player player;
        private OsuLogo logo;
        private BeatmapMetadataDisplay info;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        public PlayerLoader(Player player)
        {
            ValidForResume = false;
            this.player = player;

            Children = new Drawable[]
            {
                logo = new OsuLogo
                {
                    Scale = new Vector2(0.15f),
                    Interactive = false,
                },
            };

        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(info = new BeatmapMetadataDisplay(Beatmap)
            {
                Alpha = 0,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            player.Preload(Game);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            Background.FadeTo(0.4f, 250);

            Content.ScaleTo(0.7f);
            Content.ScaleTo(1, 750, EasingTypes.OutQuint);
            Content.FadeInFromZero(500);

            Delay(1000, true);

            logo.MoveToOffset(new Vector2(0, -180), 500, EasingTypes.InOutExpo);
            Delay(250, true);

            info.FadeIn(500);

            Delay(2000, true);

            Content.ScaleTo(0.7f, 300, EasingTypes.InQuint);
            Content.FadeOut(250);

            Delay(250);

            Schedule(() =>
            {
                if (!Push(player))
                    Exit();
            });
        }

        protected override bool OnExiting(Screen next)
        {
            Content.ScaleTo(0.7f, 150, EasingTypes.InQuint);
            FadeOut(150);
            return base.OnExiting(next);
        }

        class BeatmapMetadataDisplay : Container
        {
            class MetadataLine : Container
            {
                public MetadataLine(string left, string right)
                {
                    AutoSizeAxes = Axes.Both;
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopRight,
                            Margin = new MarginPadding { Right = 5 },
                            Colour = OsuColour.Gray(0.5f),
                            Text = left,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopLeft,
                            Margin = new MarginPadding { Left = 5 },
                            Text = string.IsNullOrEmpty(right) ? @"-" : right,
                        }
                    };
                }

            }

            public BeatmapMetadataDisplay(WorkingBeatmap beatmap)
            {
                AutoSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    new FlowContainer()
                    {
                        AutoSizeAxes = Axes.Both,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Direction = FlowDirections.Vertical,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = beatmap.BeatmapInfo.Metadata.Title,
                                TextSize = 36,
                                Font = @"Exo2.0-MediumItalic",
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                            new OsuSpriteText
                            {
                                Text = beatmap.BeatmapInfo.Metadata.Artist,
                                TextSize = 26,
                                Font = @"Exo2.0-MediumItalic",
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                            new Container
                            {
                                Size = new Vector2(300, 60),
                                Margin = new MarginPadding(10),
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                CornerRadius = 10,
                                Masking = true,
                                Children = new[]
                                {
                                    new Sprite
                                    {
                                        Texture = beatmap.Background,
                                        Origin = Anchor.Centre,
                                        Anchor = Anchor.Centre,
                                        FillMode = FillMode.Fill,
                                    },
                                }
                            },
                            new OsuSpriteText
                            {
                                Text = beatmap.BeatmapInfo.Version,
                                TextSize = 26,
                                Font = @"Exo2.0-MediumItalic",
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                Margin = new MarginPadding
                                {
                                    Bottom = 40
                                },
                            },
                            new MetadataLine("Source", beatmap.BeatmapInfo.Metadata.Source)
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                            new MetadataLine("Composer", string.Empty)
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                            new MetadataLine("Mapper", beatmap.BeatmapInfo.Metadata.Author)
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                        },
                    }
                };
            }
        }
    }
}
