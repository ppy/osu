//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Graphics.Backgrounds;
using osu.Framework.MathUtils;
using System.Collections.Generic;
using osu.Framework.Graphics.Primitives;
using osu.Game.Modes;
using osu.Game.Graphics;

namespace osu.Game.Overlays.ModSelection
{
    public class ModSelection : OverlayContainer
    {
        private FlowContainer easymods, hardmods, assistmods;
        private SpriteText ranked, multiplier;
        private PlayMode lastPlayMode;

        private const float padded_width = 0.7f;
        private const float mod_section_height = 0.31f;

        private const int section_margin = -8;
        private const int section_padding = 20;
        private const int textsize_section_info = 16;
        private const int textsize_multiplier_info = 40;

        public ModSelection()
        {
            AddTemporaryMods();

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            Size = new Vector2(1.0f, 0.75f);
            Padding = new MarginPadding { Bottom = 50 };
            Children = new Drawable[]
            {
                // bg
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = new Color4(45, 45, 45, 255)
                        },
                        new GrayTriangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            TriangleScale = 8.0f,
                        },
                    }
                },
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FlowDirection.VerticalOnly,
                    Children = new Drawable[]
                    {
                        // upper container
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.2f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = new Color4(30, 30, 30, 128)
                                },
                                new FlowContainer
                                {
                                    // upper container
                                    RelativeSizeAxes = Axes.Both,
                                    Width = padded_width,
                                    Direction = FlowDirection.VerticalOnly,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,

                                    Children = new Drawable[]
                                    {
                                        new SpriteText
                                        {
                                            Text = @"Gameplay Modes",
                                            TextSize = 30,
                                            Padding = new MarginPadding { Top = 10, Bottom = 8 },
                                        },
                                        new SpriteText
                                        {
                                            Text = @"Mods provide different ways to enjoy gameplay. Some have an effect on the score you can achieve during ranked play.",
                                            TextSize = 20,
                                        },
                                        new SpriteText
                                        {
                                            Text = @"Others are just for fun",
                                            TextSize = 20,
                                        }
                                    }
                                }
                            }
                        },
                        // main container
                        new FlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Both,
                            Width = padded_width,
                            Height = 0.65f,
                            Direction = FlowDirection.VerticalOnly,
                            Children = new Drawable[]
                            {
                                new SpriteText
                                {
                                    Margin = new MarginPadding { Top = 15, Bottom = section_margin },
                                    Text = @"Gameplay Difficulty Reduction",
                                    TextSize = textsize_section_info,
                                    Font = @"Exo2.0-Bold",
                                },
                                easymods = new FlowContainer
                                {
                                    Margin = new MarginPadding { Top = section_margin },
                                    Padding = new MarginPadding { Left = section_padding },
                                    Direction = FlowDirection.HorizontalOnly,
                                    RelativeSizeAxes = Axes.Both,
                                    Height = mod_section_height
                                },
                                //--
                                new SpriteText
                                {
                                    Margin = new MarginPadding { Bottom = section_margin },
                                    Text = @"Gameplay Difficulty Increase",
                                    TextSize = textsize_section_info,
                                    Font = @"Exo2.0-Bold",
                                },
                                hardmods = new FlowContainer
                                {
                                    Margin = new MarginPadding { Top = section_margin },
                                    Padding = new MarginPadding { Left = section_padding },
                                    Direction = FlowDirection.HorizontalOnly,
                                    RelativeSizeAxes = Axes.Both,
                                    Height = mod_section_height
                                },
                                //--
                                new SpriteText
                                {
                                    Margin = new MarginPadding { Bottom = section_margin },
                                    Text = @"Assisted",
                                    TextSize = textsize_section_info,
                                    Font = @"Exo2.0-Bold",
                                },
                                assistmods = new FlowContainer
                                {
                                    Margin = new MarginPadding { Top = section_margin },
                                    Padding = new MarginPadding { Left = section_padding },
                                    Direction = FlowDirection.HorizontalOnly,
                                    RelativeSizeAxes = Axes.Both,
                                    Height = mod_section_height
                                }
                            }
                        },
                        // lower container
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.15f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = new Color4(200, 60, 120, 128),
                                },
                                new FlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = padded_width,
                                    Direction = FlowDirection.HorizontalOnly,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,

                                    Children = new Drawable[]
                                    {
                                        ranked = new SpriteText
                                        {
                                            Origin = Anchor.CentreLeft,
                                            Anchor = Anchor.CentreLeft,
                                            Text = @"Ranked",
                                            TextSize = textsize_multiplier_info,
                                        },
                                        new SpriteText
                                        {
                                            Origin = Anchor.CentreLeft,
                                            Anchor = Anchor.CentreLeft,
                                            Text = @", Score Multiplier: ",
                                            TextSize = textsize_multiplier_info,
                                        },
                                        multiplier = new SpriteText
                                        {
                                            Origin = Anchor.CentreLeft,
                                            Anchor = Anchor.CentreLeft,
                                            Text = @"1.00 ",
                                            TextSize = textsize_multiplier_info,
                                            Font = @"Exo2.0-Bold",
                                        },
                                        new TextAwesome
                                        {
                                            Origin = Anchor.CentreLeft,
                                            Anchor = Anchor.CentreLeft,
                                            Icon = FontAwesome.fa_close,
                                            //Text = @" X",
                                            TextSize = textsize_multiplier_info,
                                            //Font = @"Exo2.0-Bold",
                                        }
                                    }
                                },
                                // FIXME? probably remove after #202 merged
                                new Box
                                {
                                    Origin = Anchor.BottomLeft,
                                    Anchor = Anchor.BottomLeft,
                                    RelativeSizeAxes = Axes.X,
                                    Height = 5,
                                    Colour = new Color4(255, 102, 170, 255),
                                },
                            }
                        }
                    }
                }
            };
            RecreateButtons(PlayMode.Osu);
        }
        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Number1:
                    ClearMods();
                    return true;
                case Key.Escape:
                case Key.Number2:
                    if (State == Visibility.Hidden) return false;
                    Hide();
                    return true;
            }
            return base.OnKeyDown(state, args);
        }
        protected override void PopIn()
        {
            // FIXME: place actual playmode here
            if (lastPlayMode != PlayMode.Osu)
            {
                RecreateButtons(PlayMode.Osu);
                lastPlayMode = PlayMode.Osu;
            }

            MoveToY(0, 500, EasingTypes.OutQuint);
            FadeIn(500, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            MoveToY(DrawSize.Y, 500, EasingTypes.InSine);
            FadeOut(500, EasingTypes.InSine);
        }

        private void ActivateMod(Mod mod)
        {
            if (!ActiveTempMods.Contains(mod))
                ActiveTempMods.Add(mod);
            else
                ActiveTempMods.Remove(mod);

            double totalmultiplier = 1.0;
            bool isRanked = true;
            foreach (Mod m in ActiveTempMods)
            {
                if (m.ScoreMultiplier == 0.0)
                    isRanked = false;

                totalmultiplier *= m.ScoreMultiplier;
            }
            multiplier.Text = totalmultiplier.ToString() + " ";

            if (isRanked)
                ranked.Text = "Ranked";
            else
                ranked.Text = "Unranked";
        }

        private void ClearMods()
        {
            ActiveTempMods.Clear();

            foreach (ModButton button in easymods.Children)
                button.Disarm();

            foreach (ModButton button in hardmods.Children)
                button.Disarm();

            foreach (ModButton button in assistmods.Children)
                button.Disarm();

            multiplier.Text = "1.0 ";
            ranked.Text = "Ranked";
        }
        private void RecreateButtons(PlayMode playmode)
        {
            easymods.Clear();
            hardmods.Clear();
            assistmods.Clear();

            // FIXME: replace with proper mod list
            foreach (Mod mod in TempMods)
            {
                if (CreateMultimodButton(mod)) // i have no idea how to check if we need multimod button or not without just hardcoding mods inside
                    continue;

                bool active = ActiveTempMods.Contains(mod);

                if (mod.ScoreMultiplier < 1 && mod.ScoreMultiplier > 0)
                    easymods.Add(new ModButton(mod, new Color4(164, 204, 0, 255), active) { Action = () => ActivateMod(mod) });
                else if ((Mods.ScoreIncreaseMods & mod.Name) == mod.Name)
                    hardmods.Add(new ModButton(mod, new Color4(255, 204, 33, 255), active) { Action = () => ActivateMod(mod) });
                else if (mod.ScoreMultiplier == 0)
                    assistmods.Add(new ModButton(mod, new Color4(102, 204, 255, 255), active) { Action = () => ActivateMod(mod) });

            }
        }
        // --
        // [ TEMP: remove when proper mod system will be added
        private bool CreateMultimodButton(Mod mod)
        {
            if (mod.Name == Mods.SuddenDeath)
            {
                MultimodButton sd;
                hardmods.Add(sd = new MultimodButton(new Color4(255, 204, 33, 255))
                {
                    Action = () => ActivateMod(mod),
                });
                sd.AddMod(mod);
                sd.AddMod(TempMods.Find(m => m.Name == Mods.Perfect));

                return true;
            }
            else if (mod.Name == Mods.DoubleTime)
            {
                MultimodButton dt;
                hardmods.Add(dt = new MultimodButton(new Color4(255, 204, 33, 255))
                {
                    Action = () => ActivateMod(mod),
                });
                dt.AddMod(mod);
                dt.AddMod(TempMods.Find(m => m.Name == Mods.Nightcore));

                return true;
            }
            else if (mod.Name == Mods.Autoplay)
            {
                MultimodButton auto;
                assistmods.Add(auto = new MultimodButton(new Color4(102, 204, 255, 255))
                {
                    Action = () => ActivateMod(mod)
                });
                auto.AddMod(mod);
                auto.AddMod(TempMods.Find(m => m.Name == Mods.Cinema));

                return true;
            }
            else if (mod.Name == Mods.Perfect || mod.Name == Mods.Nightcore || mod.Name == Mods.Cinema)
                return true;

            return false;
        }

        private List<Mod> ActiveTempMods;
        private List<Mod> TempMods;
        private void AddTemporaryMods()
        {
            TempMods = new List<Mod>();
            ActiveTempMods = new List<Mod>();
            TempMods.Add(new Mod
            {
                Name = Mods.Easy,
                Icon = FontAwesome.fa_osu_mod_easy,
                ScoreMultiplier = 0.5
            });
            TempMods.Add(new Mod
            {
                Name = Mods.NoFail,
                Icon = FontAwesome.fa_osu_mod_nofail,
                ScoreMultiplier = 0.5
            });
            TempMods.Add(new Mod
            {
                Name = Mods.HalfTime,
                Icon = FontAwesome.fa_osu_mod_halftime,
                ScoreMultiplier = 0.3
            });
            TempMods.Add(new Mod
            {
                Name = Mods.HardRock,
                Icon = FontAwesome.fa_osu_mod_hardrock,
                ScoreMultiplier = 1.06 // FIXME
            });
            TempMods.Add(new Mod
            {
                Name = Mods.SuddenDeath,
                Icon = FontAwesome.fa_osu_mod_suddendeath,
                ScoreMultiplier = 1.0
            });
            TempMods.Add(new Mod
            {
                Name = Mods.Perfect,
                Icon = FontAwesome.fa_osu_mod_perfect,
                ScoreMultiplier = 1.0
            });
            TempMods.Add(new Mod
            {
                Name = Mods.DoubleTime,
                Icon = FontAwesome.fa_osu_mod_doubletime,
                ScoreMultiplier = 1.12 // FIXME
            });
            TempMods.Add(new Mod
            {
                Name = Mods.Nightcore,
                Icon = FontAwesome.fa_osu_mod_nightcore,
                ScoreMultiplier = 1.12 // FIXME
            });
            TempMods.Add(new Mod
            {
                Name = Mods.Hidden,
                Icon = FontAwesome.fa_osu_mod_hidden,
                ScoreMultiplier = 1.06 // FIXME
            });
            TempMods.Add(new Mod
            {
                Name = Mods.Flashlight,
                Icon = FontAwesome.fa_osu_mod_flashlight,
                ScoreMultiplier = 1.12 // FIXME
            });
            TempMods.Add(new Mod
            {
                Name = Mods.Relax,
                Icon = FontAwesome.fa_osu_mod_relax,
                ScoreMultiplier = 0
            });
            TempMods.Add(new Mod
            {
                Name = Mods.Relax2,
                Icon = FontAwesome.fa_osu_mod_autopilot,
                ScoreMultiplier = 0
            });
            TempMods.Add(new Mod
            {
                Name = Mods.Target,
                Icon = FontAwesome.fa_osu_mod_target,
                ScoreMultiplier = 0
            });
            TempMods.Add(new Mod
            {
                Name = Mods.SpunOut,
                Icon = FontAwesome.fa_osu_mod_spunout,
                ScoreMultiplier = 0
            });
            TempMods.Add(new Mod
            {
                Name = Mods.Autoplay,
                Icon = FontAwesome.fa_osu_mod_auto,
                ScoreMultiplier = 0
            });
            TempMods.Add(new Mod
            {
                Name = Mods.Cinema,
                Icon = FontAwesome.fa_osu_mod_cinema,
                ScoreMultiplier = 0
            });
        }
        // ] TEMP: remove when proper mod system will be added
        // --
    }
    //--
    public class GrayTriangles : Triangles
    {
        private static readonly List<Color4> colors = new List<Color4>()
        {
            new Color4( 49,49,49,255),
            new Color4( 55,55,55,255),
            new Color4( 59,59,59,255),
        };

        public GrayTriangles()
        {
            Alpha = 1.0f;
        }

        protected override Framework.Graphics.Sprites.Triangle CreateTriangle()
        {
            var scale = TriangleScale * 4 * RNG.NextSingle() * 0.4f + 0.2f;

            Framework.Graphics.Sprites.Triangle result = base.CreateTriangle();
            result.Colour = colors[RNG.Next(0, colors.Count)];
            result.Scale = new Vector2(scale);

            return result;
        }
    }
}
