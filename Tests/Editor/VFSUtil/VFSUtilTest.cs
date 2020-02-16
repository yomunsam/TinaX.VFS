using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TinaX.VFSKitInternal.Utils;

namespace Tests
{
    public class VFSUtilTest
    {
        // A Test behaves as an ordinary method
        [Test]
        [TestCase("Assets/AA/BB","Assets/AA/",false,true)]
        [TestCase("Assets/AA","Assets/AA/BB",false,false)]
        [TestCase("Assets/AA","Assets/AA/BB",true,true)]
        public void IsSubpathTest(string path1, string path2, bool mutual, bool expect)
        {
            if(expect == VFSUtil.IsSubpath(path1, path2, mutual))
            {
                Assert.IsTrue(true);
            }
            else
            {
                TestContext.Out.WriteLine($"测试与预期不符, path1: {path1}, path2: { path2}, mutual: {mutual.ToString()}, 预期：{expect.ToString()}");
                Assert.IsFalse(true);
            }
        }

        //// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        //// `yield return null;` to skip a frame.
        //[UnityTest]
        //public IEnumerator VFSUtilTestWithEnumeratorPasses()
        //{
        //    // Use the Assert class to test conditions.
        //    // Use yield to skip a frame.
        //    yield return null;
        //}
    }
}
