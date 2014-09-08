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
        public List<int> ranges { get; set; }

        public TrackList()
        {
            Music = new List<Track>();
            ranges = new List<int>();
            ranges.Add(0);
        }

        public void Add(Track t)
        {
            Music.Add(t);

        }

        public int Last(int listId)
        {
            return ranges[listId+1]-1;
        }

        public int First(int listId){
            return ranges[listId];
        }

        internal void RangeAdd(int listId, int count)
        {
            ranges.Add(ranges[listId] + count);
        }

        public string TrackToString(int i)
        {
            return Music[i].ToString();
        }
    }
}
