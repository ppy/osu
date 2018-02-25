using osu.Framework.Configuration;
using osu.Game.Online.API.Requests;
using osu.Game.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Overlays.Profile.Sections
{
    class PaginatedRecentActivityContainer : PaginatedContainer
    {
        public PaginatedRecentActivityContainer(Bindable<User> user, string header, string missing)
            : base(user, header, missing)
        {
            ItemsPerPage = 5;
        }

        //protected override void ShowMore()
        //{
        //    base.ShowMore();

        //    var req = new GetUserRecentActivitiesRequest(User.Value.Id, VisiblePages++ * ItemsPerPage);

        //    req.Success += scores =>
        //    {
        //        foreach (var s in scores)
        //            s.ApplyRuleset(Rulesets.GetRuleset(s.OnlineRulesetID));

        //        ShowMoreButton.FadeTo(scores.Count == ItemsPerPage ? 1 : 0);
        //        ShowMoreLoading.Hide();

        //        if (!scores.Any() && VisiblePages == 1)
        //        {
        //            MissingText.Show();
        //            return;
        //        }

        //        MissingText.Hide();

        //        foreach (OnlineScore score in scores)
        //        {
        //            DrawableProfileScore drawableScore;

        //            switch (type)
        //            {
        //                default:
        //                    drawableScore = new DrawablePerformanceScore(score, includeWeight ? Math.Pow(0.95, ItemsContainer.Count) : (double?)null);
        //                    break;
        //                case ScoreType.Recent:
        //                    drawableScore = new DrawableTotalScore(score);
        //                    break;
        //            }

        //            ItemsContainer.Add(drawableScore);
        //        }
        //    };

        //    Api.Queue(req);
        //}
    }
}
