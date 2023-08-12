using UnityEngine;

public class Input
{

    public double acc;
    public double vel;
    public double delta;

    public Input(double acceleration, double velocity, double steeringAngle)
    {
        acc = acceleration;
        vel = velocity;
        delta = steeringAngle;
    }

    public override string ToString()
    {
        return "acc: " + acc + " | vel: " + vel + " | delta: " + delta;
    }
}
