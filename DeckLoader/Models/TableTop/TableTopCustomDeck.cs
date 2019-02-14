using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeckLoader.Models.TableTop
{
    public class TableTopCustomDeck
    {
        public TableTopCustomDeck()
        {
            TableTopDeckInfos = new Dictionary<int, TableTopDeckImageInfo>();
        }

        public Dictionary<int, TableTopDeckImageInfo> TableTopDeckInfos { get; set; }
    }

    public class TableTopCustomDeckJson : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.Equals(typeof(TableTopCustomDeck));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            var me = (TableTopCustomDeck)value;

            writer.WriteStartObject();
            foreach (var key in me.TableTopDeckInfos.Keys)
            {
                
                writer.WritePropertyName(key.ToString());
                serializer.Serialize(writer, me.TableTopDeckInfos[key]);
                
                
            }

            writer.WriteEndObject();

        }
    }
}
