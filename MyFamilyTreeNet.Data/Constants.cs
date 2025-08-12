namespace MyFamilyTreeNet.Data
{
    public static class Constants
    {
        // Error messages
        public const string RequireField = "Field is require";
        
        // Common field lengths
        public const int NameLength = 50;
        public const int EmailLength = 256;
        public const int BioLength = 1000;
        public const int TitleLength = 200;
        public const int DescriptionLength = 2000;
        public const int PlacesLength = 100;
        
        // Specific field lengths
        public const int FamilyDescriptionLength = 1000;
        public const int PlaceLength = 200;
        public const int NotesLength = 500;
        public const int StoryContentLength = 10000;
        public const int ProfilePictureUrlLength = 500;
        public const int RelationshipTypeNameLength = 100;
        public const int ShortProfilePictureUrlLength = 255;
        
        // Validation ranges
        public const int MinPositiveId = 1;
        public const int MaxIntValue = int.MaxValue;
    }
}