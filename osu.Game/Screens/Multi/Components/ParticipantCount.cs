// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Screens.Multi.Components
{
    public class ParticipantCountDisplay : CompositeDrawable
    {
        private const float text_size = 30;
        private const float transition_duration = 100;

        private readonly OsuSpriteText slash, maxText;

        public readonly IBindable<IEnumerable<User>> Participants = new Bindable<IEnumerable<User>>();
        public readonly IBindable<int> ParticipantCount = new Bindable<int>();
        public readonly IBindable<int?> MaxParticipants = new Bindable<int?>();

        public ParticipantCountDisplay()
        {
            AutoSizeAxes = Axes.Both;

            OsuSpriteText count;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                LayoutDuration = transition_duration,
                Children = new[]
                {
                    count = new OsuSpriteText
                    {
                        TextSize = text_size,
                        Font = @"Exo2.0-Bold"
                    },
                    slash = new OsuSpriteText
                    {
                        Text = @"/",
                        TextSize = text_size,
                        Font = @"Exo2.0-Light"
                    },
                    maxText = new OsuSpriteText
                    {
                        TextSize = text_size,
                        Font = @"Exo2.0-Light"
                    },
                }
            };

            Participants.BindValueChanged(v => count.Text = v.Count().ToString());
            MaxParticipants.BindValueChanged(_ => updateMax(), true);
            ParticipantCount.BindValueChanged(v => count.Text = v.ToString("#,0"));
        }

        private void updateMax()
        {
            if (MaxParticipants.Value == null)
            {
                slash.FadeOut(transition_duration);
                maxText.FadeOut(transition_duration);
            }
            else
            {
                slash.FadeIn(transition_duration);
                maxText.Text = MaxParticipants.Value.ToString();
                maxText.FadeIn(transition_duration);
            }
        }
    }
}
