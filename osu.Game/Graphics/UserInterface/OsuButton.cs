// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuButton : Button
    {
        private Box hover;

        private SampleChannel sampleClick;
        private SampleChannel sampleHover;

        public OsuButton()
        {
            Height = 40;
        }

        protected override SpriteText CreateText() => new OsuSpriteText
        {
            Depth = -1,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Font = @"Exo2.0-Bold",
        };

        public override bool HandleInput => Action != null;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, AudioManager audio)
        {
            if (Action == null)
                Colour = OsuColour.Gray(0.5f);

            BackgroundColour = colours.BlueDark;

            Content.Masking = true;
            Content.CornerRadius = 5;

            Add(new Drawable[]
            {
                new Triangles
                {
                    RelativeSizeAxes = Axes.Both,
                    ColourDark = colours.BlueDarker,
                    ColourLight = colours.Blue,
                },
                hover = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    BlendingMode = BlendingMode.Additive,
                    Colour = Color4.White.Opacity(0.1f),
                    Alpha = 0,
                },
            });

            sampleClick = audio.Sample.Get(@"UI/generic-click");
            sampleHover = audio.Sample.Get(@"UI/generic-hover");
        }

        protected override bool OnClick(InputState state)
        {
            sampleClick?.Play();
            return base.OnClick(state);
        }

        protected override bool OnHover(InputState state)
        {
            sampleHover?.Play();
            hover.FadeIn(200);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            hover.FadeOut(200);
            base.OnHoverLost(state);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            Content.ScaleTo(0.9f, 4000, EasingTypes.OutQuint);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            Content.ScaleTo(1, 1000, EasingTypes.OutElastic);
            return base.OnMouseUp(state, args);
        }
    }
}