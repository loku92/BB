using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassBooster.Models
{
    public class TrackList
    {
        public List<Track> Music { get; set; }
        public int? Length { get { return Music.Count; }  }

        public TrackList()
        {
            Music = new List<Track>();
        }

        public void Add(Track t)
        {
            Music.Add(t);
        }
    }
}
