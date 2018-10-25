using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Core.Containers
{
    public class SymcolSettingsTextBox : SettingsTextBox
    {
        public readonly OsuTextBox OsuTextBox = new OsuTextBox
        {
            RelativeSizeAxes = Axes.X,
            //Height = 16
        };

        protected override Drawable CreateControl() => OsuTextBox;

        public SymcolSettingsTextBox(TextBox.OnCommitHandler onCommit = null)
        {
            OsuTextBox.OnCommit += onCommit;
            OsuTextBox.OnCommit += (text, n) =>
            {
                if (Bindable != null)
                    Bindable.Value = text.Current;
            };
        }
    }
}
