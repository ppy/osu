// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using osu.Game.Modes;
using osu.Game.Modes.UI;

namespace osu.Game.Overlays.Mods
{
    public class ModButton : FillFlowContainer
    {
        private ModIcon[] icons;
        private ModIcon displayIcon => icons[icons.Length - 1];
        private SpriteText text;
        private Container iconsContainer;
        private SampleChannel sampleOn, sampleOff;

        public Action<Mod> Action; // Passed the selected mod or null if none

        private int _selectedMod = -1;
        private int selectedMod
        {
            get
            {
                return _selectedMod;
            }
            set
            {
                if (value == _selectedMod) return;
                _selectedMod = value;

                if (value >= Mods.Length)
                {
                    _selectedMod = -1;
                }
                else if (value <= -2)
                {
                    _selectedMod = Mods.Length - 1;
                }

                iconsContainer.RotateTo(Selected ? 5f : 0f, 300, EasingTypes.OutElastic);
                iconsContainer.ScaleTo(Selected ? 1.1f : 1f, 300, EasingTypes.OutElastic);
                for (int i = 0; i < icons.Length; i++)
                {
                    if (Selected && i == icons.Length - 1) icons[i].Colour = SelectedColour;
                    else icons[i].Colour = Colour;
                }

                displaySelectedMod();
            }
        }

        public bool Selected => selectedMod != -1;

        private Color4 backgroundColour;
        public new Color4 Colour
        {
            get
            {
                return backgroundColour;
            }
            set
            {
                if (value == backgroundColour) return;
                backgroundColour = value;
                foreach (ModIcon icon in icons)
                {
                    icon.Colour = value;
                }
            }
        }

        private Color4 selectedColour;
        public Color4 SelectedColour
        {
            get
            {
                return selectedColour;
            }
            set
            {
                if (value == selectedColour) return;
                selectedColour = value;
                if (Selected) icons[0].Colour = value;
            }
        }

        private Mod mod;
        public Mod Mod
        {
            get
            {
                return mod;
            }
            set
            {
                if (mod == value) return;
                mod = value;

                if (mod is MultiMod)
                {
                    mods = ((MultiMod)mod).Mods;
                }
                else
                {
                    mods = new Mod[] { mod };
                }

                createIcons();
                if (mods.Length > 0)
                {
                    displayMod(mods[0]);
                }
            }
        }

        private Mod[] mods;
        public Mod[] Mods => mods; // the mods from Mod, only multiple if Mod is a MultiMod

        public Mod SelectedMod => Mods.ElementAtOrDefault(selectedMod);

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleOn = audio.Sample.Get(@"Checkbox/check-on");
            sampleOff = audio.Sample.Get(@"Checkbox/check-off");
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            (args.Button == MouseButton.Right ? (Action)SelectPrevious : SelectNext)();
            return true;
        }

        public void SelectNext()
        {
            selectedMod++;
            if (selectedMod == -1)
            {
                sampleOff.Play();
            }
            else
            {
                sampleOn.Play();
            }

            Action?.Invoke(SelectedMod);
        }

        public void SelectPrevious()
        {
            selectedMod--;
            if (selectedMod == -1)
            {
                sampleOff.Play();
            }
            else
            {
                sampleOn.Play();
            }

            Action?.Invoke(SelectedMod);
        }

        public void Deselect()
        {
            selectedMod = -1;
        }

        private void displayMod(Mod mod)
        {
            displayIcon.Icon = mod.Icon;
            text.Text = mod.Name.GetDescription();
        }

        private void displaySelectedMod()
        {
            var modIndex = selectedMod;
            if (modIndex <= -1)
            {
                modIndex = 0;
            }

            displayMod(Mods[modIndex]);
        }

        private void createIcons()
        {
            if (Mods.Length > 1)
            {
                iconsContainer.Add(icons = new ModIcon[]
                {
                    new ModIcon
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Position = new Vector2(1.5f),
                    },
                    new ModIcon
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,  
                        Position = new Vector2(-1.5f),
                    },
                });
            }
            else
            {
                iconsContainer.Add(icons = new ModIcon[]
                {
                    new ModIcon
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                    },
                });
            }
        }

        public ModButton(Mod m)
        {
            Direction = FillDirection.Down;
            Spacing = new Vector2(0f, -5f);
            Size = new Vector2(100f);

            Children = new Drawable[]
            {
                new Container
                {
                    Size = new Vector2(77f, 80f),
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        iconsContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        }
                    }
                },
                text = new OsuSpriteText
                {
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    TextSize = 18,
                },
            };

            Mod = m;
        }
    }
}
