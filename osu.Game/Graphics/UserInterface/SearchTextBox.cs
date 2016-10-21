using System;
using System.Timers;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Graphics.UserInterface
{
    public class SearchTextBox : TextBox
    {
        public double FadeDuration => 300;
        public Timer OnSearchTimer { get; set; }
        public TextAwesome SearchIcon { get; set; }

        public SearchTextBox()
        {
            OnChange += SearchTextBox_OnChange;
            OnSearchTimer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds)
            {
                AutoReset = false
            };
            OnSearchTimer.Elapsed += OnSearchTimer_Elapsed;
        }

        #region Disposal

        protected override void Dispose(bool disposing)
        {
            OnChange -= SearchTextBox_OnChange;
            OnSearchTimer.Elapsed -= OnSearchTimer_Elapsed;
            base.Dispose(disposing);
        }

        #endregion

        public override void Load(BaseGame game)
        {
            base.Load(game);
            Add(
                SearchIcon = new TextAwesome
                {
                    Origin = Anchor.CentreRight,
                    Anchor = Anchor.CentreRight,
                    Icon = FontAwesome.search,
                    Position = new Vector2(5, 0)
                });
        }

        private void OnSearchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnSearch?.Invoke(this, new OnSearchEventArgs(Text));
        }

        private void SearchTextBox_OnChange(TextBox sender, bool newText)
        {
            if (!newText) return;
            OnSearchTimer.Stop();
            OnSearchTimer.Start();
            if (string.IsNullOrEmpty(Text))
                SearchIcon.FadeIn(FadeDuration);
            else
                SearchIcon.FadeOut(FadeDuration);
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