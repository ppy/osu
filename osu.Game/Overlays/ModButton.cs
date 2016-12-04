//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Modes;
using osu.Framework.Extensions;

namespace osu.Game.Overlays
{
    class ModButton : ClickableContainer, IStateful<ModButtonState>
    {
        private TextAwesome icon, bg;
        private Color4 bgColor;

        public ModButtonState State{get; set;}
        private Mod Mod;

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
                bg = new TextAwesome
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    TextSize = 80,
                    Icon = FontAwesome.fa_osu_mod_bg,
                    Colour = bgColor,
                    Shadow = true,
                },
                icon = new TextAwesome
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    TextSize = 50,
                    Icon = Mod.Icon,
                    Colour = new Color4(84,84,84,255)
                },
                new SpriteText
                {
                    Margin = new MarginPadding { Top = 15 },
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    TextSize = 18,
                    Text = Mod.Name.GetDescription(),
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
