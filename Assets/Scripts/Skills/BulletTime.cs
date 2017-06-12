using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BulletTime : MonoBehaviour
{

    public float maxEnergy = 4f;
    public float startingEnergy = 4f;
    public Slider energySlider;

    EmotionManager emotionManager;
    public float currentEnergy = 0;
    public bool regen = false;
    bool bulletTime = false;
    int state;

    void Start()
    {
        state = 0;
        emotionManager = EmotionManager.Instance;
        currentEnergy = startingEnergy;
    }

    // Update is called once per frame
    void Update()
    {
        energySlider.value = currentEnergy;
        switch (state)
        {
            case 0:
                if (emotionManager.holdingBreath && currentEnergy > 0)
                {
                    Time.timeScale = 0.5f;
                    regen = false;
                    CancelInvoke();
                    state = 1;
                }
                else if (regen && currentEnergy < maxEnergy)
                {
                    if (emotionManager.emotionalMechanics && emotionManager.AverageArousal() > 3 && emotionManager.AverageValence() < 2)
                        currentEnergy += Time.unscaledDeltaTime;
                    else if (emotionManager.emotionalMechanics && emotionManager.AverageArousal() > 2 && emotionManager.AverageValence() < 2)
                        currentEnergy += Time.unscaledDeltaTime * 0.5f;
                    else if (emotionManager.emotionalMechanics && emotionManager.AverageValence() > 2)
                        currentEnergy += Time.unscaledDeltaTime * 0.2f;
                    else
                        currentEnergy += Time.unscaledDeltaTime * 0.2f;
                }
                break;
            case 1:
                currentEnergy -= Time.unscaledDeltaTime;
                if (!emotionManager.holdingBreath || currentEnergy <= 0)
                {
                    Time.timeScale = 1;
                    Invoke("Regen", 4f);
                    state = 0;
                }
                break;
        }
        //DEBUGGING
        //if (Input.GetKeyDown(KeyCode.Space) && currentEnergy > 0)
        //{
        //    bulletTime = true;
        //    Time.timeScale = 0.4f;
        //    regen = false;
        //    CancelInvoke();
        //}
        //else if (Input.GetKeyUp(KeyCode.Space) || currentEnergy <= 0)
        //{
        //    bulletTime = false;
        //    Time.timeScale = 1;
        //    Invoke("Regen", 10f);
        //}
    }

    public void Regen()
    {
        regen = true;
    }
}
