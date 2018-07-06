// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    public class ShakeContainer : Container
    {
        public void Shake()
        {
            const float shake_amount = 8;
            const float shake_duration = 30;

            this.MoveToX(shake_amount, shake_duration / 2, Easing.OutSine).Then()
                .MoveToX(-shake_amount, shake_duration, Easing.InOutSine).Then()
                .MoveToX(shake_amount, shake_duration, Easing.InOutSine).Then()
                .MoveToX(-shake_amount, shake_duration, Easing.InOutSine).Then()
                .MoveToX(shake_amount, shake_duration, Easing.InOutSine).Then()
                .MoveToX(0, shake_duration / 2, Easing.InSine);
        }
    }
}
