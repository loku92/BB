using BassBooster.Common;
using BassBooster.Models;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class TrackListTest
    {
        private TrackList SetUp()
        {
            TrackList tl = new TrackList();
            tl.Add(new Track(1, "John", "Lennon", "jl.mp3", new TimeSpan(0, 3, 1)));
            tl.Add(new Track(2, "abcd", "efgh", "gcd.mp3", new TimeSpan(0, 4, 13)));
            tl.Music[0].Duration = 181;
            return tl;
        }

        [TestMethod]
        public void TLConstructorTest()
        {
            Assert.IsNotNull(new TrackList());
        }

        [TestMethod]
        public void TLConstructorTest2()
        {
            Assert.IsNotNull((new TrackList()).Music);
        }

        [TestMethod]
        public void AddTest()
        {
            TrackList tl = SetUp();
            Assert.AreEqual(tl.Music.Count,2);
        }

        [TestMethod]
        public void AddTest2()
        {
            TrackList tl = SetUp();
            Assert.AreEqual(tl.Length, 2);
        }

        [TestMethod]
        public void GetDurationIntByIdTest()
        {
            TrackList tl = new TrackList();
            tl.Add(new Track(1, "John", "Lennon", "jl.mp3", new TimeSpan(0, 3, 1)));
            tl.Add(new Track(2, "abcd", "efgh", "gcd.mp3", new TimeSpan(0, 4, 13)));
            Assert.AreEqual(tl.GetDurationIntById(1), 181);
        }

        [TestMethod]
        public void GetDurationStringByIdTest()
        {
            TrackList tl = new TrackList();
            tl.Add(new Track(1, "John", "Lennon", "jl.mp3", new TimeSpan(0, 3, 1)));
            tl.Add(new Track(2, "abcd", "efgh", "gcd.mp3", new TimeSpan(0, 4, 13)));
            string value = tl.GetDurationStringById(1);
            StringAssert.Equals(value,"4:13");
        }

        [TestMethod]
        public void TrackToStringTest()
        {
            TrackList tl = new TrackList();
            Track t = new Track(1, "John", "Lennon", "jl.mp3", new TimeSpan(0, 3, 1));
            tl.Add(new Track(1, "John", "Lennon", "jl.mp3", new TimeSpan(0, 3, 1)));
            tl.Add(new Track(2, "abcd", "efgh", "gcd.mp3", new TimeSpan(0, 4, 13)));
            string value = tl.GetDurationStringById(1);
            StringAssert.Equals(t.ToString(), tl.TrackToString(0));
        }
                  
    }
}
