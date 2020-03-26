using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace TinaX.VFSKit
{
    public interface ISceneAsset
    {
        void OpenScene(LoadSceneMode loadMode = LoadSceneMode.Single);
    }
}
