// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Verify
{
    internal abstract class IssueSection : Section
    {
        protected IssueList IssueList;

        protected IssueSection(IssueList issueList)
        {
            IssueList = issueList;
        }
    }
}
