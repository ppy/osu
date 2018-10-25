using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using Symcol.Core.Graphics.Containers;
using Symcol.osu.Core.Containers.Text;

namespace Symcol.osu.Mods.Caster.CasterScreens.Pieces
{
    public class EditableOsuSpriteText : SymcolContainer
    {
        public string Text
        {
            get => OsuSpriteText.Text;
            set
            {
                OsuSpriteText.Text = value;
                OsuTextBox.Text = value;
            }
        }

        public float TextSize
        {
            get => OsuSpriteText.TextSize;
            set
            {
                OsuSpriteText.TextSize = value;
                OsuTextBox.Height = value;
            }
        }

        public readonly Bindable<bool> Editable = new Bindable<bool>();

        public readonly ClickableOsuSpriteText OsuSpriteText;
        public readonly OsuTextBox OsuTextBox;

        public EditableOsuSpriteText()
        {
            OsuColour osu = new OsuColour();

            Children = new Drawable[]
            {
                OsuSpriteText = new ClickableOsuSpriteText
                {
                    IdleColour = osu.Pink,
                    TextSize = 40
                },
                OsuTextBox = new OsuTextBox
                {
                    RelativeSizeAxes = Axes.X
                }
            };

            OsuTextBox.OnCommit += (commit, ree) => { OsuSpriteText.Text = commit.Text; };

            Editable.ValueChanged += edit =>
            {
                OsuSpriteText.Alpha = edit ? 0 : 1;
                OsuTextBox.Alpha = edit ? 1 : 0;

                if (!edit)
                    OsuSpriteText.Text = OsuTextBox.Text;
            };
            Editable.TriggerChange();
        }
    }
}
