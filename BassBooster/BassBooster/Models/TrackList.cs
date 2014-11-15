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

           
        public string TrackToString(int i)
        {
            return Music[i].ToString();
        }


        public string GetDurationStringById(int id)
        {
            var t = (from track in Music
                     where track.Id == id
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
                     where track.Id == id
                     select track).Single();
            return t.Duration;
        }
    }
}
