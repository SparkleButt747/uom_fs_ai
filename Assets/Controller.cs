using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using JetBrains.Annotations;

public class Controller : Agent
{
    //Model 
    private DynamicBicycle model;
    private State state;
    private Input input;

    //AI Outputs
    private float ai_s;
    private float ai_t;

    //Physics Collision
    private Rigidbody rb;
    public bool isColliding { get; private set; }

    //Distance
    private float totalDistance = 0f;
    private Vector3 prevpos = new Vector3(0,0,0);
    private Vector3 currpos = new Vector3(0,0,0);

    //Average Speed
    private float totaltime = 0f;
    private float averagespeed = 0f;

    //Rotation
    private float b_p_rotataion = 0f;
    private float rotational_diff = 0f;

    //Load Track GameObj
    public LoadTrack trackGenScript;

    //Auxillary Vars
    private int stepcount = 0;


    /*
     * AI Control and training logic is here...
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

    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void FixedUpdate()
    {
        //Increment Total Time
        totaltime += Time.deltaTime;

        //Calculate Distance
        currpos = transform.position;
        float diff = Vector3.Distance(currpos, prevpos);
        totalDistance += diff;
        prevpos = currpos;

        //Add The Distance Reward
        AddReward(diff);

        //Average Speed
        averagespeed = totalDistance / totaltime;

        //Calculate Rotational Difference 
        rotational_diff = Math.Abs((float)state.yaw - b_p_rotataion);
    }









    //Heuristics User Control
    //----------------------------------

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        //Steering
        continuousActionsOut[1] = UnityEngine.Input.GetAxis("Horizontal");

        //Throttle
        continuousActionsOut[0] = UnityEngine.Input.GetAxis("Vertical");
    }









    //Car AI Input Validators And Functions
    //------------------------------------

    private static double ValidateAndCalculateThrottle(double throttle, double wheelRadius, double vehicleMass)
    {
        //Max Torque is 195 From FS_AI API
        //Max Deccel is -10 (Assuming Break Pressure Linearly Leads To Decceleration)

        return Math.Max(0, ((throttle * 195) / wheelRadius) / vehicleMass) - (10 * Math.Max(0, -throttle));

    }

    private static double CalculateSteeringAngle(double steeringInput, double sterMax)
    {
        // -1 = full right and 1 = full left
        //From FS_AI API Documentation Max Left And Right Is 21 degs

        double steeringAngleRads = sterMax * steeringInput;
        return -steeringAngleRads;

    }









    //Auxillary Functions
    //----------------------------------

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
    private void OnCollisionEnter(Collision collision)
    {
        // Set the 'isColliding' flag to true if the GameObject is colliding with another object.
        if (collision.gameObject.CompareTag("trackBoundary"))
        {
            //Debug Info
            Debug.Log(totalDistance);
            Debug.Log(GetCumulativeReward());

            AddReward(-500);

            //Reset
            EndEpisode();

        }
    }










    //AI Controllers And Functions
    //------------------------------------

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        sensor.AddObservation(new Vector3((float)state.x, (float)state.z, (float)state.y));
        sensor.AddObservation(new Vector3((float)state.a_x, (float)state.a_z, (float)state.a_y));
        sensor.AddObservation(new Vector3((float)state.v_x,(float)state.v_z,(float) state.v_y));
        sensor.AddObservation(new Vector3((float)state.r_x, (float)state.r_z, (float)state.r_z));
        sensor.AddObservation((float)state.yaw);
        sensor.AddObservation((float)input.delta);
        sensor.AddObservation((float)input.acc);
     
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);


        //Timings
        float dt = Time.deltaTime;

        //AI Outputs
        //Get Throttle Output And Validate + Mapping To Acc
        ai_t = (float)ValidateAndCalculateThrottle(actions.ContinuousActions[0],model._param.Tire.radius,model._param.Inertia.m);
        //Get Steering Output And Validate + Mapping To Rads
        ai_s = (float)CalculateSteeringAngle(actions.ContinuousActions[1], model._param.InputRanges.delta.max);


        //Rewarding Outputs
        //Reward Confident Throttle Outputs / Actions
        AddReward((float)(-0.5 * Math.Cos(Math.PI * ai_t) + 0.5));
        //Reward Confident Steering Outputs / Actions
        AddReward((float)(-0.5 * Math.Cos(Math.PI * ai_s) + 0.5));
        //Reward Confident Combinations Of Outputs / Actions
        AddReward((float)(0.6*( (-0.5 * Math.Cos(Math.PI * ai_t) + 0.5) + (-0.5 * Math.Cos(Math.PI * ai_s) + 0.5) ) - 0.2));


        input.acc = ai_t;
        input.delta = ai_s;

        model.UpdateState(ref state, ref input, dt);


        //Update Car State In Unity
        transform.position = new Vector3((float)state.x, transform.position.y, (float)state.y);
        transform.eulerAngles = new Vector3(0f, -(float)RadiansToDegrees(state.yaw) + 90, 0f);


        //Clever Little Maths Function That Does This...
        /*
        if (averagespeed<0.3)
        {
            stepcount += 1;
        }
        else
        {
            stepcount = 0;
        }
        */
        stepcount+= (int)Math.Ceiling(0.5 * Math.Tanh(-averagespeed - 18.75) + 0.5);
        stepcount = Math.Min((int)Math.Ceiling(0.5 * Math.Tanh(-averagespeed - 18.75) + 0.5)*2000, stepcount);

        //Termination Condition
        if (stepcount>=1000)
        {
            //Print Some Stats
            Debug.Log(totalDistance);
            Debug.Log(GetCumulativeReward());

            AddReward(-500);

            EndEpisode();
        }

        AddReward(averagespeed);

        //Rotation Reward...
        if (rotational_diff>Math.PI/4)
        {
            AddReward(10);
            b_p_rotataion = (float)state.yaw;
        }

    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();

        //Reset Car Position
        transform.position = new Vector3(0,0.6f,0);
        transform.eulerAngles = new Vector3(0, 90, 0);

        //New Instances
        input = new Input(0, 0, 0);
        state = new State(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        //Generate New Track
        trackGenScript.GenerateTrack();



        //Reset Params

        //Distance Params
        totalDistance = 0f;
        prevpos = new Vector3(0, 0, 0);
        currpos = new Vector3(0, 0, 0);

        //Average Speed
        totaltime = 0f;
        averagespeed = 0f;

        //Rotational Params
        b_p_rotataion = 0f;
        rotational_diff = 0f;

        //Aux Params
        stepcount = 0;
    }









    //Reward Functions (DEPRECATED)
    //------------------------------------

    public void CheckSpeedAndReward()
    {
        AddReward((float)(1 * Math.Exp(-(Math.Pow(state.v_x - 1.8, 2)) / (2 * Math.Pow(1, 2))) * Math.Exp(2 * Math.PI * 0.15 * (state.v_x - 1.8)))-0.3f);
    }

    public void CheckSteeringAngleAndReward()
    {
        AddReward(((float)(Math.Abs(Math.Tanh(7f * ai_s) * 0.35f) - 0.2f))/4);
    }
}


