namespace TinaXEditor.VFS.AssetBuilder.Structs
{
    public struct AssetPathAndGuid
    {
        public string Path { get; set; }
        public string Guid { get; set; }

        public AssetPathAndGuid(string path , string guid) { Path = path; Guid = guid; }
    }
}
