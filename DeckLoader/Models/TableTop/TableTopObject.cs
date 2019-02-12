using System.Collections.Generic;

namespace DeckLoader.Models.TableTop
{
    public class TableTopObject
    {
        public TableTopObject()
        {
            ObjectStates = new List<TableTopDeck>
            {
                new TableTopDeck
                {
                    Name = "DeckCustom",
                    Transform = new TableTopTransform
                    {
                        posX = -2.68930244,
                        posY = 1.41188371,
                        posZ = -5.359127
                    }
                }
            };
        }

        public string SaveName { get; set; } = "";
        public string GameMode { get; set; } = "";
        public double Gravity { get; set; } = 0.5;
        public double PlayArea { get; set; } = 0.5;
        public string Date { get; set; } = "";
        public string Table { get; set; } = "";
        public string Sky { get; set; } = "";
        public string Note { get; set; } = "";
        public string Rules { get; set; } = "";
        public string XmlUI { get; set; } = "";
        public string LuaScript { get; set; } = "";
        public string LuaScriptState { get; set; } = "";
        public List<TableTopDeck> ObjectStates { get; set; }
        public object TabStates { get; set; } = new object();
        public string VersionNumber { get; set; } = "";
    }
}
