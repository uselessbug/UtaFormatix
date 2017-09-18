namespace UtaFormatix.Model
{
    public class Pit
    {
        public Pit() { }
        public Pit(Pit pit)
        {
            Pos = pit.Pos;
            Repeat = pit.Repeat;
            Value = pit.Value;
        }
        public int Pos;
        public int Repeat = 1;
        public double Value;
    }
}
