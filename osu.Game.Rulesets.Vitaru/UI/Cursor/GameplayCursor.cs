using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Game.Rulesets.Vitaru.Objects.Characters;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Vitaru.UI.Cursor
{
    public class GameplayCursor : CursorContainer, IKeyBindingHandler<VitaruAction>
    {
        protected override Drawable CreateCursor() => new VitaruCursor();

        public GameplayCursor()
        {
            Masking = false;
        }

        public class VitaruCursor : Container
        {
            private readonly Characters currentCharacter = VitaruSettings.VitaruConfigManager.GetBindable<Characters>(VitaruSetting.Characters);

            private Container lineContainer;
            private Container circleContainer;
            public static CircularContainer CenterCircle;

            private SpriteText health;
            private SpriteText energy;
            private SpriteText speed;
            private SpriteText combo;

            private Bindable<double> cursorScale;
            private Bindable<bool> autoCursorScale;
            private Bindable<WorkingBeatmap> beatmap;

            public VitaruCursor()
            {
                Origin = Anchor.Centre;
                Size = new Vector2(32);
                Masking = false;
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config, OsuGameBase game, OsuColour osu)
            {
                Children = new Drawable[]
                {
                    new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(Size.X + Size.X / 3.5f),
                        Texture = VitaruRuleset.VitaruTextures.Get("ring")
                    },
                    CenterCircle = new CircularContainer
                    {
                        Masking = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(Size.X / 5),
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    },
                    lineContainer = new Container
                    {
                        Masking = false,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Rotation = 45,

                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Masking = true,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                CornerRadius = Size.X / 12,
                                Size = new Vector2(Size.X / 3, Size.X / 7),
                                Position = new Vector2(Size.X / 3, 0),
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            },
                            new Container
                            {
                                Masking = true,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                CornerRadius = Size.X / 12,
                                Size = new Vector2(Size.X / 3, Size.X / 7),
                                Position = new Vector2(-1 * Size.X / 3, 0),
                                Rotation = 180,
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            },
                            new Container
                            {
                                Masking = true,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                CornerRadius = Size.X / 12,
                                Size = new Vector2(Size.X / 3, Size.X / 7),
                                Position = new Vector2(0, Size.X / 3),
                                Rotation = 90,
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            },
                            new Container
                            {
                                Masking = true,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                CornerRadius = Size.X / 12,
                                Size = new Vector2(Size.X / 3, Size.X / 7),
                                Position = new Vector2(0, -1 * Size.X / 3),
                                Rotation = 270,
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            }
                        }
                    },
                    circleContainer = new Container
                    {
                        Masking = false,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,

                        Children = new Drawable[]
                        {
                            new CircularContainer
                            {
                                Masking = true,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(Size.X / 8),
                                Position = new Vector2(Size.X / 4, 0),
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            },
                            new CircularContainer
                            {
                                Masking = true,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(Size.X / 8),
                                Position = new Vector2(-1 * Size.X / 4, 0),
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            },
                            new CircularContainer
                            {
                                Masking = true,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(Size.X / 8),
                                Position = new Vector2(0, Size.X / 4),
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            },
                            new CircularContainer
                            {
                                Masking = true,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(Size.X / 8),
                                Position = new Vector2(0, -1 * Size.X / 4),
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            }
                        }
                    },
                    health = new SpriteText
                    {
                        Alpha = 0.5f,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Position = new Vector2(-40, 0),
                        Colour = Color4.Green,
                        Text = "null"
                    },
                    energy = new SpriteText
                    {
                        Alpha = 0.5f,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Position = new Vector2(40, 0),
                        Colour = Color4.Blue,
                        Text = "null"
                    },
                    speed = new SpriteText
                    {
                        Alpha = 0f,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Position = new Vector2(0, -40),
                        Colour = Color4.Yellow,
                        Text = "null"
                    },
                    combo = new SpriteText
                    {
                        Alpha = 0f,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Position = new Vector2(0, 40),
                        Colour = osu.Pink,
                        Text = "null"
                    }
                };

                if (currentCharacter == Characters.SakuyaIzayoi | currentCharacter == Characters.AliceMuyart)
                    speed.Alpha = 0.5f;

                if (currentCharacter == Characters.KokoroHatano)
                    combo.Alpha = 0.5f;

                beatmap = game.Beatmap.GetBoundCopy();
                beatmap.ValueChanged += v => calculateScale();

                cursorScale = config.GetBindable<double>(OsuSetting.GameplayCursorSize);
                cursorScale.ValueChanged += v => calculateScale();

                autoCursorScale = config.GetBindable<bool>(OsuSetting.AutoCursorSize);
                autoCursorScale.ValueChanged += v => calculateScale();

                calculateScale();
            }

            private void calculateScale()
            {
                float scale = (float)cursorScale.Value;

                if (autoCursorScale && beatmap.Value != null)
                {
                    // if we have a beatmap available, let's get its circle size to figure out an automatic cursor scale modifier.
                    scale *= (float)(1 - 0.7 * (1 + beatmap.Value.BeatmapInfo.BaseDifficulty.CircleSize - BeatmapDifficulty.DEFAULT_DIFFICULTY) / BeatmapDifficulty.DEFAULT_DIFFICULTY);
                }

                Scale = new Vector2(scale);
            }

            protected override void Update()
            {
                base.Update();

                if (VitaruPlayfield.VitaruPlayer != null)
                {
                    if (VitaruPlayfield.VitaruPlayer.Health > VitaruPlayfield.VitaruPlayer.MaxHealth)
                        health.Colour = Color4.Blue;
                    else if (VitaruPlayfield.VitaruPlayer.Health > VitaruPlayfield.VitaruPlayer.MaxHealth / 2)
                        health.Colour = Color4.Green;
                    else if (VitaruPlayfield.VitaruPlayer.Health > VitaruPlayfield.VitaruPlayer.MaxHealth / 4)
                        health.Colour = Color4.Yellow;
                    else
                        health.Colour = Color4.Red;

                    health.Text = ((int)VitaruPlayfield.VitaruPlayer.Health).ToString();
                    energy.Text = ((int)VitaruPlayfield.VitaruPlayer.Energy).ToString();
                    speed.Text = VitaruPlayfield.VitaruPlayer.SetRate.ToString();
                    combo.Text = VitaruPlayfield.VitaruPlayer.Combo.ToString();
                }
            }
        }

        public bool OnPressed(VitaruAction action)
        {
            return false;
        }

        public bool OnReleased(VitaruAction action)
        {
            return false;
        }
    }
}
