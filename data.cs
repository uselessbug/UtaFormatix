using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.Linq;
using UtaFormatix.Model;

namespace UtaFormatix
{
    public enum UtaFormat
    {
        Vsq2, //VOCALOID2
        Vsq3, //VOCALOID3
        Vsq4, //VOCALOID4
        Ust, //UTAU
        Ccs, //CeVIO
        None
    }

    public class Data
    {
        private const string Vsq3NameSpace = "http://www.yamaha.co.jp/vocaloid/schema/vsq3/";
        private const string Vsq4NameSpace = "http://www.yamaha.co.jp/vocaloid/schema/vsq4/";
        public Data() { }
        public Data(Data data)
        {
            ProjectName = data.ProjectName;
            Files = data.Files;
            TrackList = new List<Track>();
            foreach (Track track in data.TrackList)
            {
                TrackList.Add(new Track(track));
            }
            TimeSigList = new List<TimeSig>();
            foreach (TimeSig timeSig in data.TimeSigList)
            {
                TimeSigList.Add(new TimeSig(timeSig));
            }
            TempoList = new List<Tempo>();
            foreach (Tempo tempo in data.TempoList)
            {
                TempoList.Add(new Tempo(tempo));
            }
            PreMeasure = data.PreMeasure;
            Lyric = new Lyric(this, false);
        }
        public string ProjectName;
        public List<string> Files;
        public List<Track> TrackList;
        public List<TimeSig> TimeSigList;
        public List<Tempo> TempoList;
        public int PreMeasure = 0;
        public Lyric Lyric;
        public bool Import(List<string> filenames)
        {
            Files = new List<string>();
            Files.AddRange(filenames);
            UtaFormat format = UtaFormat.None;

            //Determine the format of the project
            if (Files[0].Remove(0, Files[0].Length - 5) == ".vsqx")
            {
                string readbuf = File.ReadAllText(Files[0]);
                if (readbuf.Contains("vsq3 xmlns=\"http://www.yamaha.co.jp/vocaloid/schema/vsq3/\""))
                {
                    format = UtaFormat.Vsq3;
                }
                else if (readbuf.Contains("vsq4 xmlns=\"http://www.yamaha.co.jp/vocaloid/schema/vsq4/\""))
                {
                    format = UtaFormat.Vsq4;
                }
            }
            else if (Files[0].Remove(0, Files[0].Length - 4) == ".ust")
            {
                format = UtaFormat.Ust;
            }
            else if (Files[0].Remove(0, Files[0].Length - 4) == ".ccs")
            {
                format = UtaFormat.Ccs;
            }
            //else if (filename.Remove(0, filename.Length - 4) == ".vsq")
            //{
            //    format = UtaFormat.vsq2;
            //}
            else
            {
                return false;
            }

            switch (format)
            {
                case UtaFormat.Vsq2:
                    break;
                case UtaFormat.Vsq3:
                    return ImportVsq3(Files);
                case UtaFormat.Vsq4:
                    return ImportVsq4(Files);
                case UtaFormat.Ust:
                    return ImportUst(Files);
                case UtaFormat.Ccs:
                    return ImportCcs(Files);
            }
            return false;
        }

