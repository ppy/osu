//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Game.Modes;
using osu.Game.Modes.UI;

namespace osu.Game.Overlays.ModSelection
{
    class MultimodButton : ClickableContainer, IStateful<ModButtonState>
    {
        private List<Mod> modList;
        private int currentMod = 0;
        private Color4 bgColor;

        public ModButtonState State { get; set; }

        private const int transform_time = 150;

        public MultimodButton( Color4 color )
        {
            modList = new List<Mod>();
            bgColor = color;

            State = ModButtonState.Disabled;

            RelativeSizeAxes = Axes.Y;
            Width = 100;

            Origin = Anchor.Centre;
            Anchor = Anchor.CentreLeft;

            Margin = new MarginPadding { Left = 40, Top = -15 };
        }
        // recreate whole button
        public void AddMod(Mod mod)
        {
            modList.Add(mod);

            Clear();

            for (int i = 1; i < modList.Count; i++)
            {
                Add(new ModIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Left = 10, Top = 10 },
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Icon = modList[i].Icon,
                    Colour = bgColor
                });
            }

            Add(new DrawableMod
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Mod = modList[0],
                Colour = bgColor
            });

        }
        protected override bool OnClick(InputState state)
        {
            if (State == ModButtonState.Disabled)
            {
                Arm();
            }
            else if (currentMod < modList.Count - 1)
            {
                currentMod++;
                ArmNext();
            }
            else
            {
                Disarm();
            }

            return base.OnClick(state);
        }
        private void Arm()
        {
            State = ModButtonState.Armed;
            RotateTo(10, transform_time, Framework.Graphics.Transformations.EasingTypes.Out);
            ScaleTo(1.1f, transform_time, Framework.Graphics.Transformations.EasingTypes.Out);
        }
        private void ArmNext()
        {
            Clear();

            if (currentMod < modList.Count - 1)
            {
                Add(new ModIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Left = 10, Top = 10 },
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Icon = modList[currentMod + 1].Icon,
                    Colour = bgColor
                });
            }
            Add(new DrawableMod
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Mod = modList[currentMod],
                Colour = bgColor
            });

        }
        public void Disarm()
        {
            State = ModButtonState.Disabled;
            RotateTo(0, transform_time, Framework.Graphics.Transformations.EasingTypes.In);
            ScaleTo(1.0f, transform_time, Framework.Graphics.Transformations.EasingTypes.In);

            currentMod = 0;
            ArmNext();
        }
    }
}
