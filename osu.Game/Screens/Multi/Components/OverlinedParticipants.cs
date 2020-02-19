// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Multi.Components
{
    public class OverlinedParticipants : OverlinedDisplay
    {
        public OverlinedParticipants()
            : base("Participants")
        {
            Content.Add(new ParticipantsList { RelativeSizeAxes = Axes.Both });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ParticipantCount.BindValueChanged(_ => setParticipantCount());
            MaxParticipants.BindValueChanged(_ => setParticipantCount());

            setParticipantCount();
        }

        private void setParticipantCount() => Details = MaxParticipants.Value != null ? $"{ParticipantCount.Value}/{MaxParticipants.Value}" : ParticipantCount.Value.ToString();
    }
}