        public bool ImportVsq3(List<string> filenames)
        {
            if (filenames.Count <= 0)
            {
                //Why not just let the first one go?
                //MessageBox.Show("Cannot Import more than one vsqx.", "Import");
                return false;
            }

            XDocument vsq = XDocument.Load(filenames[0]);
            //Set up tracklist
            var nameSpace = (vsq.FirstNode as XElement)?.GetDefaultNamespace() ?? Vsq3NameSpace;
            ProjectName = Path.GetFileNameWithoutExtension(filenames[0]);
            TrackList = new List<Track>();
            TimeSigList = new List<TimeSig>();
            TempoList = new List<Tempo>();
            int TrackNum = 0;

            //Master Track
            XElement masterTrack = vsq.Descendants(nameSpace + "masterTrack").FirstOrDefault();
            if (masterTrack == null)
            {
                return false;
            }

            PreMeasure = Convert.ToInt32(masterTrack.FirstChild("preMeasure").Value);

            var timeSigNode = masterTrack.FirstChild("timeSig");
            TimeSig newtimeSig = new TimeSig
            {
                PosMes = Convert.ToInt32(timeSigNode.FirstChild("posMes").Value),
                Nume = Convert.ToInt32(timeSigNode.FirstChild("nume").Value),
                Denomi = Convert.ToInt32(timeSigNode.FirstChild("denomi").Value)
            };
            TimeSigList.Add(newtimeSig);

            var tempoNode = masterTrack.FirstChild("tempo");
            Tempo newtempo = new Tempo
            {
                PosTick = Convert.ToInt32(tempoNode.FirstChild("posTick").Value),
                BpmTimes100 = Convert.ToInt32(tempoNode.FirstChild("bpm").Value)
            };
            TempoList.Add(newtempo);

            //Vocal Tracks
            foreach (var vstrack in vsq.Descendants(nameSpace + "vsTrack"))
            {
                int noteNum = 0;
                Track newTrack = new Track
                {
                    TrackNum = TrackNum,
                    NoteList = new List<Note>(),
                    TrackName = vstrack.FirstChild("trackName").Value
                };
                //Set up notelist for every track

                foreach (var musical in vstrack.Childs("musicalPart"))
                {
                    int partStartTime = Convert.ToInt32(musical.FirstChild("posTick").Value);
                    foreach (var noteNode in musical.Childs("note"))
                    {
                        var posTick = Convert.ToInt32(noteNode.FirstChild("posTick").Value) + partStartTime;
                        Note newNote = new Note
                        {
                            NoteNum = noteNum,
                            NoteTimeOn = posTick,
                            NoteTimeOff = posTick + Convert.ToInt32(noteNode.FirstChild("durTick").Value),
                            NoteKey = Convert.ToInt32(noteNode.FirstChild("noteNum").Value),
                            NoteLyric = noteNode.FirstChild("lyric").Value
                        };
                        noteNum++;
                        newTrack.NoteList.Add(newNote);
                    }
                }

                if (newTrack.NoteList.Count > 0)
                {
                    TrackList.Add(newTrack);
                }
            }

            if (TrackList.Count > 0)
            {
                Lyric = new Lyric(this, true);
                return true;
            }
            return false;
        }
        public bool ImportVsq4(List<string> filenames)
        {
            if (filenames.Count <= 0)
            {
                //MessageBox.Show("Cannot Import more than one vsqx.", "Import");
                return false;
            }
            XDocument vsq = XDocument.Load(filenames[0]);
            var nameSpace = (vsq.FirstNode as XElement)?.GetDefaultNamespace() ?? Vsq4NameSpace;

            //Set up tracklist
            ProjectName = Path.GetFileNameWithoutExtension(filenames[0]);
            TrackList = new List<Track>();
            TimeSigList = new List<TimeSig>();
            TempoList = new List<Tempo>();
            int TrackNum = 0;

            //Master Track
            XElement masterTrack = vsq.Descendants(nameSpace + "masterTrack").FirstOrDefault();
            if (masterTrack == null)
            {
                return false;
            }
            PreMeasure = Convert.ToInt32(masterTrack.FirstChild("preMeasure").Value);

            var timeSigNode = masterTrack.FirstChild("timeSig");
            TimeSig newtimeSig = new TimeSig
            {
                PosMes = Convert.ToInt32(timeSigNode.FirstChild("m").Value),
                Nume = Convert.ToInt32(timeSigNode.FirstChild("nu").Value),
                Denomi = Convert.ToInt32(timeSigNode.FirstChild("de").Value)
            };
            TimeSigList.Add(newtimeSig);

            var tempoNode = masterTrack.FirstChild("tempo");
            Tempo newtempo = new Tempo
            {
                PosTick = Convert.ToInt32(tempoNode.FirstChild("t").Value),
                BpmTimes100 = Convert.ToInt32(tempoNode.FirstChild("v").Value)
            };
            TempoList.Add(newtempo);

            //Vocal Tracks
            foreach (var vstrack in vsq.Descendants(nameSpace + "vsTrack"))
            {
                int noteNum = 0;
                Track newTrack = new Track
                {
                    TrackNum = int.Parse(vstrack.FirstChild("tNo").Value),
                    NoteList = new List<Note>(),
                    TrackName = vstrack.FirstChild("name").Value
                };

                //Set up notelist for every track
                foreach (var musical in vstrack.Childs("vsPart"))
                {
                    int partStartTime = Convert.ToInt32(musical.FirstChild("t").Value);
                    foreach (var noteNode in musical.Childs("note"))
                    {
                        var posTick = Convert.ToInt32(noteNode.FirstChild("t").Value) + partStartTime;
                        Note newNote = new Note
                        {
                            NoteNum = noteNum,
                            NoteTimeOn = posTick,
                            NoteTimeOff = posTick + Convert.ToInt32(noteNode.FirstChild("dur").Value),
                            NoteKey = Convert.ToInt32(noteNode.FirstChild("n").Value),
                            NoteLyric = noteNode.FirstChild("y").Value
                        };
                        noteNum++;
                        newTrack.NoteList.Add(newNote);
                    }

                    foreach (var cc in musical.Childs("cc"))
                    {
                        Pit newPit = new Pit()
                        {
                            Pos = int.Parse(cc.FirstChild("t").Value) + partStartTime,
                        };
                        //TODO: 参数转换
                    }
                }

                if (newTrack.NoteList.Count > 0)
                {
                    TrackList.Add(newTrack);
                }
            }

            if (TrackList.Count > 0)
            {
                Lyric = new Lyric(this, true);
                return true;
            }
            //MessageBox.Show("The Vsqx is invalid or empty.", "Import");
            return false;
        }
        public bool ImportUst(List<string> filenames)
        {
            int trackcount = 0;
            TrackList = new List<Track>();
            foreach (string filename in filenames)
            {
                var ustValid = false;
                var ustReader = new StreamReader(filename, Encoding.GetEncoding("Shift-JIS"));
                int noteNum = 0;
                int time = 0;
                PreMeasure = 1;
                //建立音轨列表
                Track newTrack = new Track();
                newTrack.TrackNum = 0;
                newTrack.TrackName = Path.GetFileName(filename).Replace(".ust", "");
                if (trackcount == 0)
                {
                    TimeSig firsttimeSig = new TimeSig();
                    TimeSigList = new List<TimeSig>();
                    firsttimeSig.Denomi = 4;
                    firsttimeSig.Nume = 4;
                    firsttimeSig.PosMes = 0;
                    TimeSigList.Add(firsttimeSig);
                    TempoList = new List<Tempo>();
                }
                //建立音符列表，读取音符信息
                for (string readBuf = "Starting"; readBuf != "[#TRACKEND]" && readBuf != null; readBuf = ustReader.ReadLine())
                {
                    if (trackcount == 0)
                    {
                        if (readBuf.Contains("ProjectName="))
                        {
                            ProjectName = readBuf.Remove(0, 12);
                        }
                        if (readBuf.Contains("Tempo="))
                        {
                            Tempo firstTempo = new Tempo { PosTick = 0 };
                            if (double.TryParse(readBuf.Remove(0, 6), out var bpm))
                            {
                                firstTempo.BpmTimes100 = (int)(bpm * 100);
                            }
                            TempoList.Add(firstTempo);
                        }
                    }
                    if (readBuf.Contains("[#0000]"))
                    {
                        ustValid = false;
                        newTrack.NoteList = new List<Note>();
                        Note newNote = new Note
                        {
                            NoteNum = noteNum,
                            NoteIDforUTAU = readBuf.Substring(2, 4)
                        };
                        bool noteIsValid = false;
                        bool tempoTempFlag = false;
                        for (readBuf = ustReader.ReadLine(); readBuf != "[#TRACKEND]" && readBuf != null; readBuf = ustReader.ReadLine())
                        {
                            if (readBuf.Contains("[#"))
                            {
                                if (noteIsValid)
                                {
                                    newTrack.NoteList.Add(newNote);
                                    noteNum++;
                                }
                                newNote = new Note
                                {
                                    NoteNum = noteNum,
                                    NoteIDforUTAU = readBuf.Substring(2, 4)
                                };
                                noteIsValid = false;
                            }
                            if (readBuf.Contains("Length="))
                            {
                                newNote.NoteTimeOn = time;
                                time += Convert.ToInt32(readBuf.Substring(7, readBuf.Length - 7));
                                newNote.NoteTimeOff = time;
                                if (tempoTempFlag)
                                {
                                    TempoList[TempoList.Count - 1].PosTick = newNote.NoteTimeOn;
                                }
                            }
                            if (readBuf.Contains("Lyric="))
                            {
                                if (readBuf.Substring(6, readBuf.Length - 6) != "R" && readBuf.Substring(6, readBuf.Length - 6) != "r" && newNote.GetNoteLength() != 0)
                                {
                                    noteIsValid = true;
                                    ustValid = true;
                                    newNote.NoteLyric = readBuf.Substring(6, readBuf.Length - 6);
                                }
                            }
                            if (trackcount == 0)
                            {
                                if (readBuf.Contains("Tempo="))
                                {
                                    Tempo newTempo = new Tempo();
                                    try
                                    {
                                        newTempo.PosTick = newNote.NoteTimeOn;
                                    }
                                    catch
                                    {
                                        tempoTempFlag = true;
                                    }
                                    newTempo.BpmTimes100 = (int)(100 * double.Parse(readBuf.Substring(6, readBuf.Length - 6)));
                                    TempoList.Add(newTempo);
                                }
                            }
                            if (readBuf.Contains("NoteNum="))
                            {
                                newNote.NoteKey = Convert.ToInt32(readBuf.Substring(8, readBuf.Length - 8));
                            }
                        }
                    }
                }
                if (!ustValid)
                {
                    //MessageBox.Show("The Ust is invalid or empty.", "Import");
                    return false;
                }
                foreach (Note note in newTrack.NoteList)
                {
                    note.NoteTimeOn += 1920;
                    note.NoteTimeOff += 1920;
                }
                if (trackcount == 0)
                {
                    foreach (Tempo tempo in TempoList)
                    {
                        if (TempoList.IndexOf(tempo) != 0)
                        {
                            tempo.PosTick += 1920;
                        }
                    }
                }
                TrackList.Add(newTrack);
                trackcount++;
            }
            Lyric = new Lyric(this, true);
            return true;
        }
        public bool ImportCcs(List<string> filenames)
        {
            if (filenames.Count <= 0)
            {
                //MessageBox.Show("Cannot Import more than one ccs.", "Import");
                return false;
            }
            XDocument ccs = XDocument.Load(filenames[0]);
            //var nameSpace = (ccs.FirstNode as XElement)?.GetDefaultNamespace() ?? ""; //Luckily, CCS don't use the silly namespace
            PreMeasure = 1;
            //Set up tracklist
            var sequence = ccs.Descendants("Sequence").FirstOrDefault();
            var scene = sequence.FirstChild("Scene");
            var units = scene.FirstChild("Units");
            var groups = scene.FirstChild("Groups");
            ProjectName = Path.GetFileNameWithoutExtension(filenames[0]);
            var ccsValid = false;
            TrackList = new List<Track>();
            int trackNum = 0;
            var tempoFinished = false;

            foreach (var unit in units.Descendants("Unit"))
            {
                if (unit.Attribute("Category")?.Value == "SingerSong")
                {
                    if (!tempoFinished)
                    {
                        TempoList = new List<Tempo>();
                        foreach (var sound in unit.FirstChild("Tempo").Childs("Sound"))
                        {
                            Tempo newTempo = new Tempo
                            {
                                PosTick = int.Parse(sound.Attribute("Clock").Value) / 2,
                                BpmTimes100 = (int)double.Parse(sound.Attribute("Tempo").Value) * 100
                            };
                            TempoList.Add(newTempo);
                        }
                        TimeSigList = new List<TimeSig>();
                        int time = 0;
                        int mes = 0;
                        int beats = 4;
                        int beatType = 4;
                        foreach (var timeNode in unit.FirstChild("Beat").Childs("Time"))
                        {
                            var clock = int.Parse(timeNode.Attribute("Clock").Value);
                            TimeSig newtimeSig = new TimeSig
                            {
                                PosMes = mes + (clock - time) /
                                         (beats * 960 * 4 / beatType)
                            };
                            mes = newtimeSig.PosMes;
                            time = clock;
                            newtimeSig.Nume = int.Parse(timeNode.Attribute("Beats").Value);
                            beats = newtimeSig.Nume;
                            newtimeSig.Denomi = int.Parse(timeNode.Attribute("BeatType").Value);
                            beatType = newtimeSig.Denomi;
                            TimeSigList.Add(newtimeSig);
                        }
                    }

                    Track newTrack = new Track
                    {
                        TrackNum = trackNum,
                        TrackName = (from g in groups.Descendants()
                                     where g.Attribute("Id").Value == unit.Attribute("Group").Value
                                     select g.Attribute("Name").Value).FirstOrDefault(),
                        NoteList = new List<Note>()
                    };

                    int noteNum = 0;
                    foreach (var note in unit.FirstChild("Score").Childs("Note"))
                    {
                        var clock = int.Parse(note.Attribute("Clock").Value);
                        Note newNote = new Note
                        {
                            NoteNum = noteNum,
                            NoteTimeOn = clock / 2,
                            NoteTimeOff = (clock + int.Parse(note.Attribute("Duration").Value)) / 2,
                            NoteKey = int.Parse(note.Attribute("PitchStep").Value) +
                                      (int.Parse(note.Attribute("PitchOctave").Value) + 1) * 12,
                            NoteLyric = note.Attribute("Lyric").Value
                        };
                        noteNum++;
                        newTrack.NoteList.Add(newNote);
                    }
                    if (newTrack.NoteList.Count > 0)
                    {
                        TrackList.Add(newTrack);
                        trackNum++;
                        ccsValid = true;
                    }
                }
            }

            if (!ccsValid)
            {
                //MessageBox.Show("The Ccs is invalid or empty.", "Import");
                return false;
            }
            Lyric = new Lyric(this, true);
            return true;
        }

