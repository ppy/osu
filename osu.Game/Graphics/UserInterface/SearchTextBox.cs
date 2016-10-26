using System;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Threading;
using OpenTK;

namespace osu.Game.Graphics.UserInterface
{
    public class SearchTextBox : TextBox
    {
        private double fadeDuration => 300;
        private double onSearchDelay => TimeSpan.FromSeconds(1).TotalMilliseconds;
        private ScheduledDelegate lastScheduledOnSearch;
        private TextAwesome searchIcon;

        public SearchTextBox()
        {
            OnChange += SearchTextBox_OnChange;
        }

        #region Disposal

        protected override void Dispose(bool disposing)
        {
            OnChange -= SearchTextBox_OnChange;
            base.Dispose(disposing);
        }

        #endregion

        public override void Load(BaseGame game)
        {
            base.Load(game);
            Add(
                searchIcon = new TextAwesome
                {
                    Origin = Anchor.CentreRight,
                    Anchor = Anchor.CentreRight,
                    Icon = FontAwesome.search,
                    Position = new Vector2(5, 0)
                });
        }

        private void SearchTextBox_OnChange(TextBox sender, bool newText)
        {
            if (!newText) return;
            lastScheduledOnSearch?.Cancel();
            lastScheduledOnSearch = Scheduler.AddDelayed(
                () => OnSearch?.Invoke(this, new OnSearchEventArgs(Text)),
                onSearchDelay);
            if (string.IsNullOrEmpty(Text))
                searchIcon.FadeIn(fadeDuration);
            else
                searchIcon.FadeOut(fadeDuration);
        }

        public delegate void OnSearchHandler(object sender, OnSearchEventArgs e);

        public event OnSearchHandler OnSearch;

        public class OnSearchEventArgs : EventArgs
        {
            public string RequestText { get; set; }

            public OnSearchEventArgs(string requestText)
            {
                RequestText = requestText;
            }
        }
    }
}