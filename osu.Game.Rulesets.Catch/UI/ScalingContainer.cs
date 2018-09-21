// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// A <see cref="Container"/> which scales its content relative to a target width.
    /// </summary>
    public class ScalingContainer : Container
    {
        private readonly float targetWidth;

        public ScalingContainer(float targetWidth)
        {
            this.targetWidth = targetWidth;
        }

        protected override void Update()
        {
            base.Update();

            Scale = new Vector2(Parent.ChildSize.X / targetWidth);
            Size = Vector2.Divide(Vector2.One, Scale);
        }
    }
}
