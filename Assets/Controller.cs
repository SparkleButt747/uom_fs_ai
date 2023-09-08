using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Controller : Agent
{
    //Model 
    private DynamicBicycle model;
    private State state;
    private Input input;
    private Rigidbody rb;

    //AI Outputs
    private float ai_s;
    private float ai_t;

    //Physics Collision
    public bool isColliding { get; private set; }

    //Distance
    private float totalDistance = 0f;
    private Vector3 prevpos = new Vector3(0,0,0);
    private Vector3 currpos = new Vector3(0,0,0);

    //Average Speed
    private float totaltime = 0f;
    private float averagespeed = 0f;

    //Cone Models
    public GameObject blue_n_white_prefab;
    public GameObject yellow_n_black_prefab;

    //Aux Vars
    private int stepcount;
    private int lvl = 1;

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

    private static Vector3[] ConvertToVectorTwoFromComplex_Arrays(System.Numerics.Complex[] complex)
    {
        Vector3[] vector = new Vector3[complex.Length];
        for (int t = 0; t < complex.Length; t++)
        {
            vector[t] = new Vector3((float)complex[t].Real, 0f,(float)complex[t].Imaginary);
        }
        return vector;
    }









    //Reset Logic
    private void OnCollisionEnter(Collision collision)
    {
        // Set the 'isColliding' flag to true if the GameObject is colliding with another object.
        if (collision.gameObject.CompareTag("trackBoundary"))
        {
            
            AddReward(-250);

            //Debug Info
            Debug.Log("TR: " + GetCumulativeReward());

            //Reset
            stepcount++;
            EndEpisode();

        }
    }









    //Checkpoints
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("checkpoints_"))
        {
            AddReward(25);
        }
    }










    //AI Controllers And Functions
    //------------------------------------

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        //Calculations Moved To Observation Step Will Speed Up The Game

        //Increment Total Time
        totaltime += Time.deltaTime;

        //Calculate Distance
        currpos = transform.position;
        float diff = Vector3.Distance(currpos, prevpos);
        totalDistance += diff;
        prevpos = currpos;

        //Average Speed Reward
        averagespeed = totalDistance / totaltime;
        AddReward(averagespeed/7.5f);

        //Rotational Reward
        AddReward(Math.Abs((float)state.r_z) / 2);

        //Negative Reward For Remaining Stationary...
        AddReward((float)(Math.Floor(0.5 * Math.Tanh(state.v_x + 18.5) + 0.5) - 1));

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

        input.acc = ai_t;
        input.delta = ai_s;

        model.UpdateState(ref state, ref input, dt);

        //Update Car State In Unity
        transform.position = new Vector3((float)state.x, transform.position.y, (float)state.y);
        transform.eulerAngles = new Vector3(0f, -(float)RadiansToDegrees(state.yaw) + 90, 0f);

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

        //Generate New Track And Checkpoints
        GeneratePathAndCheckpoints();


        //Reset Params

        //Distance Params
        totalDistance = 0f;
        prevpos = new Vector3(0, 0, 0);
        currpos = new Vector3(0, 0, 0);

        //Average Speed
        totaltime = 0f;
        averagespeed = 0f;

    }









    //Generate Path
    //------------------------------------

    public void GeneratePathAndCheckpoints()
    {
        //Check Whether A Track Already Exists And Remove The Cones And Old Checkpoints
        RemoveObjectsWithTag("trackBoundary");
        RemoveObjectsWithTag("checkpoints_");

        //Config Obj
        PathConfig config = new PathConfig();

        //Cirrculum
        if (stepcount>1000000*lvl)
        {
            lvl++;
        }

        config.MaxFrequency = lvl + 1;

        //PathGen Obj
        PathGeneration path = new PathGeneration(config);

        //Cone Density
        int cone_d = 16;

        System.Numerics.Complex[] points;
        System.Numerics.Complex[] normals;
        double[] cornerRadii;

        System.Numerics.Complex[] startcones;
        System.Numerics.Complex[] lcones;
        System.Numerics.Complex[] rcones;

        (points, normals, cornerRadii) = path.GeneratePathWithParams(config.MaxFrequency*cone_d);
        (startcones, lcones, rcones) = path.PlaceCones(points, normals, cornerRadii);

        //Checkpoint Array Len
        int cp_len;

        //Since L Cones And R Cone Are Arrays OF Different Sizes
        if (lcones.Length <= rcones.Length)
        {
            cp_len = lcones.Length;
        }
        else
        {
            cp_len = rcones.Length;
        }

        Vector3[] center_line = new Vector3[cp_len];

        for (int i = 0; i < lcones.Length; i++)
        {
            float x_l = (float)lcones[i].Real;
            float z_l = (float)lcones[i].Imaginary;
            Vector3 position_l = new Vector3(x_l, 0.1f, z_l);

            GameObject blue_n_white = Instantiate(blue_n_white_prefab);
            blue_n_white.tag = "trackBoundary";

            blue_n_white.AddComponent<MeshFilter>();

            blue_n_white.AddComponent<MeshRenderer>();

            blue_n_white.AddComponent<BoxCollider>();
            blue_n_white.GetComponent<BoxCollider>().size = new Vector3(0.15f, 0.15f, 0.4f);
            blue_n_white.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, 0.2f);

            blue_n_white.AddComponent<Rigidbody>();

            blue_n_white.transform.position = position_l;
            blue_n_white.transform.Rotate(-90, 0, 0);
            blue_n_white.transform.localScale = Vector3.one * 2f;
        }

        for (int i = 0; i < rcones.Length; i++)
        {
            float x_r = (float)rcones[i].Real;
            float z_r = (float)rcones[i].Imaginary;
            Vector3 position_r = new Vector3(x_r, 0.1f, z_r);

            GameObject yellow_n_black = Instantiate(yellow_n_black_prefab);
            yellow_n_black.tag = "trackBoundary";

            yellow_n_black.AddComponent<MeshFilter>();

            yellow_n_black.AddComponent<MeshRenderer>();

            yellow_n_black.AddComponent<BoxCollider>();
            yellow_n_black.GetComponent<BoxCollider>().size = new Vector3(0.15f, 0.15f, 0.4f);
            yellow_n_black.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, 0.2f);

            yellow_n_black.AddComponent<Rigidbody>();

            yellow_n_black.transform.position = position_r;
            yellow_n_black.transform.Rotate(-90, 0, 0);
            yellow_n_black.transform.localScale = Vector3.one * 2f;
        }

        for (int i = 0; i < cp_len; i++)
        {
            float x = ((float)lcones[i].Real + (float)rcones[i].Real) / 2;
            float z = ((float)lcones[i].Imaginary + (float)rcones[i].Imaginary) / 2;
            Vector3 position = new Vector3(x, 0, z);

            GameObject sphere_cp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere_cp.tag = "checkpoints_";
            sphere_cp.layer = LayerMask.NameToLayer("checkpoints_");

            sphere_cp.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            sphere_cp.transform.position = position;

            // Disable the collider of the sphere
            Collider sphereCollider = sphere_cp.GetComponent<Collider>();
            if (sphereCollider != null)
            {
                //sphereCollider.enabled = false;
                sphereCollider.isTrigger = true; // Set the collider as a trigger
            }

            Color sphere_pColor = Color.blue;
            Renderer sphere_pRenderer = sphere_cp.GetComponent<Renderer>();
            sphere_pRenderer.material.color = sphere_pColor;
        }

        GenerateLine(ConvertToVectorTwoFromComplex_Arrays(lcones));
        GenerateLine(ConvertToVectorTwoFromComplex_Arrays(rcones));

        void GenerateLine(Vector3[] positions)
        {
            string boundaryTag = "trackBoundary";
            string boundaryLayer = "trackEdges";

            for (int i = 1; i < positions.Length; i++)
            {
                Vector3 start = positions[i - 1];
                Vector3 end = positions[i];
                Vector3 mid = (start + end) / 2;
                Vector3 dir = end - start;
                float length = dir.magnitude;

                GameObject lineSegment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lineSegment.transform.position = mid + new Vector3(0,0.95f,0);
                lineSegment.tag = boundaryTag;
                lineSegment.layer = LayerMask.NameToLayer(boundaryLayer);

                // Set cylinder's up direction to match the direction between points
                lineSegment.transform.up = dir.normalized;

                // Rotate the cylinder to align it with the desired axis
                lineSegment.transform.rotation *= Quaternion.Euler(0f, 0f, 0f);

                // Adjust the scale to match the length of the line
                lineSegment.transform.localScale = new Vector3(0.1f, length / 2, 0.1f);

                // Attach a BoxCollider for collision detection
                BoxCollider collider = lineSegment.AddComponent<BoxCollider>();
                collider.size = new Vector3(1f, length, 1f);
                collider.isTrigger = true;  // Set the collider as a trigger

                // Attach Rigidbody for collision detection (if needed)
                Rigidbody rb = lineSegment.AddComponent<Rigidbody>();
                rb.isKinematic = true;

                // You can also set other properties like material, color, etc.
                Renderer lineSegment_renderer = lineSegment.GetComponent<Renderer>();
                Color color = Color.red;
                lineSegment_renderer.material.color = color;
            }

            //Starting Walls (Last Position To The First Position)

            Vector3 start_f = positions[positions.Length - 1];
            Vector3 end_f = positions[0];
            Vector3 mid_f = (start_f + end_f) / 2;
            Vector3 dir_f = end_f - start_f;
            float length_f = dir_f.magnitude;

            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.position = mid_f + new Vector3(0,0.75f,0);
            box.transform.localScale = new Vector3(length_f/1.5f, 1.5f, 0.25f);
            box.tag = boundaryTag;
        }
    }



    public void RemoveObjectsWithTag(string tagToRemove)
    {
        // Find all game objects with the specified tag
        GameObject[] objectsToRemove = GameObject.FindGameObjectsWithTag(tagToRemove);

        // Destroy each game object
        foreach (GameObject obj in objectsToRemove)
        {
            Destroy(obj);
        }
    }

}


