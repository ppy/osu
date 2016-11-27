// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterface
{
    // Basic back button as it was on stable (kinda). No skinning possible for now
    class BackButton : ExtendableButton
    {
        private TextAwesome icon;
        private Vector2 iconPos = new Vector2(20, 0);

        public BackButton()
        {
            InitialExtendLenght = new Vector2(40, 0);
            ExtendLenght = new Vector2(60, 0);

            RelativeSizeAxes = Axes.Y;
            Width = 80;
            //Height = 40; // should be set or should be relative?

            Text = @"Back";

            BGColour = new Color4(195, 40, 140, 255);
            Colour = new Color4(238, 51, 153, 255);

            Children = new Drawable[]
            {
                icon = new TextAwesome
                {
                    Anchor = Anchor.CentreLeft,
                    TextSize = 25,
                    Position = iconPos,
                    Icon = FontAwesome.fa_osu_left_o
                }
            };

            // HACK: because it never uses InitialExtendLenght that we give to it on creation
            textContainer.Position = Position + InitialExtendLenght; 
        }
        protected override bool OnHover(InputState state)
        {
            bool result = base.OnHover(state);

            icon.ClearTransformations();

            icon.MoveToX(iconPos.X + 10, 150, EasingTypes.OutElastic);

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

            return result;
        }

        protected override void OnHoverLost(InputState state)
        {
            icon.ClearTransformations();
            icon.MoveToX(iconPos.X, 150, EasingTypes.OutElastic);

            base.OnHoverLost(state);
        }
    }
}
