using UnityEngine;

public class DynamicBicycle : VehicleModel
{

    public DynamicBicycle(string yamlFile) : base(yamlFile)
    {
    }

    public override void UpdateState(ref State state, ref Input input, double dt)
    {
        ValidateInput(ref input);

        double Fz = GetNormalForce(state);

        double slipAngleFront = GetSlipAngle(state, input, true);
        double FyF = GetFy(Fz, true, slipAngleFront);

        double slipAngleBack = GetSlipAngle(state, input, false);
        double FyR = GetFy(Fz, false, slipAngleBack);

        // Drivetrain Model
        double Fx = GetFx(state, input);
        // Dynamics
        State xDotDyn = _f(state, input, Fx, FyF, FyR);
        State xNextDyn = state + xDotDyn * dt;
        state = _fKinCorrection(xNextDyn, state, input, Fx, dt);

        // Set the acceleration based on the change in velocity
        state.a_x = xDotDyn.v_x;
        state.a_y = xDotDyn.v_y;

        ValidateState(ref state);
    }

    private State _f(State x, Input u, double Fx, double FyF, double FyR)
    {
        double FyFTot = 2.0 * FyF;
        double FyRTot = 2.0 * FyR;

        State xDot = new State();

        xDot.x = Mathf.Cos((float)x.yaw) * x.v_x - Mathf.Sin((float)x.yaw) * x.v_y;
        xDot.y = Mathf.Sin((float)x.yaw) * x.v_x + Mathf.Cos((float)x.yaw) * x.v_y;

        xDot.yaw = x.r_z;

        xDot.v_x = (x.r_z * x.v_y) + (Fx - Mathf.Sin((float)u.delta) * FyFTot) / _param.Inertia.m;
        xDot.v_y = ((Mathf.Cos((float)u.delta) * FyFTot) + FyRTot) / _param.Inertia.m - (x.r_z * x.v_x);

        xDot.r_z = (Mathf.Cos((float)u.delta) * FyFTot * _param.Kinematic.l_F - FyRTot * _param.Kinematic.l_R) / _param.Inertia.I_z;

        return xDot;
    }

    private State _fKinCorrection(State xIn, State xState, Input u, double Fx, double dt)
    {
        State x = xIn;
        double vXDot = Fx / (_param.Inertia.m);
        double v = CalculateMagnitude((float)xState.v_x, (float)xState.v_y);
        double vBlend = 0.5 * (v - 1.5);
        double blend = Mathf.Clamp01((float)vBlend);

        x.v_x = blend * x.v_x + (1.0 - blend) * (xState.v_x + dt * vXDot);

        double vY = Mathf.Tan((float)u.delta) * x.v_x * _param.Kinematic.l_R / _param.Kinematic.l;
        double r = Mathf.Tan((float)u.delta) * x.v_x / _param.Kinematic.l;

        x.v_y = blend * x.v_y + (1.0 - blend) * vY;
        x.r_z = blend * x.r_z + (1.0 - blend) * r;
        return x;
    }

    private double GetFx(State x, Input u)
    {
        double acc = x.v_x <= 0.0 && u.acc < 0.0 ? 0.0 : u.acc;
        double Fx = acc * _param.Inertia.m - GetFdrag(x);
        return Fx;
    }

    private double GetNormalForce(State x)
    {
        return _param.Inertia.g * _param.Inertia.m + GetFdown(x);
    }

    private double GetFdown(State x)
    {
        return _param.Aero.c_down * x.v_x * x.v_x;
    }

    private double GetFdrag(State x)
    {
        return _param.Aero.c_drag * x.v_x * x.v_x;
    }

    private double GetFy(double Fz, bool front, double slipAngle)
    {
        double FzAxle = front ? GetDownForceFront(Fz) : GetDownForceRear(Fz);

        double B = _param.Tire.B;
        double C = _param.Tire.C;
        double D = _param.Tire.D;
        double E = _param.Tire.E;
        double muY = D * Mathf.Sin((float)(C * Mathf.Atan((float)(B * (1.0 - E) * slipAngle + E * Mathf.Atan((float)B * (float)slipAngle)))));
        double Fy = FzAxle * muY;
        return Fy;
    }

    private double GetDownForceFront(double Fz)
    {
        double FzAxle = 0.5 * _param.Kinematic.w_front * Fz;
        return FzAxle;
    }

    private double GetDownForceRear(double Fz)
    {
        double FzAxle = 0.5 * (1.0 - _param.Kinematic.w_front) * Fz;
        return FzAxle;
    }

    public float CalculateMagnitude(float xComponent, float yComponent)
    {
        return Mathf.Sqrt(xComponent * xComponent + yComponent * yComponent);
    }

}
