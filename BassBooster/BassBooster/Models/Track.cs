using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassBooster.Models
{
    public class Track
    {        
        public int Id { get; set; }
        public int ListId { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public int Length { get; set; }

        public Track(int id, int listId, string artist, string title, string fName, TimeSpan length)
        {

            this.Id = id;
            this.ListId = listId;
            this.Artist = artist;
            this.Title = title;
            this.FileName = fName;
            this.Length = (int)length.TotalSeconds;

        }

        public override string ToString()
        {
            if (!String.IsNullOrWhiteSpace(Title))
                return this.Artist + " - " + this.Title;
            return FileName.Substring(0,FileName.Length-4);
        }

    }
}
