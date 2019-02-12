using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeckLoader.Models.TableTop
{
    public class TableTopDeck
    {
        public TableTopDeck()
        {
            CustomDeck = new TableTopCustomDeck();
            ColorDiffuse = new TableTopColor();
            Transform = new TableTopTransform();
            DeckIDs = new List<int>();
            ContainedObjects = new List<TableTopCard>();
        }

        public string Name { get; set; } = "";
        public TableTopTransform Transform { get; set; }
        public string Nickname { get; set; } = "";
        public string Description { get; set; } = "";
        public TableTopColor ColorDiffuse { get; set; }
        public bool Locked { get; set; }
        public bool Grid { get; set; } = true;
        public bool Snap { get; set; } = true;
        public bool IgnoreFoW { get; set; }
        public bool AutoRaise { get; set; }
        public bool Sticky { get; set; } = true;

        [JsonProperty("ToolTip")]
        public bool ShowToolTip { get; set; } = true;
        public bool GridProjection { get; set; }
        public bool HideWhenFaceDown { get; set; } = true;
        public bool Hands { get; set; }
        public bool SideWaysCard { get; set; }
        public string XmlUI { get; set; } = "";
        public string LuaScript { get; set; } = "";
        public string LuaScriptStates { get; set; } = "";
        public string GUID { get; set; } = "";

        public List<int> DeckIDs { get; set; }
        public TableTopCustomDeck CustomDeck { get; set; }
        public List<TableTopCard> ContainedObjects { get; set; }
    }
}
