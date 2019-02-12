using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeckLoader.Models.TableTop
{
    public class TableTopDeckImageInfo
    {
        public string FaceURL { get; set; }
        public string BackURL { get; set; } = "https://i.imgur.com/P7qYTcI.png";
        public int NumWidth { get; set; } = 10;
        public int NumHeigh { get; set; } = 7;
        public bool BackIsHidden { get; set; } = true;
        public bool UniqueBack { get; set; }
    }
}
