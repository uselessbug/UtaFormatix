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
            NoteList = new List<Note>();
            foreach (Note note in track.NoteList)
            {
                NoteList.Add(new Note(note));
            }
            PitchList = new List<Pit>();
            foreach (Pit pit in track.PitchList)
            {
                PitchList.Add(new Pit(pit));
            }
        }
        public int TrackNum;
        public string TrackName;
        public List<Note> NoteList;
        public List<Pit> PitchList;
    }
}
