using NUnit.Framework;

namespace HgVersionTests.IntegrationTests
{
    [TestFixture, NonParallelizable]
    public class DevelopScenarios
    {
        [Test, NonParallelizable]
        public void WhenDevelopHasMultipleCommits_SpecifyExistingCommitId()
        {
            using (var context = new TestVesionContext())
            {
                context.CreateBranch("dev");
                context.WriteTextAndCommit("dummy.txt", "", "init commit");
                context.MakeTaggedCommit("1.0.0");

                context.MakeCommit();
                context.MakeCommit();
                context.MakeCommit();

                var thirdCommit = context.Tip(); 
                context.MakeCommit();
                context.MakeCommit();

                context.AssertFullSemver("1.1.0-alpha.3", commitId: thirdCommit.Hash);
            }
        }
        
        [Test, NonParallelizable]
        public void WhenDevelopBranchedFromTaggedCommitOnDefaultVersionDoesNotChange()
        {
            using (var context = new TestVesionContext())
            {
                context.CreateBranch("dev");
                context.WriteTextAndCommit("dummy.txt", "", "init commit");
                context.MakeTaggedCommit("1.0.0");
                context.AssertFullSemver("1.0.0");
            }
        }

        [Test, NonParallelizable]
        public void WhenBranchNameHasRUChars_ItIsStillWorking()
        {
            using (var context = new TestVesionContext())
            {
                var branchName = "одинокая ветка сирени";
                context.CreateBranch(branchName);
                context.WriteTextAndCommit("тест.txt", "тест", "тестовый коммит");

                Assert.That(context.CurrentBranch.Name, Is.EqualTo(branchName));

            }
        }
    }
}