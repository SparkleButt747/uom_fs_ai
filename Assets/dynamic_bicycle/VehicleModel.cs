using UnityEngine;
using System;

public class VehicleModel
{

    public VehicleParam _param;

    public virtual void UpdateState(ref State state, ref Input input, double dt)
    {
        // Needs to be overriden
    }

    public VehicleModel(string yamlFilePath)
    {
        _param = new VehicleParam(yamlFilePath); // Implement VehicleParam class to parse YAML file and initialize parameters.
    }

    public void ValidateState(ref State state)
    {
        state.v_x = Mathf.Max(0.0f, (float)state.v_x); // Use Mathf.Max to ensure v_x is non-negative.
    }

    public void ValidateInput(ref Input input)
    {
        float maxAcc = (float)_param.InputRanges.acc.max;
        float minAcc = (float)_param.InputRanges.acc.min;

        float maxVel = (float)_param.InputRanges.vel.max;
        float minVel = (float)_param.InputRanges.vel.min;

        float maxDelta = (float)_param.InputRanges.delta.max;
        float minDelta = (float)_param.InputRanges.delta.min;

        input.acc = Mathf.Clamp((float)input.acc, minAcc, maxAcc); // Use Mathf.Clamp to restrict input.acc to the valid range.
        input.vel = Mathf.Clamp((float)input.vel, minVel, maxVel); // Use Mathf.Clamp to restrict input.vel to the valid range.
        input.delta = Mathf.Clamp((float)input.delta, minDelta, maxDelta); // Use Mathf.Clamp to restrict input.delta to the valid range.
    }

    public float GetSlipAngle(State x, Input u, bool isFront)
    {
        float leverArmLength = (float)_param.Kinematic.l * (float)_param.Kinematic.w_front;

        if (!isFront)
        {
            float vX = Mathf.Max(1.0f, (float)x.v_x);
            return Mathf.Atan(((float)x.v_y - leverArmLength * (float)x.r_z) / (vX - 0.5f * (float)_param.Kinematic.axle_width * (float)x.r_z));
        }

        float vXFront = Mathf.Max(1.0f, (float)x.v_x);
        return Mathf.Atan(((float)x.v_y + leverArmLength * (float)x.r_z) / (vXFront - 0.5f * (float)_param.Kinematic.axle_width * (float)x.r_z)) - (float)u.delta;
    }

    public WheelsInfo GetWheelSpeeds(State state, Input input)
    {
        float PI = Mathf.PI;
        float wheelCircumference = 2.0f * PI * (float)_param.Tire.radius;

        //Let It Be Zero For Init
        WheelsInfo wheelSpeeds = new WheelsInfo();

        wheelSpeeds.steering = (float)input.delta;

        // Calculate Wheel speeds (Assuming All - Wheel Drive)
        wheelSpeeds.lf_speed = ((float)state.v_x / wheelCircumference) * 60.0f;
        wheelSpeeds.rf_speed = ((float)state.v_x / wheelCircumference) * 60.0f;

        wheelSpeeds.lb_speed = ((float)state.v_x / wheelCircumference) * 60.0f;
        wheelSpeeds.rb_speed = ((float)state.v_x / wheelCircumference) * 60.0f;

        return wheelSpeeds;
    }

    // Implement necessary classes (State, Input, VehicleParam, and WheelSpeeds) used in VehicleModel class.
}
