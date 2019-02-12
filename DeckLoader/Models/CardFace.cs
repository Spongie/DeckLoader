using System.Collections.Generic;

namespace DeckLoader.Models
{
    public class CardFace
    {
        public string Object { get; set; }
        public string Name { get; set; }
        public string Mana_cost { get; set; }
        public string Type_line { get; set; }
        public string Oracle_text { get; set; }
        public List<string> Colors { get; set; }
        public string Power { get; set; }
        public string Toughness { get; set; }
        public string Artist { get; set; }
        public string Illustration_id { get; set; }
        public ImageUris Image_uris { get; set; }
    }
}
