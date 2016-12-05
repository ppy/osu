//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Game.Modes;

namespace osu.Game.Overlays
{
    class ModButton : ClickableContainer, IStateful<ModButtonState>
    {
        private Mod Mod;
        private Color4 bgColor;

        public ModButtonState State{get; set;}

        private const int transform_time = 150;

        public ModButton(Mod mod, Color4 color, bool isActive)
        {
            Mod = mod;
            bgColor = color;

            State = ModButtonState.Disabled;

            RelativeSizeAxes = Axes.Y;
            Width = 100;

            Origin = Anchor.Centre;
            Anchor = Anchor.CentreLeft;

            Margin = new MarginPadding { Left = 40, Top = -15 };
            Children = new Drawable[]
            {
                new DrawableMod
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Mod = Mod,
                    Colour = bgColor
                }
            };

            if (isActive)
                Arm();
        }
        protected override bool OnClick(InputState state)
        {
            if (State == ModButtonState.Disabled)
                Arm();
            else
                Disarm();

            return base.OnClick(state);
        }
        private void Arm()
        {
            State = ModButtonState.Armed;
            RotateTo(10, transform_time, Framework.Graphics.Transformations.EasingTypes.Out);
            ScaleTo(1.1f, transform_time, Framework.Graphics.Transformations.EasingTypes.Out);
        }
        public void Disarm()
        {
            State = ModButtonState.Disabled;
            RotateTo(0, transform_time, Framework.Graphics.Transformations.EasingTypes.In);
            ScaleTo(1.0f, transform_time, Framework.Graphics.Transformations.EasingTypes.In);
        }
    }
    public enum ModButtonState
    {
        Disabled,
        Armed,
    }
}
