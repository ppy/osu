//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Transformations;
using osu.Game.Graphics.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.GameModes.Play.Taiko
{
    public class TaikoComboCounter : ComboCounter
    {
        protected virtual int AnimationDuration => 300;
        protected virtual float ScaleFactor => 2;
        protected virtual EasingTypes AnimationEasing => EasingTypes.None;
        protected virtual bool CanAnimateWhenBackwards => false;

        public TaikoComboCounter()
        {
            CountSpriteText.Origin = Framework.Graphics.Anchor.BottomCentre;
            CountSpriteText.Anchor = Framework.Graphics.Anchor.BottomCentre;
        }

        protected virtual void transformAnimate(ulong newValue)
        {
            CountSpriteText.Text = FormatCount(newValue);
            CountSpriteText.ScaleTo(new Vector2(1, ScaleFactor));
            CountSpriteText.ScaleTo(new Vector2(1, 1), AnimationDuration, AnimationEasing);
        }

        protected virtual void transformNotAnimate(ulong newValue)
        {
            CountSpriteText.Text = FormatCount(newValue);
            CountSpriteText.ScaleTo(1);
        }

        protected override void OnCountRolling(ulong currentValue, ulong newValue)
        {
            if (newValue == 0)
                CountSpriteText.FadeOut(AnimationDuration);
            else
                CountSpriteText.Show();

            transformNotAnimate(newValue);
        }

        protected override void OnCountChange(ulong newValue)
        {
            CountSpriteText.FadeTo(newValue == 0 ? 0 : 1);

            transformNotAnimate(newValue);
        }

        protected override void OnCountIncrement(ulong newValue)
        {
            CountSpriteText.Show();

            transformAnimate(newValue);
        }
    }
}
