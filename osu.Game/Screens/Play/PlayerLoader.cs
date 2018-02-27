﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using osu.Framework.Localisation;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Screens.Play
{
    public class PlayerLoader : OsuScreen
    {
        private Player player;

        private BeatmapMetadataDisplay info;
        private VisualSettings visualSettings;

        private bool showOverlays = true;
        public override bool ShowOverlaysOnEnter => showOverlays;

        public override bool AllowBeatmapRulesetChange => false;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        public PlayerLoader(Player player)
        {
            this.player = player;

            player.RestartRequested = () =>
            {
                showOverlays = false;
                ValidForResume = true;
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
            Add(visualSettings = new VisualSettings
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Margin = new MarginPadding(25)
            });

            LoadComponentAsync(player);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            contentIn();

            //we will only be resumed if the player has requested a re-run (see ValidForResume setting above)
            LoadComponentAsync(player = new Player
            {
                RestartCount = player.RestartCount + 1,
                RestartRequested = player.RestartRequested,
            });

            this.Delay(400).Schedule(pushWhenLoaded);
        }

        private void contentIn()
        {
            Content.ScaleTo(1, 650, Easing.OutQuint);
            Content.FadeInFromZero(400);
        }

        private void contentOut()
        {
            Content.ScaleTo(0.7f, 300, Easing.InQuint);
            Content.FadeOut(250);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            Background.FadeTo(0.4f, 250);

            Content.ScaleTo(0.7f);

            contentIn();

            info.Delay(750).FadeIn(500);
            this.Delay(2150).Schedule(pushWhenLoaded);
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            logo.RelativePositionAxes = Axes.Both;

            logo.ScaleTo(new Vector2(0.15f), 300, Easing.In);
            logo.MoveTo(new Vector2(0.5f), 300, Easing.In);
            logo.FadeIn(350);

            logo.Delay(resuming ? 0 : 500).MoveToOffset(new Vector2(0, -0.24f), 500, Easing.InOutExpo);
        }

        private void pushWhenLoaded()
        {
            if (player.LoadState != LoadState.Ready || visualSettings.IsHovered)
            {
                Schedule(pushWhenLoaded);
                return;
            }

            contentOut();

            this.Delay(250).Schedule(() =>
            {
                if (!IsCurrentScreen) return;

                if (!Push(player))
                    Exit();
                else
                {
                    //By default, we want to load the player and never be returned to.
                    //Note that this may change if the player we load requested a re-run.
                    ValidForResume = false;
                }
            });
        }

        protected override bool OnExiting(Screen next)
        {
            Content.ScaleTo(0.7f, 150, Easing.InQuint);
            this.FadeOut(150);

            return base.OnExiting(next);
        }

        private class BeatmapMetadataDisplay : Container
        {
            private class MetadataLine : Container
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

            private readonly WorkingBeatmap beatmap;

            public BeatmapMetadataDisplay(WorkingBeatmap beatmap)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load(LocalisationEngine localisation)
            {
                var metadata = beatmap?.BeatmapInfo?.Metadata ?? new BeatmapMetadata();

                AutoSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Current = localisation.GetUnicodePreference(metadata.TitleUnicode, metadata.Title),
                                TextSize = 36,
                                Font = @"Exo2.0-MediumItalic",
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                            new OsuSpriteText
                            {
                                Current = localisation.GetUnicodePreference(metadata.ArtistUnicode, metadata.Artist),
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
                                        RelativeSizeAxes = Axes.Both,
                                        Texture = beatmap?.Background,
                                        Origin = Anchor.Centre,
                                        Anchor = Anchor.Centre,
                                        FillMode = FillMode.Fill,
                                    },
                                }
                            },
                            new OsuSpriteText
                            {
                                Text = beatmap?.BeatmapInfo?.Version,
                                TextSize = 26,
                                Font = @"Exo2.0-MediumItalic",
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                Margin = new MarginPadding
                                {
                                    Bottom = 40
                                },
                            },
                            new MetadataLine("Source", metadata.Source)
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                            new MetadataLine("Composer", string.Empty)
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                            new MetadataLine("Mapper", metadata.AuthorString)
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
