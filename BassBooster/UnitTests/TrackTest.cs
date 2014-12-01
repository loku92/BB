using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using BassBooster.Models;
using System.Diagnostics;

namespace UnitTests
{
    [TestClass]
    public class TrackTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            Track t = new Track (1,"John", "Lennon" ,"jl.mp3" , new TimeSpan(0,3,33));
            Assert.IsNotNull(t);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            Track t = new Track(1, "John", "Lennon", "jl.mp3", new TimeSpan(0, 3, 33));
            Assert.IsInstanceOfType(t, typeof(Track));
        }

        [TestMethod]
        public void ToStringTest()
        {
            Track t = new Track(1, "John", "Lennon", "jl.mp3", new TimeSpan(0, 3, 33));
            Assert.IsNotNull(t.ToString());
        }

        [TestMethod]
        public void ToStringTest2()
        {
            Track t = new Track(1, null, null, "jl.mp3", new TimeSpan(0, 3, 33));
            Assert.IsNotNull(t.ToString());
        }
    }
}
