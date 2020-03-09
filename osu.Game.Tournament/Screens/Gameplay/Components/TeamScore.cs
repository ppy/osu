// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public class TeamScore : CompositeDrawable
    {
        private readonly Bindable<int?> currentTeamScore = new Bindable<int?>();
        private readonly StarCounter counter;

        public TeamScore(Bindable<int?> score, bool flip, int count)
        {
            var anchor = flip ? Anchor.CentreRight : Anchor.CentreLeft;

            Anchor = anchor;
            Origin = anchor;

            InternalChild = counter = new StarCounter(count)
            {
                Anchor = anchor,
                X = (flip ? -1 : 1) * 90,
                Y = 5,
                Scale = flip ? new Vector2(-1, 1) : Vector2.One,
            };

            currentTeamScore.BindValueChanged(scoreChanged);
            currentTeamScore.BindTo(score);
        }

        private void scoreChanged(ValueChangedEvent<int?> score) => counter.Current = score.NewValue ?? 0;
    }
}
