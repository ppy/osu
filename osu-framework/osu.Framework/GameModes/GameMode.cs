//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics.Containers;

namespace osu.Framework.GameModes
{
    public class GameMode : LargeContainer
    {
        public virtual string Name => @"Unknown";

        private GameMode lastGameMode;

        private bool modePushed;

        /// <summary>
        /// Called when this GameMode is being entered.
        /// </summary>
        /// <param name="last">The last GameMode.</param>
        protected virtual void EnterTransition(GameMode last)
        {
            FadeInFromZero(200);
        }

        /// <summary>
        /// Called when this GameMode is exiting.
        /// </summary>
        /// <param name="next">The next GameMode.</param>
        protected virtual void ExitTransition(GameMode next)
        {
            FadeOutFromOne(200);
        }

        /// <summary>
        /// Changes to a new GameMode.
        /// </summary>
        /// <param name="mode">The new GameMode.</param>
        protected void PushMode(GameMode mode)
        {
            if (modePushed)
                return;
            modePushed = true;

            AddTopLevel(mode);

            mode.lastGameMode = this;
            mode.EnterTransition(this);
        }

        /// <summary>
        /// Exits this GameMode.
        /// </summary>
        protected void ExitMode()
        {
            lastGameMode.modePushed = false;

            ExitTransition(lastGameMode);
            Delay(5000).Expire();
        }
    }
}
