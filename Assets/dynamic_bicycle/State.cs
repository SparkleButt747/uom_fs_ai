using UnityEngine;

public class State
{

    public double x;
    public double y;
    public double z;
    public double yaw;
    public double v_x;
    public double v_y;
    public double v_z;
    public double r_x;
    public double r_y;
    public double r_z;
    public double a_x;
    public double a_y;
    public double a_z;


    public State()
    {

    }

    public State(double x, double y, double z, double yaw, double v_x, double v_y, double v_z,
                 double r_x, double r_y, double r_z, double a_x, double a_y, double a_z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.yaw = yaw;
        this.v_x = v_x;
        this.v_y = v_y;
        this.v_z = v_z;
        this.r_x = r_x;
        this.r_y = r_y;
        this.r_z = r_z;
        this.a_x = a_x;
        this.a_y = a_y;
        this.a_z = a_z;
    }

    public static State operator *(State state, double dt)
    {
        return new State(dt * state.x, dt * state.y, dt * state.z, dt * state.yaw, dt * state.v_x, dt * state.v_y, dt * state.v_z,
                         dt * state.r_x, dt * state.r_y, dt * state.r_z, dt * state.a_x, dt * state.a_y, dt * state.a_z);
    }

    public static State operator +(State state, State x2)
    {
        return new State(state.x + x2.x, state.y + x2.y, state.z + x2.z, state.yaw + x2.yaw, state.v_x + x2.v_x, state.v_y + x2.v_y, state.v_z + x2.v_z,
                         state.r_x + x2.r_x, state.r_y + x2.r_y, state.r_z + x2.r_z, state.a_x + x2.a_x, state.a_y + x2.a_y, state.a_z + x2.a_z);
    }

    public string GetString()
    {
        string str = "x:" + x + "| y:" + y + "| z:" + z + "| yaw:" + yaw +
                     "| v_x:" + v_x + "| v_y:" + v_y + "| v_z:" + v_z +
                     "| r_x:" + r_x + "| r_y:" + r_y + "| r_z:" + r_z +
                     "| a_x:" + a_x + "| a_y:" + a_y + "| a_z:" + a_z;
        return str;
    }

}
