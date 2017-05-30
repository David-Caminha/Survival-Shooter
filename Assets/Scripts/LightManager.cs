using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{

    EmotionManager emotionManager;
    double avgArousal;
    double avgValence;

    public Light ambientLight;
    public Light playerLight;

    public float calmIntensity = 0.45f;
    public float nervousIntensity = 0.28f;
    public float stressedIntensity = 0f;

    public float calmRange = 12f;
    public float nervousRange = 9f;
    public float stressedRange = 8f;

    void Start()
    {
        emotionManager = EmotionManager.Instance;
        if (emotionManager == null || !emotionManager.emotionalMechanics)
            enabled = false;
    }
    
    void Update()
    {
        avgArousal = emotionManager.AverageArousal();
        avgValence = emotionManager.AverageValence();

        if (avgArousal >= 3 && avgValence < 2)
        {
            ambientLight.intensity = Mathf.Lerp(ambientLight.intensity, stressedIntensity, Time.deltaTime / 10);
            playerLight.range = Mathf.Lerp(playerLight.range, stressedRange, Time.deltaTime / 10);
        }
        else if (avgArousal >= 2 && avgValence <= 1)
        {
            ambientLight.intensity = Mathf.Lerp(ambientLight.intensity, stressedIntensity, Time.deltaTime / 10);
            playerLight.range = Mathf.Lerp(playerLight.range, stressedRange, Time.deltaTime / 10);
        }
        else if (avgArousal >= 2 && avgValence <= 2)
        {
            ambientLight.intensity = Mathf.Lerp(ambientLight.intensity, nervousIntensity, Time.deltaTime / 10);
            playerLight.range = Mathf.Lerp(playerLight.range, nervousRange, Time.deltaTime / 10);
        }
        else if (avgArousal >= 2 && avgValence > 2)
        {
            ambientLight.intensity = Mathf.Lerp(ambientLight.intensity, calmIntensity, Time.deltaTime / 10);
            playerLight.range = Mathf.Lerp(playerLight.range, calmRange, Time.deltaTime / 10);
        }
        else if (avgArousal < 2 && avgValence >= 2)
        {
            ambientLight.intensity = Mathf.Lerp(ambientLight.intensity, calmIntensity, Time.deltaTime / 8);
            playerLight.range = Mathf.Lerp(playerLight.range, calmRange, Time.deltaTime / 8);
        }
    }
}
