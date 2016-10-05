//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.GameModes;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Background;

namespace osu.Game.GameModes
{
    public class OsuGameMode : GameMode
    {
        internal BackgroundMode Background { get; private set; }

        /// <summary>
        /// Override to create a BackgroundMode for the current GameMode.
        /// Note that the instance created may not be the used instance if it matches the BackgroundMode equality clause.
        /// </summary>
        protected virtual BackgroundMode CreateBackground() => null;

        protected override void OnEntering(GameMode last)
        {
            OsuGameMode lastOsu = last as OsuGameMode;

            BackgroundMode bg = CreateBackground();

            if (lastOsu?.Background != null)
            {
                if (bg == null || lastOsu.Background.Equals(bg))
                    //we can keep the previous mode's background.
                    Background = lastOsu.Background;
                else
                {
                    lastOsu.Background.Push(Background = bg);
                }
            }
            else if (bg != null)
            {
                bg.Depth = float.MinValue;
                AddTopLevel(Background = bg);
            }


            base.OnEntering(last);
        }

        protected override void OnExiting(GameMode next)
        {
            OsuGameMode nextOsu = next as OsuGameMode;

            if (Background != null && !Background.Equals(nextOsu?.Background))
                Background.Exit();

            base.OnExiting(next);
        }

        protected override void OnResuming(GameMode last)
        {
            base.OnResuming(last);
        }

        protected override void OnSuspending(GameMode next)
        {
            base.OnSuspending(next);
        }

    }
}
