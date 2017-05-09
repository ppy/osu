// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System;
using System.Linq;

namespace osu.Game.Overlays.Mods
{

    /// <summary>
    /// Represents a clickable button which can cycle through one of more mods.
    /// </summary>
    public class ModButton : ModButtonEmpty
    {
        private ModIcon foregroundIcon;
        private readonly SpriteText text;
        private readonly Container<ModIcon> iconsContainer;
        private SampleChannel sampleOn, sampleOff;

        public Action<Mod> Action; // Passed the selected mod or null if none

        private int _selectedIndex = -1;
        private int selectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (value == _selectedIndex) return;
                _selectedIndex = value;

                if (value >= Mods.Length)
                {
                    _selectedIndex = -1;
                }
                else if (value <= -2)
                {
                    _selectedIndex = Mods.Length - 1;
                }

                iconsContainer.RotateTo(Selected ? 5f : 0f, 300, EasingTypes.OutElastic);
                iconsContainer.ScaleTo(Selected ? 1.1f : 1f, 300, EasingTypes.OutElastic);
                foregroundIcon.Highlighted = Selected;

                if (mod != null)
                    displayMod(SelectedMod ?? Mods[0]);
            }
        }

        public bool Selected => selectedIndex != -1;


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
                if (Selected) foregroundIcon.Colour = value;
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
                mod = value;

                if (mod == null)
                {
                    Mods = new Mod[0];
                    Alpha = 0;
                }
                else
                {
                    Mods = (mod as MultiMod)?.Mods ?? new[] { mod };
                    Alpha = 1;
                }

                createIcons();
                if (Mods.Length > 0)
                {
                    displayMod(Mods[0]);
                }
            }
        }

        public Mod[] Mods { get; private set; }

        // the mods from Mod, only multiple if Mod is a MultiMod

        public override Mod SelectedMod => Mods.ElementAtOrDefault(selectedIndex);

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleOn = audio.Sample.Get(@"Checkbox/check-on");
            sampleOff = audio.Sample.Get(@"Checkbox/check-off");
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            switch (args.Button)
            {
                case MouseButton.Left:
                    SelectNext();
                    break;
                case MouseButton.Right:
                    SelectPrevious();
                    break;
            }
            return true;
        }

        public void SelectNext()
        {
            (++selectedIndex == -1 ? sampleOff : sampleOn).Play();
            Action?.Invoke(SelectedMod);
        }

        public void SelectPrevious()
        {
            (--selectedIndex == -1 ? sampleOff : sampleOn).Play();
            Action?.Invoke(SelectedMod);
        }

        public void Deselect()
        {
            selectedIndex = -1;
        }

        private void displayMod(Mod mod)
        {
            foregroundIcon.Icon = mod.Icon;
            text.Text = mod.Name;
        }

        private void createIcons()
        {
            iconsContainer.Clear();
            if (Mods.Length > 1)
            {
                iconsContainer.Add(new[]
                {
                    new ModIcon(Mods[0])
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Position = new Vector2(1.5f),
                    },
                    foregroundIcon = new ModIcon(Mods[0])
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
                iconsContainer.Add(foregroundIcon = new ModIcon(Mod)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                });
            }
        }

        public ModButton(Mod mod)
        {
            Children = new Drawable[]
            {
                new Container
                {
                    Size = new Vector2(77f, 80f),
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        iconsContainer = new Container<ModIcon>
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        }
                    }
                },
                text = new OsuSpriteText
                {
                    Y = 75,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    TextSize = 18,
                },
            };

            Mod = mod;
        }
    }
}
