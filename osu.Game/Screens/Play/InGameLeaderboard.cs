// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Users;

namespace osu.Game.Screens.Play
{
    public class InGameLeaderboard : CompositeDrawable
    {
        protected readonly InGameScoreContainer ScoresContainer;

        public readonly BindableDouble PlayerCurrentScore = new BindableDouble();

        private bool playerItemCreated;
        private User playerUser;

        public User PlayerUser
        {
            get => playerUser;
            set
            {
                playerUser = value;

                if (playerItemCreated)
                    return;

                ScoresContainer.AddRealTimePlayer(PlayerCurrentScore, playerUser);
                playerItemCreated = true;
            }
        }

        public InGameLeaderboard()
        {
            AutoSizeAxes = Axes.Y;

            InternalChild = ScoresContainer = new InGameScoreContainer();
        }
    }
}
