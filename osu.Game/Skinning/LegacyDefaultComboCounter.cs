// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Threading;
using osuTK;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Uses the 'x' symbol and has a pop-out effect while rolling over.
    /// </summary>
    public partial class LegacyDefaultComboCounter : LegacyComboCounter
    {
        private const double big_pop_out_duration = 300;
        private const double small_pop_out_duration = 100;

        private ScheduledDelegate? scheduledPopOut;

        public LegacyDefaultComboCounter()
        {
            Margin = new MarginPadding(10);

            PopOutCountText.Anchor = Anchor.BottomLeft;
            DisplayedCountText.Anchor = Anchor.BottomLeft;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            const float font_height_ratio = 0.625f;
            const float vertical_offset = 9;

            DisplayedCountText.OriginPosition = new Vector2(0, font_height_ratio * DisplayedCountText.Height + vertical_offset);
            DisplayedCountText.Position = new Vector2(0, -(1 - font_height_ratio) * DisplayedCountText.Height + vertical_offset);

            PopOutCountText.OriginPosition = new Vector2(3, font_height_ratio * PopOutCountText.Height + vertical_offset); // In stable, the bigger pop out scales a bit to the left
            PopOutCountText.Position = new Vector2(0, -(1 - font_height_ratio) * PopOutCountText.Height + vertical_offset);
        }

        protected override void OnCountIncrement()
        {
            scheduledPopOut?.Cancel();
            scheduledPopOut = null;

            DisplayedCountText.Show();

            PopOutCountText.Text = FormatCount(Current.Value);

            PopOutCountText.ScaleTo(1.56f)
                           .ScaleTo(1, big_pop_out_duration);

            PopOutCountText.FadeTo(0.6f)
                           .FadeOut(big_pop_out_duration);

            this.Delay(big_pop_out_duration - 140).Schedule(() =>
            {
                base.OnCountIncrement();

                DisplayedCountText.ScaleTo(1).Then()
                                  .ScaleTo(1.1f, small_pop_out_duration / 2, Easing.In).Then()
                                  .ScaleTo(1, small_pop_out_duration / 2, Easing.Out);
            }, out scheduledPopOut);
        }

        protected override void OnCountRolling()
        {
            scheduledPopOut?.Cancel();
            scheduledPopOut = null;

            base.OnCountRolling();
        }

        protected override void OnCountChange()
        {
            scheduledPopOut?.Cancel();
            scheduledPopOut = null;

            base.OnCountChange();
        }

        protected override string FormatCount(int count) => $@"{count}x";
    }
}
