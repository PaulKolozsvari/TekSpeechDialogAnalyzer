namespace TekSpeech.DialogAnalyzer.Lib.Data
{
    public enum SpeechCommandType
    {
        VD, //Voice Device
        VO, //Voice Ouput
        NS, //Calibration
        VE, //SNR (Signal to Noise Ratio) e.g. "too loud" or Bad SNR
        VS //Voice Speech
    }
}
