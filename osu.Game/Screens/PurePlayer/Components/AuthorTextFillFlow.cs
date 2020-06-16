using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Allocation;
using osu.Framework.Bindables;

namespace osu.Game.Screens.PurePlayer.Components
{
    public class AuthorTextFillFlow : FillFlowContainer
    {

        [Resolved]
        private IBindable<WorkingBeatmap> b { get; set; }

        private OsuSpriteText name;
        private OsuSpriteText author;
        public AuthorTextFillFlow()
        {
            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Vertical;
            InternalChildren = new Drawable[]
            {
                name = new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Text = "Title",
                    Font = OsuFont.GetFont(size: 24, weight: FontWeight.SemiBold),
                },
                author = new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Text = "Author",
                    Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                },
            };
        }

        protected override void LoadComplete()
        {
            b.BindValueChanged(OnWorkingBeatmapChanged, true);
        }

        private void OnWorkingBeatmapChanged(ValueChangedEvent<WorkingBeatmap> B)
        {
            UpdateInfo(B.NewValue);
        }

        public void UpdateInfo(WorkingBeatmap b)
        {
            name.Text = new LocalisedString((b.Metadata.TitleUnicode, b.Metadata.Title));
            author.Text = new LocalisedString((b.Metadata.ArtistUnicode, b.Metadata.Artist));
        }
    }
}