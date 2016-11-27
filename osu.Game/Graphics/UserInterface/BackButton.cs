// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterface
{
    // Basic back button as it was on stable (kinda). No skinning possible for now
    class BackButton : ClickableContainer
    {
        private TextAwesome icon;

        private Container leftContainer;
        private Container rightContainer;
        private Box textBox;

        public Vector2 ExtendLength;
        public Vector2 InitialExtendLength;

        private const double transformTime = 300.0;

        public BackButton()
        {
            // [ should be set or should be relative?
            InitialExtendLength = new Vector2(40, 0);
            ExtendLength = new Vector2(60, 0);

            Width = 80;
            //Height = 40;
            // ] should be set or should be relative?

            Children = new Drawable[]
            {
                leftContainer = new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = InitialExtendLength.X,
                    Children = new Drawable[] 
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = new Color4(195, 40, 140, 255),
                            Shear = new Vector2(0.1f, 0),
                        },
                        icon = new TextAwesome
                        {
                            Anchor = Anchor.Centre,
                            TextSize = 25,
                            Icon = FontAwesome.fa_osu_left_o
                        },
                    }
                },
                rightContainer = new Container
                {
                    Origin = Anchor.TopLeft,
                    Anchor = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.Both,
                    Position = Position + InitialExtendLength,
                    Children = new Drawable[]
                    {
                        textBox = new Box
                        {
                            Colour = new Color4(238, 51, 153, 255),
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.Both,
                            Shear = new Vector2(0.1f, 0), 
                            EdgeSmoothness = new Vector2(1.5f, 0), 
                        },
                        new SpriteText
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Text = @"Back",
                        }
                    }
                }
            };
        }
        protected override bool OnHover(InputState state)
        {
            icon.ClearTransformations();
            rightContainer.ClearTransformations();
            leftContainer.ClearTransformations();

            rightContainer.MoveTo(Position + ExtendLength, transformTime, EasingTypes.OutElastic);
            leftContainer.ResizeTo(new Vector2(ExtendLength.X, 1.0f), transformTime, EasingTypes.OutElastic);

            int duration = 0; //(int)(Game.Audio.BeatLength / 2);
            if (duration == 0) duration = 250;

            double offset = 0; //(1 - Game.Audio.SyncBeatProgress) * duration;
            double startTime = Time.Current + offset;

            // basic pulse
            icon.Transforms.Add(new TransformScale
            {
                StartValue = new Vector2(1.1f, 1.1f),
                EndValue = Vector2.One,
                StartTime = startTime,
                EndTime = startTime + duration,
                Easing = EasingTypes.Out,
                LoopCount = -1,
                LoopDelay = duration
            });

            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            icon.ClearTransformations();
            rightContainer.ClearTransformations();
            leftContainer.ClearTransformations();

            rightContainer.MoveTo(Position + InitialExtendLength, transformTime, EasingTypes.OutElastic);
            leftContainer.ResizeTo(new Vector2(InitialExtendLength.X, 1.0f), transformTime, EasingTypes.OutElastic);
        }

        protected override bool OnClick(InputState state)
        {
            var flash = new Box
            {
                RelativeSizeAxes = RelativeSizeAxes
            };

            Add(flash);

            flash.Colour = textBox.Colour;
            flash.BlendingMode = BlendingMode.Additive;
            flash.Alpha = 0.3f;
            flash.FadeOutFromOne(200);
            flash.Expire();

            return base.OnClick(state);
        }
    }
}
