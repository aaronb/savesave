using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace savesave
{
    using NUnit.Framework;

    [TestFixture]
    class LinkTest
    {

        string testpath;

        protected string path(string last)
        {
            return Path.Combine(testpath, last);
        }

        [SetUp]
        protected void SetUp()
        {
            testpath = Path.Combine(TestContext.CurrentContext.TestDirectory, "testdata");
            Directory.CreateDirectory(testpath);
            Directory.Delete(testpath, true);
            Directory.CreateDirectory(testpath);
        }

        [Test]
        public void CreateLink()
        {
            Directory.CreateDirectory(path("targetdir"));
            Link.CreateSymbolicLink(path("linkdir"), path("targetdir"));

            using (StreamWriter file = new StreamWriter(path("targetdir\\target")))
            {
                file.WriteLine("hello");
            }
            Link.CreateSymbolicLink(path("link"), path("targetdir\\target"));

            Assert.That(File.Exists(path("linkdir\\target")));
            Assert.That(File.Exists(path("link")));

            Assert.That(Link.IsSymbolicLink(path("link")));
            Assert.That(Link.IsSymbolicLink(path("linkdir")));

            Assert.That(!Link.IsSymbolicLink(path("targetdir\\target")));
            Assert.That(!Link.IsSymbolicLink(path("targetdir")));
        }

        [Test]
        public void Unicode()
        {
            Directory.CreateDirectory(path("targetdir\u63f4"));
            Link.CreateSymbolicLink(path("\u7d46linkdir"), path("targetdir\u63f4"));

            using (StreamWriter file = new StreamWriter(path("targetdir\u63f4\\target\u6551"))) {
                file.WriteLine("hello");
            }
            Link.CreateSymbolicLink(path("\u7d46link"), path("targetdir\u63f4\\target\u6551"));

            Assert.That(File.Exists(path("\u7d46linkdir\\target\u6551")));
            Assert.That(File.Exists(path("\u7d46link")));

            Assert.That(Link.IsSymbolicLink(path("\u7d46link")));
            Assert.That(Link.IsSymbolicLink(path("\u7d46linkdir")));

            Assert.That(!Link.IsSymbolicLink(path("targetdir\u63f4\\target\u6551")));
            Assert.That(!Link.IsSymbolicLink(path("targetdir\u63f4")));
        }
    }
}
