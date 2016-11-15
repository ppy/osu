//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Transformations;
using osu.Game.Modes.UI;
using OpenTK;

namespace osu.Game.Modes.Taiko.UI
{
    /// <summary>
    /// Allows tint and scaling animations. Used in osu!taiko.
    /// </summary>
    public class TaikoComboCounter : ComboCounter
    {
        protected virtual int AnimationDuration => 300;
        protected virtual float ScaleFactor => 2;
        protected virtual EasingTypes AnimationEasing => EasingTypes.None;
        protected virtual bool CanAnimateWhenBackwards => false;

        public TaikoComboCounter()
        {
            DisplayedCountSpriteText.Origin = Framework.Graphics.Anchor.BottomCentre;
            DisplayedCountSpriteText.Anchor = Framework.Graphics.Anchor.BottomCentre;
        }

        protected virtual void TransformAnimate(ulong newValue)
        {
            DisplayedCountSpriteText.Text = FormatCount(newValue);
            DisplayedCountSpriteText.ScaleTo(new Vector2(1, ScaleFactor));
            DisplayedCountSpriteText.ScaleTo(new Vector2(1, 1), AnimationDuration, AnimationEasing);
        }

        protected virtual void TransformNotAnimate(ulong newValue)
        {
            DisplayedCountSpriteText.Text = FormatCount(newValue);
            DisplayedCountSpriteText.ScaleTo(1);
        }

        protected override void OnDisplayedCountRolling(ulong currentValue, ulong newValue)
        {
            if (newValue == 0)
                DisplayedCountSpriteText.FadeOut(FadeOutDuration);
            else
                DisplayedCountSpriteText.Show();

            TransformNotAnimate(newValue);
        }

        protected override void OnDisplayedCountChange(ulong newValue)
        {
            DisplayedCountSpriteText.FadeTo(newValue == 0 ? 0 : 1);

            TransformNotAnimate(newValue);
        }

        protected override void OnDisplayedCountIncrement(ulong newValue)
        {
            DisplayedCountSpriteText.Show();

            TransformAnimate(newValue);
        }
    }
}
