using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class SettingsTextBoxWithIndicator : SettingsTextBox
    {
        public enum ParseState
        {
            Working,
            Success,
            Failed
        }

        private Indicator currentIndicator;

        [BackgroundDependencyLoader]
        private void load()
        {
            FlowContent.Add(currentIndicator = new Indicator());
        }

        public void ChangeState(ParseState state, string result, List<string> errors = null)
        {
            currentIndicator.UpdateInfo(state, result, errors);
        }

        private class Indicator : CompositeDrawable
        {
            private readonly SpriteIcon iconDisplay = new SpriteIcon
            {
                Icon = FontAwesome.Solid.Times,
                Size = new Vector2(12)
            };

            private readonly OsuTextFlowContainer textDisplay = new OsuTextFlowContainer(t =>
            {
                t.Font = OsuFont.GetFont(size: 16);
            })
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X
            };

            public Indicator(ParseState state = ParseState.Working, string result = "等待解析...", List<string> errors = null)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                var margin = textDisplay.Margin;
                margin.Left = 20;

                textDisplay.Margin = margin;

                InternalChildren = new Drawable[]
                {
                    iconDisplay,
                    textDisplay
                };

                UpdateInfo(state, result, errors);
            }

            [Resolved]
            private OsuColour colors { get; set; }

            public void UpdateInfo(ParseState newState, string result, List<string> errors = null)
            {
                this.FlashColour(Color4.White, 1000, Easing.OutQuint);

                string errorsText = string.Empty;

                if (errors != null)
                {
                    foreach (string err in errors)
                    {
                        errorsText += err + "\n";
                    }
                }

                switch (newState)
                {
                    case ParseState.Failed:
                        iconDisplay.Icon = FontAwesome.Solid.Times;
                        textDisplay.Text = errorsText;
                        this.FadeColour(colors.Red, 300, Easing.OutQuint);
                        break;

                    case ParseState.Success:
                        iconDisplay.Icon = errors?.Count > 0 ? FontAwesome.Solid.Exclamation : FontAwesome.Solid.Check;
                        textDisplay.Text = errors?.Count > 0
                            ? errorsText
                            : $"解析完毕, 最终URL将会是 {result}";

                        this.FadeColour(errors?.Count > 0
                            ? Color4.Gold
                            : colors.Green, 300, Easing.OutQuint);
                        break;

                    case ParseState.Working:
                        iconDisplay.Icon = new IconUsage();
                        textDisplay.Text = result;
                        this.FadeTo(0.8f, 300, Easing.OutQuint);
                        break;
                }
            }
        }
    }
}
