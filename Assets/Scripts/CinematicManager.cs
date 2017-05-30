using UnityEngine;
using UnityEngine.UI;
using System.Collections;


// Hi! This script presents the overlay info for our tutorial content, linking you back to the relevant page.
public class CinematicManager : MonoBehaviour
{
    EmotionManager emotionManager;

    public Text person1Text;
    public Image person1Img;

    public Text person2Text;
    public Image person2Img;

    public string[] person1TextLines;
    public string[] person2TextLines;

    // store the GameObject which renders the overlay info
    public GameObject overlay;

	// store a reference to the audio listener in the scene, allowing for muting of the scene during the overlay
	public AudioListener mainListener;

    private bool wasP1Last = true;
    private int linesIndex = 0;


    void Awake()
	{
        emotionManager = EmotionManager.Instance;
	    ShowCinematicScreen();
        person1Text.text = person1TextLines[0];
        person2Text.text = "";
        person2Img.enabled = false;
    }

    void Update()
    {
        if(Input.anyKeyDown)
        {
            if(!wasP1Last && linesIndex < person1TextLines.Length || wasP1Last && linesIndex < person2TextLines.Length)
            {
                if (wasP1Last)
                {
                    person2Img.enabled = true;
                    wasP1Last = false;
                    person2Text.text = person2TextLines[linesIndex];
                    linesIndex++;
                }
                else
                {
                    wasP1Last = true;
                    person1Text.text = person1TextLines[linesIndex];
                }
            }
            else
            {
                StartGame();
            }
        }
    }

	// show overlay info, pausing game time, disabling the audio listener 
	// and enabling the overlay info parent game object
	public void ShowCinematicScreen()
	{
        if (emotionManager)
            emotionManager.AddEvent("Cinematic start");
		Time.timeScale = 0f;
		mainListener.enabled = false;
		overlay.SetActive (true);
	}

	// continue to play, by ensuring the preference is set correctly, the overlay is not active, 
	// and that the audio listener is enabled, and time scale is 1 (normal)
	public void StartGame()
    {
        if (emotionManager)
            emotionManager.AddEvent("Cinematic end");
        overlay.SetActive (false);
		mainListener.enabled = true;
		Time.timeScale = 1f;
	}
}
