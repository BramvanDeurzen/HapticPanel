using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behaviour_xfestival : Behaviour
{
    // Alarm components
    bool playsound = false;
    AudioSource audiosource;

    float timeLeftForInfo = 0.0f;
    float timeLeftToLogout = 0.0f;
    bool doLogout = false;
    const float autologoutTime = 1000.0f;
    const float leaveBuildingTimeOut = 3.0f;

    // Slider components
    [Tooltip("Amount of sliderss")]
    public static int amountOfSliders = 1;
    [Tooltip("Transform of the VR component that represents the slider")]
    public Transform[] sliderComponentTransform = new Transform[amountOfSliders];
    public PickandPlaceController pickandPlaceController;
    public TextMesh textScreen;

    public ButtonAnimator[] numberAnimators;
    public ButtonAnimator LoginButton;
    public ButtonAnimator LogoffButton;
    public ButtonAnimator PickButton;
    public ButtonAnimator PlaceButton;
    public ButtonAnimator DeleteButton1;
    public ButtonAnimator DeleteButton2;
    public ButtonAnimator EnterButton;

    private Vector3[] sliderStartPosition = new Vector3[amountOfSliders];
    //private string[] sliderValueScreenArray = new string[amountOfSliders];
    private int[] sliderValueArray = new int[amountOfSliders];


    private enum State
    {
        Zero,       //Complete beginning, allows trigger onentry
        Start,      //Start screen of the alarm
        Login,
        MainMenu,
        SwitchingMenu,
        CodeError,
        TurningOn,
        TurningOff,
        ZoneSelectOn,
        ZoneSelectOff,
        TurningOnAll,
        TurningOffAll,
        LeaveBuilding,
    }
    private enum Trigger
    {
        Slider,
        Button,
        Enter,
        LogOn, LogOff, Delete1, Delete2, Number, Cancel, Back, Place, Pick    //All the keys
    }

    private enum SliderDirection
    {
        Horizontal,
        Vertical
    }

    private Dictionary<string, Trigger> idmap;

    struct TriggerData
    {
        public Trigger trigger;
        public int number;
        public int index;
        public float travelSize;
        public SliderDirection sliderDirection;
    }
    private Dictionary<string, TriggerData> idmapNumber;

    string code = "";
    bool mainbuilding = false;
    bool sidebuilding = false;

    // Use this for initialization
    void Start()
    {
        idmap = new Dictionary<string, Trigger>() {
            {"Component_LogOn", Trigger.LogOn },
            {"Component_LogOff", Trigger.LogOff },
            {"Component_Delete1", Trigger.Delete1 },
            {"Component_Delete2", Trigger.Delete2 },
            {"Component_Place", Trigger.Place },
            {"Component_Pick", Trigger.Pick },
            {"Component_Enter", Trigger.Enter },
        };

        idmapNumber = new Dictionary<string, TriggerData>() {
            {"Component_0", new TriggerData(){number = 0, trigger = Trigger.Button } },
            {"Component_1", new TriggerData(){number = 1, trigger = Trigger.Button } },
            {"Component_2", new TriggerData(){number = 2, trigger = Trigger.Button } },
            {"Component_3", new TriggerData(){number = 3, trigger = Trigger.Button } },
            {"Component_4", new TriggerData(){number = 4, trigger = Trigger.Button } },
            {"Component_5", new TriggerData(){number = 5, trigger = Trigger.Button } },
            {"Component_6", new TriggerData(){number = 6, trigger = Trigger.Button } },
            {"Component_7", new TriggerData(){number = 7, trigger = Trigger.Button } },
            {"Component_8", new TriggerData(){number = 8, trigger = Trigger.Button } },
            {"Component_9", new TriggerData(){number = 9, trigger = Trigger.Button } },
            {"Slider", new TriggerData(){trigger = Trigger.Slider, travelSize = 53.0f, sliderDirection = SliderDirection.Horizontal, index = 0 } }
        };

        sliderStartPosition[0] = sliderComponentTransform[0].localPosition;

    }


    public override void onPress(string id, bool status)
    {
        if(!status)
        {
            Debug.LogError("input status is false test");
        }
        if (idmap.ContainsKey(id) && status)
        {
            HandleTriggerInput(idmap[id], true);
            Debug.LogWarning("VR onPress fired component ID " + id);
        }
        else if (idmap.ContainsKey(id)){
            HandleTriggerInput(idmap[id], false);
        }
        else if (idmapNumber.ContainsKey(id) && logOnPressed)
        {
            HandleNumberInput(idmapNumber[id].trigger, idmapNumber[id].number, status);
        }
        else
        {
            Debug.LogError("Unknown VR component ID");
        }
    }

    /* Code that handles the demo input
     * And the variables needed to make everything work
     */
    private bool logOnPressed = false;
    private bool enterPressed = false;
    private bool enterPressedToEarly = false;
    private bool codeCorrect = false;
    private bool pickAndPlaceEnabled = false;
    private string codeInput = "";
    private int currentCodeLength = 0;
    private const int INPUTLENGTH = 6;
    private string originalCode = "493720";

    void HandleTriggerInput(Trigger inputTrigger, bool status)
    {
        if (inputTrigger.Equals(Trigger.LogOn))
        {
            logOnPressed = true;
            LoginButton.AnimateButton(status);
        }
        else if(inputTrigger.Equals(Trigger.LogOff))
        {
            logOnPressed = false;
            LogoffButton.AnimateButton(status);
            ResetScene();
        }
        else if(inputTrigger.Equals(Trigger.Delete1))
        {
            DeleteButton1.AnimateButton(status);
            DeleteLastNumberFromInput();
        }
        else if (inputTrigger.Equals(Trigger.Delete2))
        {
            DeleteButton2.AnimateButton(status);
            DeleteLastNumberFromInput();
        }
        else if(inputTrigger.Equals(Trigger.Place))
        {
            PlaceButton.AnimateButton(status);
            Debug.LogError("Trigger place used, todo in code");
        }
        else if (inputTrigger.Equals(Trigger.Pick))
        {
            PickButton.AnimateButton(status);
            Debug.LogError("Trigger pick used, todo in code");
        }
        else if (inputTrigger.Equals(Trigger.Enter))
        {
            EnterButton.AnimateButton(status);
            if (codeInput.Length == INPUTLENGTH)
                CheckCode();
            else
                enterPressedToEarly = true;
        }
    }

    void HandleNumberInput(Trigger inputTrigger, int number, bool status)
    {
        numberAnimators[number].AnimateButton(status);
        // Add to code
        if(codeInput.Length < INPUTLENGTH)
        {
            codeInput += number;
            currentCodeLength++;
        }
            
        
    }

    void CheckCode()
    {
        enterPressed = true;
        currentCodeLength = 0;
        codeCorrect = codeInput.Equals(originalCode);
    }
    
    void DeleteLastNumberFromInput()
    {
        if(currentCodeLength > 0 )
        {
            codeInput = codeInput.Remove(codeInput.Length - 1);
            currentCodeLength--;
        }

    }

    // Main function that updates all the information during program operation.
    void ProcessComponentInformation()
    {
        if (logOnPressed)
        {
            if (enterPressed)
            {
                if (codeCorrect)
                {
                    if (!pickAndPlaceEnabled)
                    {
                        textScreen.text = "Code is correct, you can now control\nthe pick and place machine using the slider.";
                        EnablePickandPlace();
                    }
                    else
                    {
                        textScreen.text = "Code is correct, you can now control\nthe pick and place machine using the slider." + "\n------------------------------------------------------\nPress the logout button to restart.";
                    }
                }
                else
                {
                    textScreen.text = "Code is incorrect, try again." + "\n------------------------------------------------------\nPress the logout button to restart.";
                    codeInput = "";
                    enterPressed = false;
                }
            }
            else
            {
                textScreen.text = "Code: " + formatCodeInput() + "\n------------------------------------------------------\nPress the logout button to restart.";
            }
        }
        else
        {
            textScreen.text = "Press the login button to start!";
        }
    }
    
    string formatCodeInput()
    {
        string result = codeInput;
        for(int i = currentCodeLength; i < INPUTLENGTH; i++)
        {
            result += "_";
        }

        return result;
    }

    void EnablePickandPlace()
    {
        pickAndPlaceEnabled = true;
    }


    void ResetScene()
    {
        logOnPressed = false;
        enterPressed = false;
        codeCorrect = false;
        pickAndPlaceEnabled = false;
        codeInput = "";
        enterPressedToEarly = false;
        currentCodeLength = 0;
    }




    // Slider code
    public override void onSliderChange(string ID, int sliderValue)
    {
        if (idmapNumber.ContainsKey(ID))
        {

            Trigger idTrigger = idmapNumber[ID].trigger;
            if (idTrigger.Equals(Trigger.Slider))
            {
                int index = idmapNumber[ID].index;
                sliderValueArray[index] = sliderValue;
                //sliderValueScreenArray[index] = sliderValueArray[index].ToString();

                // transform the slider components to simulate movement.
                // Calculate travelFactor according to travelSize of the slider
                float travelFactor = idmapNumber[ID].travelSize / 255;
                // Calculate position platform should be in.
                // Scale from mm to meter
                float position = (sliderValue * travelFactor) / 1000;
                Vector3 newPosition = sliderStartPosition[index];
                if (idmapNumber[ID].sliderDirection.Equals(SliderDirection.Horizontal))
                {
                    // Change in X value
                    newPosition.x -= position;
                }
                else if (idmapNumber[ID].sliderDirection.Equals(SliderDirection.Vertical))
                {
                    // Change in Y value
                    newPosition.y -= position;
                }
                sliderComponentTransform[index].localPosition = newPosition;

                // Update the pickandplace according to the value from the slider
                // Transform travelfactor to percentage:
                if (pickAndPlaceEnabled)
                {
                    float percentage = sliderValue / 255.0f;
                    pickandPlaceController.MoveArmToPosPercentage(percentage);
                }

            }
        }
    }

    void Update()
    {
        // Handle the update from all the necesarry components in the scene.
        ProcessComponentInformation();
    }
}
