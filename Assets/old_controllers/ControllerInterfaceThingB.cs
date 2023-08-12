/*
 * DEPRECATED
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;









public class ControllerInterfaceThingB : MonoBehaviour
{
    private NonLinearBicycleModel model;

    private float throttlef;
    private float throttleb;
    private float sterl;
    private float sterr;

    //Load Params
    public static VehicleParam _param = new VehicleParam("Assets/model_configs/configDry.yaml");

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

    // Start is called before the first frame update
    void Start()
    {
        model = new NonLinearBicycleModel();
    }

    // Update is called once per frame
    void Update()
    {
        //Time Stuff
        float dt = Time.deltaTime;


        //Forward
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
            throttlef = 0f;
        }

        if (isKeyDownW)
        {
            // Calculate the key press duration while the key is pressed
            float keyDuration = Time.time - keyDownTimeW;

            // Map the key duration to a sigmoid function
            mappedValueW = SigmoidFunction(keyDuration);

            //Acc
            //throttlef = (float)CalculateAcceleration(mappedValueW, _param.Inertia.m, _param.Tire.radius);
            throttlef = mappedValueW;
        }




        //Left
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
            sterl = 0f;
        }

        if (isKeyDownA)
        {
            // Calculate the key press duration while the key is pressed
            float keyDuration = Time.time - keyDownTimeA;

            // Map the key duration to a sigmoid function
            mappedValueA = SigmoidFunction(keyDuration);

            //sterl = (float)CalculateSteeringAngle(mappedValueA * 1);
            sterl = mappedValueA;
        }





        //Right
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
            sterr = 0f;
        }

        if (isKeyDownD)
        {
            // Calculate the key press duration while the key is pressed
            float keyDuration = Time.time - keyDownTimeD;

            // Map the key duration to a sigmoid function
            mappedValueD = SigmoidFunction(keyDuration);

            //sterr = (float)CalculateSteeringAngle(mappedValueD * -1);
            sterr = mappedValueD * -1;
        }





        //Break
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
            throttleb = 0f;
        }

        if (isKeyDownS)
        {
            // Calculate the key press duration while the key is pressed
            float keyDuration = Time.time - keyDownTimeS;

            // Map the key duration to a sigmoid function
            mappedValueS = SigmoidFunction(keyDuration);

            throttleb = mappedValueS*1;
        }

        model.Update(ValidateThrottleInput(throttlef,throttleb,model.Vx,_param.InputRanges.acc.max,_param.InputRanges.acc.min),AddAndClamp(sterr,sterl),dt);

        transform.position = new Vector3((float)model.X, transform.position.y, (float)model.Y);
        transform.eulerAngles = new Vector3(0, -(float)RadiansToDegrees(model.Yaw)+90, 0);

        //Debug.Log("X Velo: " + model.Vx + "Y Velo: " + model.Vy + "Yaw in Degs: " + RadiansToDegrees(model.Yaw));
        Debug.Log("Steering: " + AddAndClamp(sterr, sterl) + "Throttle: " + ValidateThrottleInput(throttlef, throttleb, model.Vx, _param.InputRanges.acc.max, _param.InputRanges.acc.min) + "X Velo: " + model.Vx);

    }

    private float SigmoidFunction(float x)
    {
        return 1.0f / (1.0f + 100 * Mathf.Exp(-10 * x));
    }

    public float CalculateMagnitude(float xComponent, float yComponent)
    {
        return Mathf.Sqrt(xComponent * xComponent + yComponent * yComponent);
    }


    public static double ValidateThrottleInput(double a, double b, double velocity, double accMax, double accMin)
    {

        if (velocity >= 0.0 && velocity <= 0.5)
        {
            return a;
        }
        else
        {
            //Will make a better function later...
            //For now max and min acceleration will be taken as input...
            //accMax is positive and accMin is negative...
            double sum = (a*accMax) + (b*accMin);

            if (sum > accMax)
                return accMax;
            else if (sum < accMin)
                return accMin;
            else
                return sum;
        }

    }


    public static double AddAndClamp(double a, double b)
    {
        double sum = a + b;

        if (sum > 1.0)
            return 1.0;
        else if (sum < -1.0)
            return -1.0;
        else
            return sum;
    }


    public static double RadiansToDegrees(double radians)
    {
        const double pi = Math.PI;
        return radians * (180.0 / pi);
    }


}
