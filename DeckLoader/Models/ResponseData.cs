namespace DeckLoader.Models
{
    public class ResponseData
    {
        public string OriginalName { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string DeleteToken { get; set; }
        public long Size { get; set; }
        public string Link { get; set; }
        public string DeleteLink { get; set; }
    }
}
