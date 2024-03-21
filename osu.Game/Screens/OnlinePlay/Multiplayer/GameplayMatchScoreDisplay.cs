// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class GameplayMatchScoreDisplay : MatchScoreDisplay
    {
        public Bindable<bool> Expanded = new Bindable<bool>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Scale = new Vector2(0.5f);

            Expanded.BindValueChanged(expandedChanged, true);
        }

        private void expandedChanged(ValueChangedEvent<bool> expanded)
        {
            if (expanded.NewValue)
            {
                Score1Text.FadeIn(500, Easing.OutQuint);
                Score2Text.FadeIn(500, Easing.OutQuint);
                this.ResizeWidthTo(2, 500, Easing.OutQuint);
            }
            else
            {
                Score1Text.FadeOut(500, Easing.OutQuint);
                Score2Text.FadeOut(500, Easing.OutQuint);
                this.ResizeWidthTo(1, 500, Easing.OutQuint);
            }
        }
    }
}
