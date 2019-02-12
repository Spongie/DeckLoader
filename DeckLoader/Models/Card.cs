using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeckLoader.Models
{
    public class Card
    {
        public string Id { get; set; }
        public string Oracle_id { get; set; }
        public IList<object> Multiverse_ids { get; set; }
        public string Name { get; set; }
        public string Lang { get; set; }
        public string Released_at { get; set; }
        public string Uri { get; set; }
        public string Scryfall_uri { get; set; }
        public string Layout { get; set; }
        public bool Highres_image { get; set; }
        public ImageUris Image_uris { get; set; }
        public string Mana_cost { get; set; }
        public double Cmc { get; set; }
        public string Type_line { get; set; }
        public string Oracle_text { get; set; }
        public IList<object> Colors { get; set; }
        public IList<string> Color_identity { get; set; }
        public Legalities Legalities { get; set; }
        public IList<string> Games { get; set; }
        public bool Reserved { get; set; }
        public bool Foil { get; set; }
        public bool Nonfoil { get; set; }
        public bool Oversized { get; set; }
        public bool Promo { get; set; }
        public bool Reprint { get; set; }
        public string Set { get; set; }
        public string Set_name { get; set; }
        public string Set_uri { get; set; }
        public string Set_search_uri { get; set; }
        public string Scryfall_set_uri { get; set; }
        public string Rulings_uri { get; set; }
        public string Prints_search_uri { get; set; }
        public string Collector_number { get; set; }
        public bool Digital { get; set; }
        public string Rarity { get; set; }
        public string Watermark { get; set; }
        public string Illustration_id { get; set; }
        public string Artist { get; set; }
        public string Border_color { get; set; }
        public string Frame { get; set; }
        public string Frame_effect { get; set; }
        public bool Full_art { get; set; }
        public bool Story_spotlight { get; set; }
        public RelatedUris Related_uris { get; set; }
        [JsonProperty("card_faces")]
        public List<CardFace> CardFaces { get; set; }
    }
}
