// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Core.Screens.Evast;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using OpenTK;
using OpenTK.Graphics;
using Symcol.osu.Core.Evast.MusicVisualizers;
using Symcol.osu.Core.Evast.Visualizers;

namespace Symcol.osu.Core.KoziLord.EvastModded.MusicPlayers
{

    //TODO: Adding media controls and the entry animation entry animation.
    public class FullscreenPlayer : BeatmapScreen
    {
        private BeatmapSprite beatmapSprite;

        public Container Visualizer;

        public Container MediaControls;

        public SpriteText Title;
        public SpriteText Artist;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new SpaceParticlesContainer(),
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,

                    Children = new Drawable[]
                    {
                        Visualizer = new Container
                        {
                            Scale = new Vector2(0.4f),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(350),
                            Children = new Drawable[]
                            {
                                new CircularVisualizer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    DegreeValue = 180,
                                    BarsAmount = 100,
                                    CircleSize = 348,
                                    BarWidth = 2,
                                },
                                new CircularVisualizer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    DegreeValue = 180,
                                    BarsAmount = 100,
                                    CircleSize = 348,
                                    BarWidth = 2,
                                    Rotation = 180,
                                },
                                new CircularContainer
                                {
                                   Anchor = Anchor.Centre,
                                   Origin = Anchor.Centre,
                                   Size = new Vector2(350),
                                   Masking = true,
                                   EdgeEffect = new EdgeEffectParameters
                                   {
                                       Type = EdgeEffectType.Shadow,
                                       Colour = Color4.Black.Opacity(0.18f),
                                       Offset = new Vector2(0, 2),
                                       Radius = 24,
                                    },
                                   Child = beatmapSprite = new BeatmapSprite
                                   {
                                       RelativeSizeAxes = Axes.Both,
                                       Anchor = Anchor.Centre,
                                       Origin = Anchor.Centre,
                                       FillMode = FillMode.Fill,
                                   }
                                }

                            },

                        },
                        Title = new SpriteText
                        {
                            AlwaysPresent = true,
                            Alpha = 0,
                            Scale = new Vector2(0.6f),
                            Margin = new MarginPadding{Top = 20},
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = @"Song Name",
                            Font = @"Exo2.0-Medium",
                            TextSize = 56,
                            Shadow = true

                        },
                        Artist = new SpriteText
                        {
                            AlwaysPresent = true,
                            Alpha = 0,
                            Scale = new Vector2(0.6f),
                            Margin = new MarginPadding{Top = 10},
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = @"Artist",
                            Font = @"Exo2.0-MediumItalic",
                            TextSize = 36,
                            Shadow = true
                        },
                        MediaControls = new Container
                        {
                            Scale = new Vector2(0.6f),
                            Alpha = 0,
                            AlwaysPresent = true,
                            Height = 100,
                            Width = 400,
                            CornerRadius = 16,
                            Margin = new MarginPadding{Top = 20},
                            Masking = true,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White.Opacity(0.1f),
                                }
                            }
                        }
                    }
                },
              
            };
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            Visualizer.ScaleTo(1f, 500, Easing.OutQuad);
            Title.Delay(150).ScaleTo(1f, 500, Easing.OutQuad).FadeIn(500,Easing.Out);
            Artist.Delay(300).ScaleTo(1f, 500, Easing.OutQuad).FadeIn(500,Easing.Out);
            MediaControls.Delay(400).ScaleTo(1f, 500, Easing.OutQuad).FadeIn(500, Easing.Out);
        }

        protected override void OnBeatmapChange(WorkingBeatmap beatmap)
        {
            base.OnBeatmapChange(beatmap);
            beatmapSprite.UpdateTexture(beatmap);

            Title.Text = beatmap.Beatmap.Metadata.Title;
            Artist.Text = beatmap.Beatmap.Metadata.Artist;
        }

        private class BeatmapSprite : Sprite
        {
            public void UpdateTexture(WorkingBeatmap beatmap)
            {
                if (beatmap.Background != null)
                    Texture = beatmap.Background;
            }
        }
    }
}
