using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Audio;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Database;
using osu.Game.Screens.Select;
using osu.Game.Graphics;
using System.Collections.Generic;
using osu.Framework.MathUtils;

namespace osu.Game
{
    public class BeatmapOptions : OverlayContainer, IStateful<BeatmapOptionsState>
    {

        private Box backgroundBox;
        private const float width = 500;
        private FlowContainer buttonFlow;
        private FlowContainer header;
        private OsuGameBase osuGameBase;

        private BeatmapOptionsState state;
        public new BeatmapOptionsState State
        {
            get { return state; }
            set
            {
                state = value;
                switch (state)
                {
                    case BeatmapOptionsState.Delete:
                        applyDeleteState();
                        break;
                    case BeatmapOptionsState.Initial:
                        applyInitialState();
                        break;
                }
            }
        }

        public BeatmapOptions()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Width = width;
            RelativeSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                new BeatmapOptionsTriangles
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                backgroundBox = new Box
                {
                    Colour = new Color4(0, 0, 0, 255),
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.90f,
                },
                new FlowContainer
                {
                    Direction = FlowDirection.VerticalOnly,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        header = new FlowContainer
                        {
                            Direction = FlowDirection.VerticalOnly,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Bottom = -40,
                            },
                        },
                        buttonFlow = new FlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Direction = FlowDirection.VerticalOnly,
                            Spacing = new Vector2(0, 20),
                        },
                    }
                }
            };

            applyInitialState();
            PopOut();
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame, Database.BeatmapDatabase beatmaps, AudioManager audio, TextureStore textures)
        {
            osuGameBase = osuGame;
        }

        private void resetStateAppearance()
        {
            buttonFlow.Clear();
            header.Clear();
        }

        private void applyInitialState()
        {
            resetStateAppearance();
            buttonFlow.Add(new Drawable[]
            {
                new Button
                {
                    Width = 350,
                    Height = 50,
                    Text = "Manage Collections",
                    Colour = new Color4(238, 51, 153, 255),
                },
                new Button
                {
                    Width = 350,
                    Height = 50,
                    Text = "Delete",
                    Colour = new Color4(238, 51, 153, 255),
                    Action = () => State = BeatmapOptionsState.Delete,
                },
                new Button
                {
                    Width = 350,
                    Height = 50,
                    Text = "Remove from Unplayed",
                    Colour = new Color4(238, 51, 153, 255),
                },
                new Button
                {
                    Width = 350,
                    Height = 50,
                    Text = "Clear local Scores",
                    Colour = new Color4(238, 51, 153, 255),
                },
                new Button
                {
                    Width = 350,
                    Height = 50,
                    Text = "Edit",
                    Colour = new Color4(238, 51, 153, 255),
                },
                new Button
                {
                    Width = 350,
                    Height = 50,
                    Text = "Cancel",
                    Colour = new Color4(238, 51, 153, 255),
                    Action = ToggleVisibility,
                },
            });
        }

        private void applyDeleteState()
        {
            resetStateAppearance();
            header.Add(new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding
                    {
                        Bottom = 20,
                    },
                    Children = new TextAwesome[]
                    {
                        new TextAwesome
                        {
                            Icon = FontAwesome.fa_circle_thin,
                            TextSize = 120,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        new TextAwesome
                        {
                            Icon = FontAwesome.fa_trash_o,
                            TextSize = 50,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    },
                },
                new SpriteText
                {
                    Text = "Delete Beatmap",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding
                    {
                        Bottom = 20,
                    },
                },
                new SpriteText
                {
                    Text = "Confirm deletion of",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = new Color4(0, 229, 229, 255),
                },
                new SpriteText
                {
                    Text = $"{osuGameBase.Beatmap.Value.BeatmapInfo.Metadata.Artist}" + " - " + $"{osuGameBase.Beatmap.Value.BeatmapInfo.Metadata.Title}",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = @"Exo2.0-SemiBoldItalic",
                },
            });

            buttonFlow.Add(new Drawable[]
            {
                new Button
                {
                    Width = 350,
                    Height = 50,
                    Text = "Yes. Totally. Delete it.",
                    Colour = new Color4(238, 51, 153, 255),
                },
                new Button
                {
                    Width = 350,
                    Height = 50,
                    Text = "Firetruck, I didn't mean to!",
                    Colour = new Color4(0, 229, 229, 255),
                    Action = () => State = BeatmapOptionsState.Initial,
                }
            });
        }

        private void reset()
        {
            State = BeatmapOptionsState.Initial;
            Hide();
        }

        private const int transition_length = 200;

        protected override void PopIn()
        {
            MoveToY(0, transition_length, EasingTypes.In);
        }

        protected override void PopOut()
        {
            MoveToY(ScreenSpaceDrawQuad.Height, transition_length, EasingTypes.Out);
        }

        public override void Hide()
        {
            base.Hide();
            State = BeatmapOptionsState.Initial;
        }

        class BeatmapOptionsTriangles : Container
        {
            private Texture triangle;
            private const int num_triangles = 300;

            public BeatmapOptionsTriangles()
            {
                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                triangle = textures.Get(@"Play/osu/triangle@2x");
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                for (int i = 0; i < num_triangles; i++)
                {
                    Add(new Sprite
                    {
                        Texture = triangle,
                        Origin = Anchor.Centre,
                        RelativePositionAxes = Axes.Both,
                        Position = new Vector2(RNG.NextSingle(), RNG.NextSingle()),
                        Scale = new Vector2(RNG.NextSingle() * 0.4f + 0.2f),
                        Alpha = RNG.NextSingle() * 0.3f,
                        Colour = new Color4(RNG.NextSingle(),RNG.NextSingle(),RNG.NextSingle(), 255),
                    });
                }
            }

            protected override void Update()
            {
                base.Update();

                foreach (Drawable d in Children)
                {
                    d.Position -= new Vector2(0, (float)(d.Scale.X * (Time.Elapsed / 2880)));
                    if (d.DrawPosition.Y + d.DrawSize.Y * d.Scale.Y < 0)
                        d.MoveToY(1);
                }
            }
        }
    }

    public enum BeatmapOptionsState
    {
        Initial,
        Delete,
    }
}
