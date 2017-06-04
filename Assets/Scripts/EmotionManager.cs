using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EmotionManager : MonoBehaviour
{
    public string playerName;
    public bool emotionalMechanics = false;
    public float tutorialTime;

    public double ArousalValue;
    public double ValenceValue;

    public ESLevel ArousalLevel; // [0-4]
    public ESLevel ValenceLevel; // [0-4]

    public bool recording;
    public List<double> highestVal; // #[SC_h, HR_h, EMG1_h, EMG2_h]
    public List<double> lowestVal; // #[SC_l, HR_l, EMG1_l, EMG2_l]

    private List<List<double>> raw_data = new List<List<double>>();  // #[time_float, SC_raw, HR_raw, EMG1_raw, EMG2_raw]
    private List<List<double>> smoothed_data = new List<List<double>>();// #[time_float, SC_s, HR_s, EMG1_s, EMG2_s]
    private List<List<double>> normalized_data = new List<List<double>>();// #[time_float, SC_n, HR_n, EMG1_n, EMG2_n]
    private List<List<double>> classified_data = new List<List<double>>(); // #[time_float, Arousal, Valence, Flags]

    public List<List<string>> eventList = new List<List<string>>(); //[time, event]

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
        Medium,
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
        highestVal = new List<double> { Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue}; // #[SC_h, HR_h, EMG1_h, EMG2_h]
        lowestVal = new List<double> { Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue}; // #[SC_l, HR_l, EMG1_l, EMG2_l]
        recording = true;
        LoadPlayerStats();
        StartCoroutine(BioSensorDataPool());
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, Screen.height - 120, 200, 20), "Avg Arousal: " + AverageArousal().ToString());
        GUI.Label(new Rect(10, Screen.height - 90, 200, 20), "Avg Valence: " + AverageValence().ToString());
        GUI.Label(new Rect(10, Screen.height - 60, 140, 20), "Arousal: " + ArousalLevel.ToString());
        GUI.Label(new Rect(10, Screen.height - 30, 140, 20), "Valence: " + ValenceLevel.ToString());
    }

    public double AverageArousal()
    {
        double avg = 0;
        for (int i = classified_data.Count - 1; i >= classified_data.Count - 10; i--)
        {
            avg += classified_data[i][1];
        }
        avg = avg / 10.0;
        return avg;
    }

    public double AverageValence()
    {
        double avg = 0;
        for (int i = classified_data.Count - 1; i >= classified_data.Count - 10; i--)
        {
            avg += classified_data[i][2];
        }
        avg = avg / 10.0;
        return avg;
    }

    private IEnumerator BioSensorDataPool()
    {
        FileStream bs_fs;
        BinaryReader bs_br;

        bs_fs = new FileStream("C:/BioTrace/System/DataChan.bin", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        bs_br = new BinaryReader(bs_fs, new System.Text.ASCIIEncoding());

        //Real time readings
        double bs_GSR = -1;
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
            bs_br.ReadDouble();
            bs_br.ReadDouble();

            bs_br.ReadDouble();
            bs_br.ReadDouble();
            bs_GSR = bs_br.ReadDouble();
            for (int m = 0; m < 5; m++)
            {
                bs_br.ReadDouble();
            }
            bs_br.ReadDouble();
            bs_br.ReadDouble();
            bs_EMG1_20_amp = bs_br.ReadDouble();
            bs_EMG1_100_amp = bs_br.ReadDouble();

            bs_br.ReadDouble();

            bs_br.ReadDouble();
            bs_br.ReadDouble();
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

            raw_data.Add(new List<double> { sw.ElapsedMilliseconds / 1000.00, bs_GSR, bs_HR, bs_EMG1_20_amp, bs_EMG1_100_amp, bs_EMG2_20_amp, bs_EMG2_100_amp });
            smoothed_data.Add(new List<double> { sw.ElapsedMilliseconds / 1000.00, bs_GSR, bs_HR, bs_EMG1_20_amp, bs_EMG1_100_amp, bs_EMG2_20_amp, bs_EMG2_100_amp });

            Debug.Log("time: " + String.Format("{0:f}", sw.ElapsedMilliseconds / 1000f) + "\tHR: " + bs_HR + "\tGSR: " + bs_GSR + "\tEMG1_20_amp: " + bs_EMG1_20_amp + "\tEMG1_100_amp: " + bs_EMG1_100_amp + "\tEMG2_20_amp: " + bs_EMG2_20_amp + "\tEMG2_100_amp: " + bs_EMG2_100_amp);

            for(int i = 0; i < 6; i++)
            {
                Debug.Log(i);
                Debug.Log(smoothed_data[raw_data.Count - 1][i + 1]);
                Debug.Log(lowestVal[i]);
                Debug.Log(highestVal[i]);
                if (smoothed_data[raw_data.Count - 1][i + 1] < lowestVal[i])
                    lowestVal[i] = smoothed_data[raw_data.Count - 1][i + 1];

                if (smoothed_data[raw_data.Count - 1][i + 1] > highestVal[i])
                    highestVal[i] = smoothed_data[raw_data.Count - 1][i + 1];
            }

            Debug.Log("after for");

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
            normalized_data.Add(new List<double> { smoothed_data[raw_data.Count - 1][0], 0, 0, 0, 0, 0, 0 });

            // GSR
            double GSR_n = (smoothed_data[raw_data.Count - 1][1] - lowestVal[0]) / (highestVal[0] - lowestVal[0]) * 100;
            normalized_data[raw_data.Count - 1][1] = GSR_n;

            // HR
            double HR_n = (smoothed_data[raw_data.Count - 1][2] - lowestVal[1]) / (highestVal[1] - lowestVal[1]) * 100;
            normalized_data[raw_data.Count - 1][2] = HR_n;

            // EMG1_20_amp
            double EMG1_20_amp_n = (smoothed_data[raw_data.Count - 1][3] - lowestVal[2]) / (highestVal[2] - lowestVal[2]) * 100;
            normalized_data[raw_data.Count - 1][3] = EMG1_20_amp_n;

            // EMG1_100_amp
            double EMG1_100_amp_n = (smoothed_data[raw_data.Count - 1][4] - lowestVal[3]) / (highestVal[3] - lowestVal[3]) * 100;
            normalized_data[raw_data.Count - 1][4] = EMG1_100_amp_n;

            // EMG2_20_amp
            double EMG2_20_amp_n = (smoothed_data[raw_data.Count - 1][5] - lowestVal[4]) / (highestVal[4] - lowestVal[4]) * 100;
            normalized_data[raw_data.Count - 1][5] = EMG2_20_amp_n;

            // EMG2_100_amp
            double EMG2_100_amp_n = (smoothed_data[raw_data.Count - 1][6] - lowestVal[5]) / (highestVal[5] - lowestVal[5]) * 100;
            normalized_data[raw_data.Count - 1][6] = EMG2_100_amp_n;

            Debug.Log("Normalized time: " + String.Format("{0:f}", sw.ElapsedMilliseconds / 1000f) + "\tHR: " + normalized_data[raw_data.Count - 1][2] + "\tGSR: " + normalized_data[raw_data.Count - 1][1] + "\tEMG1_20_amp: " + normalized_data[raw_data.Count - 1][3] + "\tEMG1_100_amp: " + normalized_data[raw_data.Count - 1][4] + "\tEMG2_20_amp: " + normalized_data[raw_data.Count - 1][5] + "\tEMG2_100_amp: " + normalized_data[raw_data.Count - 1][6]);


            ESLevel HR_level;
            ESLevel GSR_level;
            ESLevel EMG1_level; //Smiling
            ESLevel EMG2_level; //Frowning

            if (normalized_data[raw_data.Count - 1][2] < 30)
                HR_level = ESLevel.Low;
            else if (normalized_data[raw_data.Count - 1][2] < 80)
                HR_level = ESLevel.Medium;
            else
                HR_level = ESLevel.High;

            if (normalized_data[raw_data.Count - 1][1] < 25)
                GSR_level = ESLevel.VeryLow;
            else if (normalized_data[raw_data.Count - 1][1] < 50)
                GSR_level = ESLevel.Low;
            else if (normalized_data[raw_data.Count - 1][1] < 70)
                GSR_level = ESLevel.High;
            else
                GSR_level = ESLevel.VeryHigh;

            if (normalized_data[raw_data.Count - 1][3] < 7)
                EMG1_level = ESLevel.Low;
            else if (normalized_data[raw_data.Count - 1][3] < 21)
                EMG1_level = ESLevel.Medium;
            else
                EMG1_level = ESLevel.High;

            if (normalized_data[raw_data.Count - 1][5] < 6)
                EMG2_level = ESLevel.Low;
            else if (normalized_data[raw_data.Count - 1][5] < 15)
                EMG2_level = ESLevel.Medium;
            else
                EMG2_level = ESLevel.High;


            // Calculate Arousal
            if (GSR_level == ESLevel.VeryHigh)
                ArousalLevel = ESLevel.VeryHigh;
            if (GSR_level == ESLevel.High)
                ArousalLevel = ESLevel.High;
            if (GSR_level == ESLevel.Low)
                ArousalLevel = ESLevel.Low;
            if (GSR_level == ESLevel.VeryLow)
                ArousalLevel = ESLevel.VeryLow;

            if (GSR_level == ESLevel.VeryLow && HR_level == ESLevel.High)
                ArousalLevel = ESLevel.Low;
            if (GSR_level == ESLevel.VeryHigh && HR_level == ESLevel.Low)
                ArousalLevel = ESLevel.High;

            // Calculate Valence
            if (EMG2_level == ESLevel.High)
                ValenceLevel = ESLevel.VeryLow;
            if (EMG2_level == ESLevel.Medium)
                ValenceLevel = ESLevel.Low;

            if (EMG1_level == ESLevel.High)
                ValenceLevel = ESLevel.VeryHigh;
            if (EMG1_level == ESLevel.Medium)
                ValenceLevel = ESLevel.High;

            if (EMG1_level == ESLevel.Low && EMG2_level == ESLevel.Low)
                ValenceLevel = ESLevel.Medium;

            if (EMG1_level == ESLevel.High && EMG2_level == ESLevel.Low)
                ValenceLevel = ESLevel.VeryHigh;

            if (EMG1_level == ESLevel.High && EMG2_level == ESLevel.Medium)
                ValenceLevel = ESLevel.High;

            if (EMG1_level == ESLevel.Low && EMG2_level == ESLevel.High)
                ValenceLevel = ESLevel.VeryLow;

            if (EMG1_level == ESLevel.Medium && EMG2_level == ESLevel.High)
                ValenceLevel = ESLevel.Low;

            if (EMG1_level == ESLevel.Low && EMG2_level == ESLevel.Low && HR_level == ESLevel.Low)
                ValenceLevel = ESLevel.Low;
            if (EMG1_level == ESLevel.Low && EMG2_level == ESLevel.Low && HR_level == ESLevel.High)
                ValenceLevel = ESLevel.High;

            classified_data.Add(new List<double> { smoothed_data[raw_data.Count - 1][0], (int) ArousalLevel, (int) ValenceLevel });


            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    public void AddEvent(string e)
    {
        eventList.Add(new List<string> { (sw.ElapsedMilliseconds / 1000.00).ToString(), e });
    }

    public void SavePlayerStats()
    {
        string line1 = highestVal[0] + " " + highestVal[1] + " " + highestVal[2] + " " + highestVal[3];
        string line2 = lowestVal[0] + " " + lowestVal[1] + " " + lowestVal[2] + " " + lowestVal[3];
        string dir = Application.persistentDataPath + Path.DirectorySeparatorChar + playerName + Path.DirectorySeparatorChar;
        string path = dir + "playerStats.txt";
        Directory.CreateDirectory(dir);
        StreamWriter file = new StreamWriter(path);
        file.WriteLine(line1);
        file.WriteLine(line2);

        file.Close();
    }

    public void LoadPlayerStats()
    {
        string path = Application.persistentDataPath + Path.DirectorySeparatorChar + playerName + Path.DirectorySeparatorChar + "playerStats.txt";
        string line1, line2;

        if(File.Exists(path))
        {
            StreamReader file = new StreamReader(path);
            line1 = file.ReadLine();
            line2 = file.ReadLine();

            file.Close();

            string[] higher = line1.Split(null);
            string[] lower = line2.Split(null);

            double res;
            bool success;

            for (int i = 0; i < 4; i++)
            {
                success = Double.TryParse(higher[0], out res);
                if (success)
                    highestVal[i] = res;
                else
                {
                    ResetStats();
                    break;
                }
                success = Double.TryParse(lower[0], out res);
                if (success)
                    lowestVal[i] = res;
                else
                {
                    ResetStats();
                    break;
                }
            }
        }
    }

    void ResetStats()
    {
        for(int i = 0; i < highestVal.Count; i++)
        {
            highestVal[i] = Double.MinValue;
            lowestVal[i] = Double.MaxValue;
        }
    }

    void DumpNormalizedValuesLog()
    {
        string path = Application.persistentDataPath + Path.DirectorySeparatorChar + playerName + Path.DirectorySeparatorChar + "NormalizedValuesLog.txt";
        StreamWriter file = new StreamWriter(path);

        string header = "Time;GSR;HR;EMG1_20;EMG1_100;EMG2_20;EMG2_100";
        file.WriteLine(header);

        for (int i = 0; i < normalized_data.Count; i++)
        {
            string s = normalized_data[i][0].ToString() + ";" + normalized_data[i][1].ToString() + ";" + normalized_data[i][2].ToString() + ";" + normalized_data[i][3].ToString() + ";" + normalized_data[i][4].ToString() + ";" + normalized_data[i][5].ToString() + ";" + normalized_data[i][6].ToString();
            file.WriteLine(s);
        }

        file.Close();
    }

    void DumpAVLog()
    {
        string path = Application.persistentDataPath + Path.DirectorySeparatorChar + playerName + Path.DirectorySeparatorChar + "AVlog.txt";
        StreamWriter file = new StreamWriter(path);

        string header = "Time;Arousal;Valence";
        file.WriteLine(header);

        for (int i = 0; i < classified_data.Count; i++)
        {
            string s = classified_data[i][0].ToString() + ";" + classified_data[i][1].ToString() + ";" + classified_data[i][2].ToString();
            file.WriteLine(s);
        }

        file.Close();
    }

    void DumpEventLog()
    {
        string path = Application.persistentDataPath + Path.DirectorySeparatorChar + playerName + Path.DirectorySeparatorChar + "EventLog.txt";
        StreamWriter file = new StreamWriter(path);

        string header = "Time;Event";
        file.WriteLine(header);

        for (int i = 0; i < eventList.Count; i++)
        {
            string s = eventList[i][0].ToString() + ";" + eventList[i][1].ToString();
            file.WriteLine(s);
        }

        file.Close();
    }

    void OnApplicationQuit()
    {
        SavePlayerStats();
        recording = false;
        DumpNormalizedValuesLog();
        DumpAVLog();
        DumpEventLog();
    }
}
