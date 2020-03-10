## 关于各种文件的描述以及它们应该存放在哪儿


## XAssetBundleManifest

一个类似`UnityEngine.AssetBundleManifest`的东西。因为Unity的Manifest不能满足需求，于是VFS中自己实现了一个。



每个Group拥有一个独立的Manifest文件，在Main Package中，它存放在根目录下的数据目录的`Manifests`文件夹下。这个目录作为常量定义在`TinaX.VFSKit.Const.VFSConst.MainPackage_AssetBundleManifests_Folder`. 在文件夹中，每个组的Manifest文件使用该组的*16位小写MD5*值命名

在扩展组下，Manifest文件存放在自己的Package的根目录，文件名为`VFSManifest.json`， 作为常量定义在`TinaX.VFSKit.Const.VFSConst.AssetBundleManifestFileName`;

获取Manifest路径的方法在`TinaX.VFS.VFSGroup`对象中`GetManifestFilePath`方法。

------

## AssetBundle Files Hash

一个把每个组的所有AssetBundle的hash信息都存下来的文件，使用16位小写MD5来记录。

每组拥有一个独立的FilesHash文件，在Main Package中，它存放在根目录下的数据目录的`Manifests`文件夹下。这个目录作为常量定义在`TinaX.VFSKit.Const.VFSConst.MainPackage_AssetBundle_Hash_Files_Folder`. 在文件夹中，每个组的Manifest文件使用该组的*16位小写MD5*值命名

在扩展组下，Manifest文件存放在自己的Package的根目录，文件名为`FilesHash.json`， 作为常量定义在`TinaX.VFSKit.Const.VFSConst.AssetBundleFilesHash_FileName`;

获取Manifest路径的方法在`TinaX.VFS.VFSGroup`对象中`GetAssetBundleHashsFilePath`方法。

------
