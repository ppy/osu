// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osu.Game.Configuration;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Displays beatmap metadata inside <see cref="PlayerLoader"/>
    /// </summary>
    public class BeatmapMetadataDisplay : Container
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
                        Colour = OsuColour.Gray(0.8f),
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
        private readonly Bindable<IReadOnlyList<Mod>> mods;
        private readonly Drawable facade;
        private LoadingSpinner loading;
        private Sprite backgroundSprite;

        public IBindable<IReadOnlyList<Mod>> Mods => mods;

        public bool Loading
        {
            set
            {
                if (value)
                    loading.Show();
                else
                    loading.Hide();
            }
        }

        public BeatmapMetadataDisplay(WorkingBeatmap beatmap, Bindable<IReadOnlyList<Mod>> mods, Drawable facade)
        {
            this.beatmap = beatmap;
            this.facade = facade;

            this.mods = new Bindable<IReadOnlyList<Mod>>();
            this.mods.BindTo(mods);
        }

        private Container bg;
        private readonly Bindable<bool> Optui = new Bindable<bool>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            var metadata = beatmap.BeatmapInfo?.Metadata ?? new BeatmapMetadata();

            AutoSizeAxes = Axes.Both;
            AddRangeInternal(new Drawable[]
            {
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Masking = false,
                    Children = new Drawable[]
                    {
                        bg = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            CornerRadius = 20,
                            CornerExponent = 2.5f,
                            Masking = true,
                            BorderColour = Color4.Black,
                            BorderThickness = 3f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.5f,
                                    Colour = Color4.Black,
                                }
                            }
                        },
                    new FillFlowContainer
                    {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Direction = FillDirection.Vertical,
                    Children = new[]
                    {
                        facade.With(d =>
                        {
                            d.Anchor = Anchor.TopCentre;
                            d.Origin = Anchor.TopCentre;
                        }),
                        new OsuSpriteText
                        {
                            Text = new LocalisedString((metadata.TitleUnicode, metadata.Title)),
                            Font = OsuFont.GetFont(size: 36, italics: true),
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Margin = new MarginPadding { Top = 15 },
                        },
                        new OsuSpriteText
                        {
                            Text = new LocalisedString((metadata.ArtistUnicode, metadata.Artist)),
                            Font = OsuFont.GetFont(size: 26, italics: true),
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
                            Children = new Drawable[]
                            {
                                backgroundSprite = new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Texture = beatmap?.Background,
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                    FillMode = FillMode.Fill,
                                },
                                loading = new LoadingLayer(backgroundSprite)
                            }
                        },
                        new OsuSpriteText
                        {
                            Text = beatmap?.BeatmapInfo?.Version,
                            Font = OsuFont.GetFont(size: 26, italics: true),
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Margin = new MarginPadding
                            {
                                Bottom = 40
                            },
                        },
                        new MetadataLine("来源", metadata.Source)
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                        },
                        new MetadataLine("作图者", metadata.AuthorString)
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                        },
                        new ModDisplay
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding { Top = 20 },
                            Current = mods
                        }
                    },
                },
                    }
                },
            });

            Loading = true;

            config.BindWith(OsuSetting.OptUI, Optui);
            Optui.ValueChanged += _ => UpdateVisualEffects();

            EntryAnimation();
        }

        private void UpdateVisualEffects()
        {
            switch (Optui.Value)
            {
                case true:
                    bg.ScaleTo(1.2f, 500, Easing.OutQuint).FadeIn(500, Easing.OutQuint);
                    return;

                case false:
                    bg.ScaleTo(1.5f, 500, Easing.OutQuint).FadeOut(500, Easing.OutQuint);
                    return;
            }
        }

        private void EntryAnimation()
        {
            switch (Optui.Value)
            {
                case true:
                    bg.ScaleTo(1.2f);
                    this.FadeOut().ScaleTo(1.5f).Then().Delay(750).FadeIn(500, Easing.OutQuint).ScaleTo(1f, 500, Easing.OutQuint);
                    return;
                
                case false:
                    bg.ScaleTo(1.5f).Then().FadeOut();
                    return;
            }
        }
    }
}
