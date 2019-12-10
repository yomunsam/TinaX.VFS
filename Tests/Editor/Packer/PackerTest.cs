using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TinaXEditor.VFSKit;

namespace TinaXTests.VFSKit
{
    public class PackerTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void MakeAssetBundle()
        {
            //var packer = new VFSPacker()
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator PackerTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
