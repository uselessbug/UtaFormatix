using System.Collections.Generic;

namespace UtaFormatix.Model
{
    public class Track
    {
        public Track() { }
        public Track(Track track)
        {
            TrackNum = track.TrackNum;
            TrackName = track.TrackName;
            NoteList = new List<Note>(track.NoteList);
            PitchList = new List<Pit>(track.PitchList);
            SingerId = track.SingerId;
            SingerName = track.SingerName;
        }
        public int TrackNum;
        public string TrackName;
        public string SingerId;
        public string SingerName;
        public List<Note> NoteList = new List<Note>();
        public List<Pit> PitchList = new List<Pit>();
    }
}
