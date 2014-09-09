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

        public void RangeAdd(int listId, int count)
        {
            ranges.Add(ranges[listId] + count);
        }

        public string TrackToString(int i)
        {
            return Music[i].ToString();
        }

        public int[] FindById(int id)
        {
            var t = (from track in Music
                    where track.DisplayId == id
                    select track).Single();
            return new int[]{t.Id,t.ListId};
        }

        public string GetDurationStringById(int id)
        {
            var t = (from track in Music
                     where track.DisplayId == id
                     select track).Single();
            string duration = "";
            duration += t.Duration / 60 + ":";
            int sec = t.Duration % 60;
            if (sec < 10)
                duration += "0" + sec;
            else
                duration += sec;
            return  duration;
        }

        public int GetDurationIntById(int id)
        {
            var t = (from track in Music
                     where track.DisplayId == id
                     select track).Single();
            return t.Duration;
        }
    }
}