        public string ExportUst(string filepath)
        {
            string omitList = "";
            for (int tracknum = 0; tracknum < TrackList.Count; tracknum++)
            {
                string ustcontents = "[#VERSION]" + Environment.NewLine;
                ustcontents += "UST Version1.2" + Environment.NewLine;
                ustcontents += "[#SETTING]" + Environment.NewLine;
                foreach (TimeSig timesig in TimeSigList)
                {
                    if (timesig.Denomi != 4 || timesig.Nume != 4)
                    {
                        if (tracknum == 0)
                        {
                            omitList += "Meter change omitted at Measure [" + timesig.PosMes + "] : " + timesig.Nume + "/" + timesig.Denomi + Environment.NewLine;
                        }
                    }
                }
                foreach (Tempo tempo in TempoList)
                {
                    if (tempo.PosTick != 0)
                    {
                        if (tracknum == 0)
                        {
                            omitList += "Tempo change omitted at Tick [" + tempo.PosTick + "] : " + (((double)(tempo.BpmTimes100)) / 100).ToString("F2") + Environment.NewLine;
                        }
                    }
                    else
                    {
                        ustcontents += "Tempo=" + (((double)(tempo.BpmTimes100)) / 100).ToString("F2") + Environment.NewLine;
                    }
                }
                int pos = 0;
                int Rcount = 0;
                ustcontents += "Tracks=1" + Environment.NewLine;
                ustcontents += "ProjectName=" + TrackList[tracknum].TrackName + Environment.NewLine;
                ustcontents += "Mode2=True" + Environment.NewLine;
                for (int notenum = 0; notenum < TrackList[tracknum].NoteList.Count; notenum++)
                {
                    Note thisnote = TrackList[tracknum].NoteList[notenum];
                    if (pos < thisnote.NoteTimeOn)
                    {
                        ustcontents += "[#" + (notenum + Rcount).ToString("D4") + "]" + Environment.NewLine;
                        ustcontents += "Length=" + (thisnote.NoteTimeOn - pos).ToString() + Environment.NewLine;
                        ustcontents += "Lyric=R" + Environment.NewLine;
                        ustcontents += "NoteNum=60" + Environment.NewLine;
                        ustcontents += "PreUtterance=" + Environment.NewLine;
                        Rcount++;
                    }
                    ustcontents += "[#" + (notenum + Rcount).ToString("D4") + "]" + Environment.NewLine;
                    ustcontents += "Length=" + thisnote.GetNoteLength().ToString() + Environment.NewLine;
                    ustcontents += "Lyric=" + thisnote.NoteLyric + Environment.NewLine;
                    ustcontents += "NoteNum=" + thisnote.NoteKey + Environment.NewLine;
                    ustcontents += "PreUtterance=" + Environment.NewLine;
                    pos = thisnote.NoteTimeOff;
                }
                ustcontents += "[#TRACKEND]";
                File.WriteAllText(filepath + "\\" + ProjectName + "_" + tracknum.ToString() + "_" + TrackList[tracknum].TrackName.Replace("\\", "").Replace("/", "").Replace(".", "") + ".ust", ustcontents, Encoding.GetEncoding("Shift-JIS"));
            }
            return omitList;
        }
        public void ExportVsq4(string filename)
        {
            XNamespace nameSpace = Vsq4NameSpace;
            string template = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\r\n<vsq4 xmlns=\"http://www.yamaha.co.jp/vocaloid/schema/vsq4/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.yamaha.co.jp/vocaloid/schema/vsq4/ vsq4.xsd\">\r\n  <vender><![CDATA[Yamaha corporation]]></vender>\r\n  <version><![CDATA[4.0.0.3]]></version>\r\n  <vVoiceTable>\r\n    <vVoice>\r\n      <bs>0</bs>\r\n      <pc>0</pc>\r\n      <id><![CDATA[BCXDC6CZLSZHZCB4]]></id>\r\n      <name><![CDATA[VY2V3]]></name>\r\n      <vPrm>\r\n        <bre>0</bre>\r\n        <bri>0</bri>\r\n        <cle>0</cle>\r\n        <gen>0</gen>\r\n        <ope>0</ope>\r\n      </vPrm>\r\n    </vVoice>\r\n  </vVoiceTable>\r\n  <mixer>\r\n    <masterUnit>\r\n      <oDev>0</oDev>\r\n      <rLvl>0</rLvl>\r\n      <vol>0</vol>\r\n    </masterUnit>\r\n    <vsUnit>\r\n      <tNo>0</tNo>\r\n      <iGin>0</iGin>\r\n      <sLvl>-898</sLvl>\r\n      <sEnable>0</sEnable>\r\n      <m>0</m>\r\n      <s>0</s>\r\n      <pan>64</pan>\r\n      <vol>0</vol>\r\n    </vsUnit>\r\n    <monoUnit>\r\n      <iGin>0</iGin>\r\n      <sLvl>-898</sLvl>\r\n      <sEnable>0</sEnable>\r\n      <m>0</m>\r\n      <s>0</s>\r\n      <pan>64</pan>\r\n      <vol>0</vol>\r\n    </monoUnit>\r\n    <stUnit>\r\n      <iGin>0</iGin>\r\n      <m>0</m>\r\n      <s>0</s>\r\n      <vol>-129</vol>\r\n    </stUnit>\r\n  </mixer>\r\n  <masterTrack>\r\n    <seqName><![CDATA[Untitled0]]></seqName>\r\n    <comment><![CDATA[New VSQ File]]></comment>\r\n    <resolution>480</resolution>\r\n    <preMeasure>4</preMeasure>\r\n    <timeSig>\r\n      <m>0</m>\r\n      <nu>4</nu>\r\n      <de>4</de>\r\n    </timeSig>\r\n    <tempo>\r\n      <t>0</t>\r\n      <v>12000</v>\r\n    </tempo>\r\n  </masterTrack>\r\n  <vsTrack>\r\n    <tNo>0</tNo>\r\n    <name><![CDATA[Track]]></name>\r\n    <comment><![CDATA[Track]]></comment>\r\n    <vsPart>\r\n      <t>7680</t>\r\n      <playTime>61440</playTime>\r\n      <name><![CDATA[NewPart]]></name>\r\n      <comment><![CDATA[New Musical Part]]></comment>\r\n      <sPlug>\r\n        <id><![CDATA[ACA9C502-A04B-42b5-B2EB-5CEA36D16FCE]]></id>\r\n        <name><![CDATA[VOCALOID2 Compatible Style]]></name>\r\n        <version><![CDATA[3.0.0.1]]></version>\r\n      </sPlug>\r\n      <pStyle>\r\n        <v id=\"accent\">50</v>\r\n        <v id=\"bendDep\">8</v>\r\n        <v id=\"bendLen\">0</v>\r\n        <v id=\"decay\">50</v>\r\n        <v id=\"fallPort\">0</v>\r\n        <v id=\"opening\">127</v>\r\n        <v id=\"risePort\">0</v>\r\n      </pStyle>\r\n      <singer>\r\n        <t>0</t>\r\n        <bs>0</bs>\r\n        <pc>0</pc>\r\n      </singer>\r\n      <plane>0</plane>\r\n    </vsPart>\r\n  </vsTrack>\r\n  <monoTrack>\r\n  </monoTrack>\r\n  <stTrack>\r\n  </stTrack>\r\n  <aux>\r\n    <id><![CDATA[AUX_VST_HOST_CHUNK_INFO]]></id>\r\n    <content><![CDATA[VlNDSwAAAAADAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=]]></content>\r\n  </aux>\r\n</vsq4>";
            XDocument vsq = XDocument.Parse(template);
            var mixer = vsq.Descendants(nameSpace + "mixer").FirstOrDefault();
            var masterTrack = vsq.Descendants(nameSpace + "masterTrack").FirstOrDefault();
            var emptyTrack = vsq.Descendants(nameSpace + "vsTrack").FirstOrDefault();
            var emptyUnit = vsq.Descendants(nameSpace + "vsUnit").FirstOrDefault();
            var toBeRemovedTrack = emptyTrack;
            var toBeRemovedUnit = emptyUnit;
            var preMeasure = masterTrack.FirstChild("preMeasure");
            preMeasure.Value = PreMeasure.ToString();
            var firstTempo = masterTrack.FirstChild("tempo");
            firstTempo.FirstChild("v").Value = TempoList[0].BpmTimes100.ToString();
            var firsttimeSig = masterTrack.FirstChild("timeSig");
            firsttimeSig.FirstChild("nu").Value = TimeSigList[0].Nume.ToString();
            firsttimeSig.FirstChild("de").Value = TimeSigList[0].Denomi.ToString();
            if (TempoList.Count > 1)
            {
                for (int i = 1; i < TempoList.Count; i++)
                {
                    XElement newTempo = new XElement(firstTempo);
                    newTempo.FirstChild("t").Value = TempoList[i].PosTick.ToString();
                    newTempo.FirstChild("v").Value = TempoList[i].BpmTimes100.ToString();
                    firstTempo.AddAfterSelf(newTempo);
                    firstTempo = newTempo;
                }
            }
            if (TimeSigList.Count > 1)
            {
                for (int i = 1; i < TimeSigList.Count; i++)
                {
                    XElement newtimeSig = new XElement(firsttimeSig);
                    newtimeSig.FirstChild("m").Value = TimeSigList[i].PosMes.ToString();
                    newtimeSig.FirstChild("nu").Value = TimeSigList[i].Nume.ToString();
                    newtimeSig.FirstChild("de").Value = TimeSigList[i].Denomi.ToString();
                    firsttimeSig.AddAfterSelf(newtimeSig);
                    firsttimeSig = newtimeSig;
                }
            }
            if (TrackList.Count > 0)
            {
                for (int tracknum = 0; tracknum < TrackList.Count; tracknum++)
                {
                    XElement newTrack = new XElement(emptyTrack);
                    newTrack.FirstChild("tNo").Value = (tracknum).ToString();
                    newTrack.FirstChild("name").Value = TrackList[tracknum].TrackName;
                    XElement part = newTrack.FirstChild("vsPart");
                    int pos = 0;
                    int mes = 0;
                    int nume = 4;
                    int denomi = 4;
                    foreach (TimeSig timesig in TimeSigList)
                    {
                        if (timesig.PosMes > PreMeasure)
                        {
                            break;
                        }
                        pos += (timesig.PosMes - mes) * nume * 4 * 480 / denomi;
                        mes = timesig.PosMes;
                        nume = timesig.Nume;
                        denomi = timesig.Denomi;
                    }
                    pos += (PreMeasure - mes) * nume * 4 * 480 / denomi;
                    part.FirstChild("t").Value = pos.ToString();
                    int partStartTime = pos;
                    int time = 0;
                    Track thisTrack = TrackList[tracknum];
                    var lastNote = part.FirstChild("singer");

                    var defaultStyle = new XElement("nStyle",
                        new XElement("v", 50, new XAttribute("id", "accent")),
                        new XElement("v", 0, new XAttribute("id", "bendDep")),
                        new XElement("v", 0, new XAttribute("id", "bendLen")),
                        new XElement("v", 50, new XAttribute("id", "decay")),
                        new XElement("v", 0, new XAttribute("id", "fallPort")),
                        new XElement("v", 127, new XAttribute("id", "opening")),
                        new XElement("v", 0, new XAttribute("id", "risePort")),
                        new XElement("v", 0, new XAttribute("id", "vibLen")),
                        new XElement("v", 0, new XAttribute("id", "vibType"))
                        );

                    foreach (Note currentNote in thisTrack.NoteList)
                    {
                        var noteNode = new XElement("note",
                            new XElement("t", (currentNote.NoteTimeOn - partStartTime).ToString()),
                            new XElement("dur", currentNote.GetNoteLength().ToString()),
                            new XElement("n", currentNote.NoteKey.ToString()),
                            new XElement("v", 64),
                            new XElement("y", new XCData(currentNote.NoteLyric)),
                            new XElement("p", new XCData("a")),
                            new XElement(defaultStyle)
                        );
                        noteNode.ApplyNamespace(nameSpace);
                        lastNote.AddAfterSelf(noteNode);
                        lastNote = noteNode;
                        time = currentNote.NoteTimeOff;
                    }

                    part.FirstChild("playTime").Value = time.ToString();
                    emptyTrack.AddAfterSelf(newTrack);
                    emptyTrack = newTrack;
                    var newUnit = new XElement(emptyUnit);
                    newUnit.FirstChild("tNo").Value = (tracknum).ToString();
                    emptyUnit.AddAfterSelf(newUnit);
                    emptyUnit = newUnit;
                }
                toBeRemovedTrack?.Remove();
                toBeRemovedUnit?.Remove();
            }
            vsq.Save(filename);
        }
        public void ExportCcs(string filename)
        {
            string template = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Scenario Code=\"7251BC4B6168E7B2992FA620BD3E1E77\">\r\n  <Generation>\r\n    <Author Version=\"3.2.21.2\" />\r\n    <TTS Version=\"3.1.0\">\r\n      <Dictionary Version=\"1.4.0\" />\r\n      <SoundSources />\r\n    </TTS>\r\n    <SVSS Version=\"3.0.5\">\r\n      <Dictionary Version=\"1.0.0\" />\r\n      <SoundSources>\r\n        <SoundSource Version=\"1.0.0\" Id=\"XSV-JPF-W\" Name=\"緑咲 香澄\" />\r\n        <SoundSource Version=\"1.0.0\" Id=\"XSV-JPM-P\" Name=\"赤咲 湊\" />\r\n      </SoundSources>\r\n    </SVSS>\r\n  </Generation>\r\n  <Sequence Id=\"\">\r\n    <Scene Id=\"\">\r\n      <Units>\r\n        <Unit Version=\"1.0\" Id=\"\" Category=\"SingerSong\" Group=\"7de5f694-4b60-493d-b6b0-16f6b56deb1f\" StartTime=\"00:00:00\" Duration=\"00:00:02\" CastId=\"XSV-JPM-P\" Language=\"Japanese\">\r\n          <Song Version=\"1.02\">\r\n            <Tempo>\r\n              <Sound Clock=\"0\" Tempo=\"120\" />\r\n            </Tempo>\r\n            <Beat>\r\n              <Time Clock=\"0\" Beats=\"4\" BeatType=\"4\" />\r\n            </Beat>\r\n            <Score>\r\n              <Key Clock=\"0\" Fifths=\"0\" Mode=\"0\" />\r\n            </Score>\r\n          </Song>\r\n        </Unit>\r\n      </Units>\r\n      <Groups>\r\n        <Group Version=\"1.0\" Id=\"7de5f694-4b60-493d-b6b0-16f6b56deb1f\" Category=\"SingerSong\" Name=\"ソング 1\" Color=\"#FFAF1F14\" Volume=\"0\" Pan=\"0\" IsSolo=\"false\" IsMuted=\"false\" CastId=\"XSV-JPM-P\" Language=\"Japanese\" />\r\n      </Groups>\r\n      <SoundSetting Rhythm=\"4/4\" Tempo=\"78\" />\r\n    </Scene>\r\n  </Sequence>\r\n</Scenario>";
            XDocument ccs = XDocument.Parse(template);
            var scenario = ccs.Descendants("Scenario").FirstOrDefault();
            var scene = scenario.FirstChild("Sequence").FirstChild("Scene");
            var units = scene.FirstChild("Units");
            var emptyUnit = units.FirstChild("Unit");
            var groups = scene.FirstChild("Groups");
            var emptyGroup = groups.FirstChild("Group");
            var allTempo = emptyUnit.FirstChild("Song").FirstChild("Tempo");
            var firstTempo = allTempo.FirstChild("Sound");
            var toBeRemovedUnit = emptyUnit;
            var toBeRemovedGroup = emptyGroup;
            bool first = true;
            //for (int i = 1; i < TempoList.Count; i++) //not elegant
            foreach (var tempo in TempoList)
            {
                XElement newTempo = new XElement(firstTempo);
                newTempo.Attribute("Tempo").Value = (0.01 * tempo.BpmTimes100).ToString();
                if (!first)
                {
                    newTempo.Attribute("Clock").Value = (tempo.PosTick * 2).ToString();
                }
                else
                {
                    first = false;
                }
                firstTempo.AddAfterSelf(newTempo);
                firstTempo = newTempo;
            }

            var allBeat = units.FirstChild("Unit").FirstChild("Song").FirstChild("Beat");
            var firstBeat = allBeat.FirstChild("Time");
            firstBeat.Attribute("Beats").Value = TimeSigList[0].Nume.ToString();
            firstBeat.Attribute("BeatType").Value = TimeSigList[0].Denomi.ToString();
            int pos = 0;
            first = true;
            for (int i = 0; i < TimeSigList.Count; i++)
            {
                var newBeat = new XElement(firstBeat);
                if (!first)
                {
                    pos += (TimeSigList[i].PosMes - TimeSigList[i - 1].PosMes) * 960 * 4 * TimeSigList[i - 1].Nume /
                           TimeSigList[i - 1].Denomi;
                    newBeat.Attribute("Clock").Value = pos.ToString();
                }
                else
                {
                    first = false;
                }
                newBeat.Attribute("Beats").Value = TimeSigList[i].Nume.ToString();
                newBeat.Attribute("BeatType").Value = TimeSigList[i].Denomi.ToString();
                firstBeat.AddAfterSelf(newBeat);
                firstBeat = newBeat;
            }

            //Dictionary<Track, string> guids = new Dictionary<Track, string>(TrackList.Count);
            foreach (var track in TrackList)
            {
                var newUnit = new XElement(emptyUnit);
                var newGroup = new XElement(emptyGroup);
                var guid = Guid.NewGuid().ToString("D");
                //guids[track] = guid;
                //while (idList.Count <= tracknum) //原文真是蜜汁GUID构造法...
                emptyUnit.Attribute("Group").Value = guid;
                emptyGroup.Attribute("Id").Value = guid;
                emptyGroup.Attribute("Name").Value = track.TrackName;
                var song = emptyUnit.FirstChild("Song");
                var tempo = song.FirstChild("Tempo");
                var beat = song.FirstChild("Beat");
                var score = song.FirstChild("Score");
                tempo.ReplaceWith(new XElement(allTempo));
                beat.ReplaceWith(new XElement(allBeat));

                foreach (Note Note in track.NoteList)
                {
                    score.Add(new XElement("Note",
                        new XAttribute("Clock", (Note.NoteTimeOn * 2).ToString()),
                        new XAttribute("PitchStep", (Note.NoteKey % 12).ToString()),
                        new XAttribute("PitchOctave", (Note.NoteKey / 12 - 1).ToString()),
                        new XAttribute("Duration", (Note.GetNoteLength() * 2).ToString()),
                        new XAttribute("Lyric", Note.NoteLyric)
                    ));
                }
                int posTick = 0;
                int posSecond = 0;
                int lastTick = track.NoteList.LastOrDefault()?.NoteTimeOff ?? 0;
                for (int j = 1; j < TempoList.Count; j++)
                {
                    posTick = TempoList[j].PosTick;
                    posSecond += (int)((TempoList[j].PosTick - TempoList[j - 1].PosTick) / 8.0 / (0.01 * TempoList[j - 1].BpmTimes100) + 1);
                }
                if (posTick < lastTick)
                {
                    posSecond += (int)((lastTick - posTick) / 8.0 / (0.01 * (TempoList[TempoList.Count - 1].BpmTimes100)) + 1);
                }
                TimeSpan timespan = new TimeSpan(0, 0, posSecond);
                emptyUnit.Attribute("Duration").Value = timespan.ToString("c");
                units.Add(newUnit);
                groups.Add(newGroup);
                emptyUnit = newUnit;
                emptyGroup = newGroup;
            }
            toBeRemovedUnit.Remove();
            toBeRemovedGroup.Remove();
            ccs.Save(filename);
        }
    }
}

