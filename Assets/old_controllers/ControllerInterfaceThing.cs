/*
  DEPRECATED
 */


using System;
using Unity.VisualScripting;
using UnityEngine;

public class ControllerInterfaceThing : MonoBehaviour
{

    private DynamicBicycle model;
    private State state;
    private Input input;
    private float accf;
    private float accb;
    private float sterl;
    private float sterr;

    private float keyDownTimeW;
    private float keyDownTimeA;
    private float keyDownTimeD;
    private float keyDownTimeS;
    private bool isKeyDownW;
    private bool isKeyDownA;
    private bool isKeyDownD;
    private bool isKeyDownS;
    private float mappedValueW;
    private float mappedValueA;
    private float mappedValueD;
    private float mappedValueS;

    public LoadTrack trackGenScript;

    private Rigidbody rb;

    //Testing Rotational Rewards
    private float b_p_rotataion = 0f;
    private float rotational_diff = 0f;

    /*
     * Player Control and logic is here...
     */


    // Start is called before the first frame update
    void Start()
    {
        model = new DynamicBicycle("Assets/model_configs/configDry.yaml");
        input = new Input(0, 0, 0);
        state = new State(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        //Get The Rigidbody obj
        rb = GetComponent<Rigidbody>();
        rb.detectCollisions = true;

        trackGenScript.GenerateTrack();
    }









    // Update is called once per frame
    void Update()
    {
        //Time Stuff
        float dt = Time.deltaTime;


        /*
        * Player Controller Logic, (Physics Model Breaks When Inputs Can't Be Mapped Onto A Smooth
        * Function That Is Differentiable) Hence The Sigmoid Mapping...
        */



        /*
        * Forward
        */

        if (UnityEngine.Input.GetKeyDown(KeyCode.W) && !isKeyDownW)
        {
            // Key pressed
            isKeyDownW = true;
            keyDownTimeW = Time.time;
        }

        if (UnityEngine.Input.GetKeyUp(KeyCode.W) && isKeyDownW)
        {
            // Key released
            isKeyDownW = false;
            keyDownTimeW = 0f;
            mappedValueW = 0f;
            accf = 0;
        }

        if (isKeyDownW)
        {
            // Calculate the key press duration while the key is pressed
            float keyDuration = Time.time - keyDownTimeW;

            // Map the key duration to a sigmoid function
            mappedValueW = SigmoidFunction(keyDuration);

            //Acc
            accf = (float)CalculateAcceleration(mappedValueW, model._param.Inertia.m, model._param.Tire.radius);
        }









        /*
        * Left
        */

        if (UnityEngine.Input.GetKeyDown(KeyCode.A) && !isKeyDownA)
        {
            // Key pressed
            isKeyDownA = true;
            keyDownTimeA = Time.time;
        }

        if (UnityEngine.Input.GetKeyUp(KeyCode.A) && isKeyDownA)
        {
            // Key released
            isKeyDownA = false;
            keyDownTimeA = 0f;
            mappedValueA = 0f;
            sterl = 0;
        }

        if (isKeyDownA)
        {
            // Calculate the key press duration while the key is pressed
            float keyDuration = Time.time - keyDownTimeA;

            // Map the key duration to a sigmoid function
            mappedValueA = SigmoidFunction(keyDuration);

            sterl = (float)CalculateSteeringAngle(mappedValueA, model._param.InputRanges.delta.max);
        }









        /*
        * Right
        */

        if (UnityEngine.Input.GetKeyDown(KeyCode.D) && !isKeyDownD)
        {
            // Key pressed
            isKeyDownD = true;
            keyDownTimeD = Time.time;
        }

        if (UnityEngine.Input.GetKeyUp(KeyCode.D) && isKeyDownD)
        {
            // Key released
            isKeyDownD = false;
            keyDownTimeD = 0f;
            mappedValueD = 0f;
            sterr = 0;
        }

        if (isKeyDownD)
        {
            // Calculate the key press duration while the key is pressed
            float keyDuration = Time.time - keyDownTimeD;

            // Map the key duration to a sigmoid function
            mappedValueD = SigmoidFunction(keyDuration);

            sterr = (float)CalculateSteeringAngle(mappedValueD * -1, model._param.InputRanges.delta.max);
        }









        /*
        * Break 
        */

        if (UnityEngine.Input.GetKeyDown(KeyCode.S) && !isKeyDownS)
        {
            // Key pressed
            isKeyDownS = true;
            keyDownTimeS = Time.time;
        }

        if (UnityEngine.Input.GetKeyUp(KeyCode.S) && isKeyDownS)
        {
            // Key released
            isKeyDownS = false;
            keyDownTimeS = 0f;
            mappedValueS = 0f;
            accb = 0;
        }

        if (isKeyDownS)
        {
            // Calculate the key press duration while the key is pressed
            float keyDuration = Time.time - keyDownTimeS;

            // Map the key duration to a sigmoid function
            mappedValueS = SigmoidFunction(keyDuration);

            accb = (float)CalculateDecceleration(mappedValueS);
        }



        //Updating And Validating Inputs And States...
        input.acc = ValidateThrottleInput(accf, accb,model._param.InputRanges.acc.max, model._param.InputRanges.acc.min);
        input.delta = ValidateSteeringInput(sterl,sterr,model._param.InputRanges.delta.max,model._param.InputRanges.delta.min);

        model.UpdateState(ref state, ref input, dt);

        transform.position = new Vector3((float)state.x, transform.position.y, (float)state.y);
        transform.eulerAngles = new Vector3(0f, -(float)RadiansToDegrees(state.yaw)+90, 0f);


        //Testing Rotational Rewards
        rotational_diff = Math.Abs((float)state.yaw - b_p_rotataion);

        //Check Rotation
        if (rotational_diff > Math.PI / 4)
        {
            Debug.Log("Wow Big Rotatation");
            b_p_rotataion = (float)state.yaw;
        }

    }









    //Car Input Validators
    //------------------------------------

    private static double CalculateAcceleration(double torqueInput, double vehicleMass, double wheelRadius)
    {

        //Max Torque Is 195
        //Input Range Is From 0 to 1
        //Therefore torqueInput*195

        // Calculate driving force (F_drive) based on torque and wheel radius
        double drivingForce = (torqueInput*195) / wheelRadius;

        // Calculate acceleration
        double acceleration = drivingForce / vehicleMass;
        return acceleration;
    }


    private static double CalculateDecceleration(double breakPressure)
    {
        //Assuming The Front And Rear Hydralic Brakes Provide A Maximum Decceleration Of -10 When Breakpressure is 1
        //Therefore deccel = breakPressure*10
        return (breakPressure * 10)*-1;
    }

    private static double CalculateSteeringAngle(double steeringInput, double sterMax)
    {
        //Assuming -1 means full left and 1 mean full right
        //From API Documentation Max Left And Right Is 21 degs

        double steeringAngleRads = sterMax * steeringInput;
        return steeringAngleRads;

    }

    public static double ValidateSteeringInput(double a, double b, double steerMax, double steerMin)
    {
        //Will make a better function later...
        double sum = a + b;

        if (sum > steerMax)
            return steerMax;
        else if (sum < steerMin)
            return steerMin;
        else
            return sum;
    }

    public static double ValidateThrottleInput(double a, double b, double accMax, double accMin)
    {

        //Will make a better function later...
        //For now max and min acceleration will be taken as input...
        //accMax is positive and accMin is negative...
        double sum = a + b;

        if (sum > accMax)
            return accMax;
        else if (sum < accMin)
            return accMin;
        else
            return sum;

    }









    //Auxillary Functions
    //----------------------------------

    private float SigmoidFunction(float x)
    {
        return 1.0f / (1.0f + 100*Mathf.Exp(-10*x));
    }

    public float CalculateMagnitude(float xComponent, float yComponent)
    {
        return Mathf.Sqrt(xComponent * xComponent + yComponent * yComponent);
    }

    public static double RadiansToDegrees(double radians)
    {
        return radians * (180.0 / Math.PI);
    }

    public static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }









    //Reset Logic
    //----------------------------------

    private void OnCollisionEnter(Collision collision)
    {
        // Set the 'isColliding' flag to true if the GameObject is colliding with another object.
        if (collision.gameObject.CompareTag("trackBoundary"))
        {
            //Reset Car Position
            transform.position = new Vector3(0, 0.6f, 0);
            transform.eulerAngles = new Vector3(0, 90, 0);

            //New Instances
            input = new Input(0, 0, 0);
            state = new State(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            //Create New Track
            trackGenScript.GenerateTrack();
        }
    }


}
