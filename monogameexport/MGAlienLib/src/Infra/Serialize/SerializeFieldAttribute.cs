

namespace MGAlienLib
{
    public class SerializeFieldAttribute : System.Attribute
    {
        public bool HideInInspector { get; set; }
        public bool BrowseFile { get; set; }

        public SerializeFieldAttribute(bool hideInInspector = false, bool browseFile = false)
        {
            HideInInspector = hideInInspector;
            BrowseFile = browseFile;
        }
    }

    public class SerializeDTOAttribute : SerializeFieldAttribute
    {
    }

}
