using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EmotionManager : MonoBehaviour
{
    public double ArousalValue;
    public double ValenceValue;

    public bool recording;
    [SerializeField]
    private List<double> highestVal = new List<double>{ Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue }; // #[SC_h, HR_h, EMG1_h, EMG2_h]
    [SerializeField]
    private List<double> lowestVal = new List<double> { Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue }; // #[SC_l, HR_l, EMG1_l, EMG2_l]

    private List<List<double>> raw_data = new List<List<double>>();  // #[time_float, SC_raw, HR_raw, EMG1_raw, EMG2_raw]
    private List<List<double>> smoothed_data = new List<List<double>>();// #[time_float, SC_s, HR_s, EMG1_s, EMG2_s]
    private List<List<double>> normalized_data = new List<List<double>>();// #[time_float, SC_n, HR_n, EMG1_n, EMG2_n]
    private List<List<double>> classified_data = new List<List<double>>(); // #[time_float, Arousal, Valence, Flags]

    public List<List<String>> eventList = new List<List<string>>(); //[time, event]

    //HR Filtering Threshold
    double HRMaxPosDev = 1.2;
    double HRMaxNegDev = 0.8;

    //Smoothing Parameters
    int SCSmoothingWindowSize = 80; // #5 second filtering window (32sps * 5 = 160 points in the averaging mask, 80 in each direction)
    int EMGSmoothingWindowSize = 32; // #2 second filtering window (same process as above)
    int HRSmoothingWindowSize = 32; // #2 second filtering window (same process as above)

    private System.Diagnostics.Stopwatch sw;
    private DateTime initTime;
    
    public enum ESLevel
    {
        VeryLow,
        Low,
        Average,
        High,
        VeryHigh
    }

    private static EmotionManager instance;
    public static EmotionManager Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        instance = this;
        ArousalValue = 5;
        ValenceValue = 5;
        Application.runInBackground = true;
        sw = new System.Diagnostics.Stopwatch();
    }

    // Use this for initialization
    void Start()
    {
        recording = true;
        StartCoroutine(BioSensorDataPool());
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, Screen.height - 60, 80, 20), ArousalValue.ToString());
        GUI.Label(new Rect(10, Screen.height - 30, 80, 20), ValenceValue.ToString());
    }

    private IEnumerator BioSensorDataPool()
    {
        FileStream bs_fs;
        BinaryReader bs_br;

        bs_fs = new FileStream("C:/BioTrace/System/DataChan.bin", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        bs_br = new BinaryReader(bs_fs, new System.Text.ASCIIEncoding());

        //Real time readings
        double bs_GSR = -1;
        double bs_EMG1_raw = -1;
        double bs_EMG2_raw = -1;
        double bs_EMG1_20 = -1;
        double bs_EMG2_20 = -1;
        double bs_EMG1_100 = -1;
        double bs_EMG2_100 = -1;
        double bs_EMG1_20_amp = -1;
        double bs_EMG2_20_amp = -1;
        double bs_EMG1_100_amp = -1;
        double bs_EMG2_100_amp = -1;
        double bs_HR = -1;

        double prevHR = -1;

        sw.Start();
        initTime = DateTime.Now;

        while(recording)
        {
            // Ch3: EMG1   Ch4: EMG2   Ch5: SC/GSR   Ch23: HR   
            bs_br.ReadDouble();
            bs_br.ReadDouble();

            bs_EMG1_raw = bs_br.ReadDouble();
            bs_EMG2_raw = bs_br.ReadDouble();
            bs_GSR = bs_br.ReadDouble();
            for (int m = 0; m < 5; m++)
            {
                bs_br.ReadDouble();
            }
            bs_EMG1_20 = bs_br.ReadDouble();
            bs_EMG1_100 = bs_br.ReadDouble();
            bs_EMG1_20_amp = bs_br.ReadDouble();
            bs_EMG1_100_amp = bs_br.ReadDouble();

            bs_br.ReadDouble();

            bs_EMG2_20 = bs_br.ReadDouble();
            bs_EMG2_100 = bs_br.ReadDouble();
            bs_EMG2_20_amp = bs_br.ReadDouble();
            bs_EMG2_100_amp = bs_br.ReadDouble();

            bs_br.ReadDouble(); bs_br.ReadDouble(); bs_br.ReadDouble();

            bs_HR = bs_br.ReadDouble();

            bs_fs.Position = 0;

            //Pre-process Data:
            //Filter HR:
            //Debug.Log(bs_HR +">"+ prevHR * HRMaxPosDev +"||"+ bs_HR +"<"+ prevHR * HRMaxNegDev);
            if (raw_data.Count != 0 && ((bs_HR > prevHR * HRMaxPosDev) || (bs_HR < prevHR * HRMaxNegDev)))
            {
                bs_HR = prevHR;
            }
            else
            {
                if (raw_data.Count != 0 && bs_HR == 70 && Math.Abs(prevHR - bs_HR) > 10) bs_HR = prevHR;
                else prevHR = bs_HR;
            }

            raw_data.Add(new List<double> { sw.ElapsedMilliseconds / 1000.00, bs_GSR, bs_HR, bs_EMG1_raw, bs_EMG2_raw, bs_EMG1_20, bs_EMG1_100, bs_EMG1_20_amp, bs_EMG1_100_amp, bs_EMG2_20, bs_EMG2_100, bs_EMG2_20_amp, bs_EMG2_100_amp });
            smoothed_data.Add(new List<double> { sw.ElapsedMilliseconds / 1000.00, bs_GSR, bs_HR, bs_EMG1_raw, bs_EMG2_raw, bs_EMG1_20, bs_EMG1_100, bs_EMG1_20_amp, bs_EMG1_100_amp, bs_EMG2_20, bs_EMG2_100, bs_EMG2_20_amp, bs_EMG2_100_amp });

            Debug.Log("time: " + String.Format("{0:f}", sw.ElapsedMilliseconds / 1000f) + "\tHR: " + bs_HR + "\tGSR: " + bs_GSR + "\tEMG1: " + bs_EMG1_raw + "\tEMG2: " + bs_EMG2_raw + "\tEMG1_20: " + bs_EMG1_20 + "\tEMG1_100: " + bs_EMG1_100 + "\tEMG1_20_amp: " + bs_EMG1_20_amp + "\tEMG1_100_amp: " + bs_EMG1_100_amp + "\tEMG2_20: " + bs_EMG2_20 + "\tEMG2_100: " + bs_EMG2_100 + "\tEMG2_20_amp: " + bs_EMG2_20_amp + "\tEMG2_100_amp: " + bs_EMG2_100_amp);

            for(int i = 0; i < 12; i++)
            {
                if (smoothed_data[raw_data.Count - 1][i + 1] < lowestVal[i])
                    lowestVal[i] = smoothed_data[raw_data.Count - 1][i + 1];

                if (smoothed_data[raw_data.Count - 1][i + 1] > highestVal[i])
                    highestVal[i] = smoothed_data[raw_data.Count - 1][i + 1];
            }

            ////Smooth GSR:
            //if (raw_data.Count - 1 >= SCSmoothingWindowSize && raw_data.Count - 1 < (raw_data.Count - SCSmoothingWindowSize)) //Start @ a point in the data where enough data has been collected
            //{ //Stop when not enough data has been collected yet
            //    double acc = raw_data[raw_data.Count - 1][2];

            //    //for point in range(1,SCSmoothingWindowSize+1): #Get equidistant points to the window's centered sample
            //    for (int i = 1; i < SCSmoothingWindowSize + 1; i++)
            //    { //Get equidistant points to the window's centered sample
            //        acc += raw_data[raw_data.Count - 1 + i][2];
            //        acc += raw_data[raw_data.Count - 1 - i][2];
            //        smoothed_data[raw_data.Count - 1][1] = acc / ((2 * SCSmoothingWindowSize) + 1);
            //    }
            //}

            ////Smooth HR:
            //if (raw_data.Count - 1 >= HRSmoothingWindowSize && raw_data.Count - 1 < (raw_data.Count - HRSmoothingWindowSize))
            //{ //Stop when not enough data has been collected yet
            //    double acc = raw_data[raw_data.Count - 1][3];

            //    //for point in range(1,SCSmoothingWindowSize+1): #Get equidistant points to the window's centered sample
            //    for (int i = 1; i < HRSmoothingWindowSize + 1; i++)
            //    { //Get equidistant points to the window's centered sample
            //        acc += raw_data[raw_data.Count - 1 + i][3];
            //        acc += raw_data[raw_data.Count - 1 - i][3];
            //        smoothed_data[raw_data.Count - 1][2] = acc / ((2 * HRSmoothingWindowSize) + 1);
            //    }
            //}

            ////Smooth EMG1:
            //if (raw_data.Count - 1 >= EMGSmoothingWindowSize && raw_data.Count - 1 < (raw_data.Count - EMGSmoothingWindowSize))
            //{ //Stop when not enough data has been collected yet
            //    double acc = raw_data[raw_data.Count - 1][4];

            //    //for point in range(1,SCSmoothingWindowSize+1): #Get equidistant points to the window's centered sample
            //    for (int i = 1; i < EMGSmoothingWindowSize + 1; i++)
            //    { //Get equidistant points to the window's centered sample
            //        acc += raw_data[raw_data.Count - 1 + i][4];
            //        acc += raw_data[raw_data.Count - 1 - i][4];
            //        smoothed_data[raw_data.Count - 1][3] = acc / ((2 * EMGSmoothingWindowSize) + 1);
            //    }
            //}

            ////Smooth EMG2:
            //if (raw_data.Count - 1 >= EMGSmoothingWindowSize && raw_data.Count - 1 < (raw_data.Count - EMGSmoothingWindowSize))
            //{ //Stop when not enough data has been collected yet
            //    double acc = raw_data[raw_data.Count - 1][5];

            //    //for point in range(1,SCSmoothingWindowSize+1): #Get equidistant points to the window's centered sample
            //    for (int i = 1; i < EMGSmoothingWindowSize + 1; i++)
            //    { //Get equidistant points to the window's centered sample
            //        acc += raw_data[raw_data.Count - 1 + i][5];
            //        acc += raw_data[raw_data.Count - 1 - i][5];
            //        smoothed_data[raw_data.Count - 1][4] = acc / ((2 * EMGSmoothingWindowSize) + 1);
            //    }
            //}

            //Debug.Log("smoothed time: " + String.Format("{0:f}", sw.ElapsedMilliseconds / 1000f) + "\tHR: " + smoothed_data[raw_data.Count - 1][2] + "\tGSR: " + smoothed_data[raw_data.Count - 1][1] + "\tEMG1: " + smoothed_data[raw_data.Count - 1][3] + "\tEMG2: " + smoothed_data[raw_data.Count - 1][4]);


            // Normalize values
            normalized_data.Add(new List<double> { smoothed_data[raw_data.Count - 1][0], 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            // GSR
            double GSR_n = (smoothed_data[raw_data.Count - 1][1] - lowestVal[0]) / (highestVal[0] - lowestVal[0]) * 100;
            normalized_data[raw_data.Count - 1][1] = GSR_n;

            // HR
            double HR_n = (smoothed_data[raw_data.Count - 1][2] - lowestVal[1]) / (highestVal[1] - lowestVal[1]) * 100;
            normalized_data[raw_data.Count - 1][2] = HR_n;

            // EMG1_raw
            double EMG1_n = (smoothed_data[raw_data.Count - 1][3] - lowestVal[2]) / (highestVal[2] - lowestVal[2]) * 100;
            normalized_data[raw_data.Count - 1][3] = EMG1_n;

            // EMG2_raw
            double EMG2_n = (smoothed_data[raw_data.Count - 1][4] - lowestVal[3]) / (highestVal[3] - lowestVal[3]) * 100;
            normalized_data[raw_data.Count - 1][4] = EMG2_n;

            // EMG1_20
            double EMG1_20_n = (smoothed_data[raw_data.Count - 1][5] - lowestVal[4]) / (highestVal[4] - lowestVal[4]) * 100;
            normalized_data[raw_data.Count - 1][5] = EMG1_20_n;

            // EMG1_100
            double EMG1_100_n = (smoothed_data[raw_data.Count - 1][6] - lowestVal[5]) / (highestVal[5] - lowestVal[5]) * 100;
            normalized_data[raw_data.Count - 1][6] = EMG1_100_n;

            // EMG1_20_amp
            double EMG1_20_amp_n = (smoothed_data[raw_data.Count - 1][7] - lowestVal[6]) / (highestVal[6] - lowestVal[6]) * 100;
            normalized_data[raw_data.Count - 1][7] = EMG1_20_amp_n;

            // EMG1_100_amp
            double EMG1_100_amp_n = (smoothed_data[raw_data.Count - 1][8] - lowestVal[7]) / (highestVal[7] - lowestVal[7]) * 100;
            normalized_data[raw_data.Count - 1][8] = EMG1_100_amp_n;

            // EMG2_20
            double EMG2_20_n = (smoothed_data[raw_data.Count - 1][9] - lowestVal[8]) / (highestVal[8] - lowestVal[8]) * 100;
            normalized_data[raw_data.Count - 1][9] = EMG2_20_n;

            // EMG2_100
            double EMG2_100_n = (smoothed_data[raw_data.Count - 1][10] - lowestVal[9]) / (highestVal[9] - lowestVal[9]) * 100;
            normalized_data[raw_data.Count - 1][10] = EMG2_100_n;

            // EMG2_20_amp
            double EMG2_20_amp_n = (smoothed_data[raw_data.Count - 1][11] - lowestVal[10]) / (highestVal[10] - lowestVal[10]) * 100;
            normalized_data[raw_data.Count - 1][11] = EMG2_20_amp_n;

            // EMG2_100_amp
            double EMG2_100_amp_n = (smoothed_data[raw_data.Count - 1][12] - lowestVal[11]) / (highestVal[11] - lowestVal[11]) * 100;
            normalized_data[raw_data.Count - 1][12] = EMG2_100_amp_n;

            Debug.Log("Normalized time: " + String.Format("{0:f}", sw.ElapsedMilliseconds / 1000f) + "\tHR: " + normalized_data[raw_data.Count - 1][2] + "\tGSR: " + normalized_data[raw_data.Count - 1][1] + "\tEMG1_raw: " + normalized_data[raw_data.Count - 1][3] + "\tEMG2: " + normalized_data[raw_data.Count - 1][4] + "\tEMG1_20: " + normalized_data[raw_data.Count - 1][5] + "\tEMG1_100: " + normalized_data[raw_data.Count - 1][6] + "\tEMG1_20_amp: " + normalized_data[raw_data.Count - 1][7] + "\tEMG1_100_amp: " + normalized_data[raw_data.Count - 1][8] + "\tEMG2_20: " + normalized_data[raw_data.Count - 1][9] + "\tEMG2_100: " + normalized_data[raw_data.Count - 1][10] + "\tEMG2_20_amp: " + normalized_data[raw_data.Count - 1][11] + "\tEMG2_100_amp: " + normalized_data[raw_data.Count - 1][12]);

            // Calculate Arousal
            // Calculate Valence

            yield return new WaitForSeconds(0.05f);
        }
    }

    void OnApplicationQuit()
    {
        recording = false;
        //DumpAVLog();
        //DumpEventLog();
    }
}
