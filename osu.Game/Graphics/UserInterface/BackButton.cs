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

        public Vector2 ExtendLength = new Vector2(60, 0);
        public Vector2 InitialExtendLength = new Vector2(40, 0);

        private Color4 colorBright = new Color4(238, 51, 153, 255);
        private Color4 colorDark = new Color4(195, 40, 140, 255);
        private const double transform_time = 300.0;
        private const int pulse_length = 250;

        public BackButton()
        {
            Width = 80;
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
                            Colour = colorDark,
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
                        new Box
                        {
                            Colour = colorBright,
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

            rightContainer.MoveTo(Position + ExtendLength, transform_time, EasingTypes.OutElastic);
            leftContainer.ResizeTo(new Vector2(ExtendLength.X, 1.0f), transform_time, EasingTypes.OutElastic);

            int duration = 0; //(int)(Game.Audio.BeatLength / 2);
            if (duration == 0) duration = pulse_length;

            double offset = 0; //(1 - Game.Audio.SyncBeatProgress) * duration;
            double startTime = Time.Current + offset;

            // basic pulse
            icon.Transforms.Add(new TransformScale
            {
                StartValue = new Vector2(1.1f),
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

            rightContainer.MoveTo(Position + InitialExtendLength, transform_time, EasingTypes.OutElastic);
            leftContainer.ResizeTo(new Vector2(InitialExtendLength.X, 1.0f), transform_time, EasingTypes.OutElastic);

            int duration = 0; //(int)(Game.Audio.BeatLength / 2);
            if (duration == 0) duration = pulse_length * 2;

            double offset = 0; //(1 - Game.Audio.SyncBeatProgress) * duration;
            double startTime = Time.Current + offset;

            // slow pulse
            icon.Transforms.Add(new TransformScale
            {
                StartValue = new Vector2(1.1f),
                EndValue = Vector2.One,
                StartTime = startTime,
                EndTime = startTime + duration,
                Easing = EasingTypes.Out,
                LoopCount = -1,
                LoopDelay = duration
            });
        }

        protected override bool OnClick(InputState state)
        {
            var flash = new Box
            {
                RelativeSizeAxes = RelativeSizeAxes,
                Colour = colorBright,
                BlendingMode = BlendingMode.Additive,
                Alpha = 0.3f
            };
            Add(flash);

            flash.FadeOutFromOne(200);
            flash.Expire();

            return base.OnClick(state);
        }
    }
}
