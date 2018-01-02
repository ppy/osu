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
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// Represents a clickable button which can cycle through one of more mods.
    /// </summary>
    public class ModButton : ModButtonEmpty, IHasTooltip
    {
        private ModIcon foregroundIcon;
        private ModIcon backgroundIcon;
        private readonly SpriteText text;
        private readonly Container<ModIcon> iconsContainer;
        private SampleChannel sampleOn, sampleOff;

        /// <summary>
        /// Fired when the selection changes.
        /// </summary>
        public Action<Mod> SelectionChanged;

        public string TooltipText => (SelectedMod?.Description ?? Mods.FirstOrDefault()?.Description) ?? string.Empty;

        private const Easing mod_switch_easing = Easing.InOutSine;
        private const double mod_switch_duration = 120;

        // A selected index of -1 means not selected.
        private int selectedIndex = -1;

        /// <summary>
        /// Change the selected mod index of this button.
        /// </summary>
        /// <param name="value">The new index</param>
        /// <returns>Whether the selection changed.</returns>
        private bool changeSelectedIndex(int value)
        {
            if (value == selectedIndex) return false;

            int direction = value < selectedIndex ? -1 : 1;
            bool beforeSelected = Selected;

            Mod modBefore = SelectedMod ?? Mods[0];

            if (value >= Mods.Length)
                selectedIndex = -1;
            else if (value < -1)
                selectedIndex = Mods.Length - 1;
            else
                selectedIndex = value;

            Mod modAfter = SelectedMod ?? Mods[0];

            if (!modAfter.HasImplementation)
            {
                if (modAfter != modBefore)
                    return changeSelectedIndex(selectedIndex + direction);
                return false;
            }

            if (beforeSelected != Selected)
            {
                iconsContainer.RotateTo(Selected ? 5f : 0f, 300, Easing.OutElastic);
                iconsContainer.ScaleTo(Selected ? 1.1f : 1f, 300, Easing.OutElastic);
            }

            if (modBefore != modAfter)
            {
                const float rotate_angle = 16;

                foregroundIcon.RotateTo(rotate_angle * direction, mod_switch_duration, mod_switch_easing);
                backgroundIcon.RotateTo(-rotate_angle * direction, mod_switch_duration, mod_switch_easing);

                backgroundIcon.Icon = modAfter.Icon;
                using (BeginDelayedSequence(mod_switch_duration, true))
                {
                    foregroundIcon
                        .RotateTo(-rotate_angle * direction)
                        .RotateTo(0f, mod_switch_duration, mod_switch_easing);

                    backgroundIcon
                        .RotateTo(rotate_angle * direction)
                        .RotateTo(0f, mod_switch_duration, mod_switch_easing);

                    Schedule(() => displayMod(modAfter));
                }
            }

            foregroundIcon.Highlighted = Selected;

            (selectedIndex == -1 ? sampleOff : sampleOn).Play();
            SelectionChanged?.Invoke(SelectedMod);
            return true;
        }

        public bool Selected => selectedIndex != -1;

        private Color4 selectedColour;

        public Color4 SelectedColour
        {
            get { return selectedColour; }
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
            get { return mod; }
            set
            {
                mod = value;

                if (mod == null)
                {
                    Mods = Array.Empty<Mod>();
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

        public virtual Mod SelectedMod => Mods.ElementAtOrDefault(selectedIndex);

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleOn = audio.Sample.Get(@"UI/check-on");
            sampleOff = audio.Sample.Get(@"UI/check-off");
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

        public void SelectNext() => changeSelectedIndex(selectedIndex + 1);

        public void SelectPrevious() => changeSelectedIndex(selectedIndex - 1);

        public void Deselect() => changeSelectedIndex(-1);

        private void displayMod(Mod mod)
        {
            if (backgroundIcon != null)
                backgroundIcon.Icon = foregroundIcon.Icon;
            foregroundIcon.Icon = mod.Icon;
            text.Text = mod.Name;
            Colour = mod.HasImplementation ? Color4.White : Color4.Gray;
        }

        private void createIcons()
        {
            iconsContainer.Clear();
            if (Mods.Length > 1)
            {
                iconsContainer.AddRange(new[]
                {
                    backgroundIcon = new PassThroughTooltipModIcon(Mods[1])
                    {
                        Origin = Anchor.BottomRight,
                        Anchor = Anchor.BottomRight,
                        Position = new Vector2(1.5f),
                    },
                    foregroundIcon = new PassThroughTooltipModIcon(Mods[0])
                    {
                        Origin = Anchor.BottomRight,
                        Anchor = Anchor.BottomRight,
                        Position = new Vector2(-1.5f),
                    },
                });
            }
            else
            {
                iconsContainer.Add(foregroundIcon = new PassThroughTooltipModIcon(Mod)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
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
                new HoverClickSounds()
            };

            Mod = mod;
        }

        private class PassThroughTooltipModIcon : ModIcon
        {
            public override string TooltipText => null;

            public PassThroughTooltipModIcon(Mod mod)
                : base(mod)
            {
            }
        }
    }
}
