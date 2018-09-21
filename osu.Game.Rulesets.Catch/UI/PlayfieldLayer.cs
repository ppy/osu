// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class PlayfieldLayer : Container
    {
        protected override void Update()
        {
            base.Update();

            Scale = new Vector2(Parent.ChildSize.X / CatchPlayfield.BASE_WIDTH);
            Size = Vector2.Divide(Vector2.One, Scale);
        }
    }
}
