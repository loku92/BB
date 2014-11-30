using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassBooster.Models
{

    /// <summary>
    /// DataModel of playlist
    /// </summary>
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

        /// <summary>
        /// Returns duration of the song with given id
        /// </summary>
        /// <param name="id">Song's id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return duration of song as integer
        /// </summary>
        /// <param name="id">Song/s id</param>
        /// <returns></returns>
        public int GetDurationIntById(int id)
        {
            var t = (from track in Music
                     where track.Id == id
                     select track).Single();
#if DEBUG
            Debug.WriteLine(t.Duration / 1000);
#endif
            return t.Duration/1000;
        }
    }
}
