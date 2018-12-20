// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Android.Content;
using Android.Util;
using osu.Framework.Android;
using osu.Game;

namespace osu.Android
{
    public class GameView : AndroidGameView
    {
        public GameView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            CreateGame();
        }

        public GameView(Context context) : base(context)
        {
            CreateGame();
        }
        public override Framework.Game CreateGame() => new OsuGame();
    }
}
