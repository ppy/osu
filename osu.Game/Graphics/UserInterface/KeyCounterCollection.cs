//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.UserInterface
{
    public class KeyCounterCollection : FlowContainer<KeyCounter>
    {
        public KeyCounterCollection()
        {
            Direction = FlowDirection.HorizontalOnly;
            AutoSizeAxes = Axes.Both;
        }

        public override void Add(KeyCounter key)
        {
            base.Add(key);
            key.IsCounting = IsCounting;
            key.FadeTime = FadeTime;
            key.KeyDownTextColor = KeyDownTextColor;
            key.KeyUpTextColor = KeyUpTextColor;
        }

        public void ResetCount()
        {
            foreach (var counter in Children)
                counter.ResetCount();
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        //further: change default values here and in KeyCounter if needed, instead of passing them in every constructor
        private bool isCounting;
        public bool IsCounting
        {
            get { return isCounting; }
            set
            {
                if (value != isCounting)
                {
                    isCounting = value;
                    foreach (var child in Children)
                        child.IsCounting = value;
                }
            }
        }

        private int fadeTime = 0;
        public int FadeTime
        {
            get { return fadeTime; }
            set
            {
                if (value != fadeTime)
                {
                    fadeTime = value;
                    foreach (var child in Children)
                        child.FadeTime = value;
                }
            }
        }

        private Color4 keyDownTextColor = Color4.DarkGray;
        public Color4 KeyDownTextColor
        {
            get { return keyDownTextColor; }
            set
            {
                if (value != keyDownTextColor)
                {
                    keyDownTextColor = value;
                    foreach (var child in Children)
                        child.KeyDownTextColor = value;
                }
            }
        }

        private Color4 keyUpTextColor = Color4.White;
        public Color4 KeyUpTextColor
        {
            get { return keyUpTextColor; }
            set
            {
                if (value != keyUpTextColor)
                {
                    keyUpTextColor = value;
                    foreach (var child in Children)
                        child.KeyUpTextColor = value;
                }
            }
        }
    }
}
