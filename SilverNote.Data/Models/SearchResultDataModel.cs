using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SilverNote.Data.Models
{
    [XmlType("search_result")]
    public class SearchResultDataModel : DataModelBase
    {
        [XmlElement("note")]
        public NoteDataModel Note { get; set; }

        [XmlElement("text")]
        public string Text { get; set; }

        [XmlArray("offsets")]
        public SearchResultOffsetDataModel[] Offsets { get; set; }
    }

    [XmlType("result_offset")]
    public class SearchResultOffsetDataModel
    {
        public static SearchResultOffsetDataModel[] FromString(string str, int column)
        {
            // http://www.sqlite.org/fts3.html#section_4_1

            string[] tokens = str.Split();

            var results = new List<SearchResultOffsetDataModel>();

            for (int i = 0; i + 4 <= tokens.Length; i += 4)
            {
                if (tokens[i] != column.ToString())
                {
                    continue;
                }

                int offset, length;
                if (!Int32.TryParse(tokens[i + 2], out offset) || !Int32.TryParse(tokens[i + 3], out length))
                {
                    continue;
                }

                var result = new SearchResultOffsetDataModel(offset, length);

                results.Add(result);
            }

            results.Sort((a, b) => a.Offset - b.Offset);

            return results.ToArray();
        }

        public SearchResultOffsetDataModel()
        {

        }

        public SearchResultOffsetDataModel(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }

        [XmlAttribute("offset")]
        public int Offset { get; set; }

        [XmlAttribute("length")]
        public int Length { get; set; }
    }
}
