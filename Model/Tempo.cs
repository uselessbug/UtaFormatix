namespace UtaFormatix.Model
{
    public class Tempo
    {
        public Tempo() { }
        public Tempo(Tempo tempo)
        {
            PosTick = tempo.PosTick;
            BpmTimes100 = tempo.BpmTimes100;
        }
        public int PosTick;
        public int BpmTimes100;
    }
}
