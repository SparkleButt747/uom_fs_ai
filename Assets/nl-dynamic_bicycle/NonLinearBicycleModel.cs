using System;

public class NonLinearBicycleModel
{
    
    // Variables to store the state of the vehicle
    private double x;     // x-position
    private double y;     // y-position
    private double yaw;   // yaw angle (orientation)
    private double vx;    // x-velocity
    private double vy;    // y-velocity
    private double omega; // angular velocity
    private double c_a;   // aerodynamic coefficient
    private double c_r1;  // friction coefficient

    //For Constants In The Model
    public static VehicleParam _param = new VehicleParam("/Users/brndy.747/Projects/unity/fs_ai_simulation_v2/Assets/configDry.yaml");

    // Constants used in the model
    private double max_steer = _param.InputRanges.delta.max; // [rad] max steering angle
    private double L = _param.Kinematic.l;                 // [m] Wheel base of vehicle
    //private double dt = 0.1;                // Time step
    private double Lr = _param.Kinematic.b_R;        // [m] Distance from center of mass to rear axle
    private double Lf = _param.Kinematic.b_F;         // [m] Distance from center of mass to front axle
    private double Cf = _param.Inertia.C_f;      // N/rad Front tire cornering stiffness
    private double Cr = _param.Inertia.C_r;      // N/rad Rear tire cornering stiffness
    private double Iz = _param.Inertia.I_z;            // kg/m^2 Moment of inertia around the vertical axis
    private double m = _param.Inertia.m;             // kg Mass of the vehicle



    public NonLinearBicycleModel(double x = 0.0, double y = 0.0, double yaw = 0.0, double vx = 0.01, double vy = 0, double omega = 0.0)
    {
        // Constructor to initialize the state variables of the vehicle
        this.x = x;
        this.y = y;
        this.yaw = yaw;
        this.vx = vx;
        this.vy = vy;
        this.omega = omega;

        // Aerodynamic and friction coefficients
        c_a = _param.Aero.c_drag;
        c_r1 = 0.1;
    }

    public double NormalizeAngle(double angle)
    {
        // Normalize an angle to the range [-pi, pi]
        while (angle > Math.PI)
        {
            angle -= 2.0 * Math.PI;
        }

        while (angle < -Math.PI)
        {
            angle += 2.0 * Math.PI;
        }

        return angle;
    }

    public void Update(double throttle, double delta, double dt)
    {
        // Update the state of the vehicle based on the given throttle and steering angle

        // Clip the steering angle to the maximum allowed value
        if (delta > max_steer)
            delta = max_steer;
        else if (delta < -max_steer)
            delta = -max_steer;

        // Update the x and y positions based on the current velocities and yaw
        x += vx * Math.Cos(yaw) * dt - vy * Math.Sin(yaw) * dt;
        y += vx * Math.Sin(yaw) * dt + vy * Math.Cos(yaw) * dt;

        // Update the yaw angle based on the angular velocity
        yaw += omega * dt;

        // Normalize the yaw angle to the range [-pi, pi]
        yaw = NormalizeAngle(yaw);

        // Calculate lateral tire forces
        double Ffy = -Cf * Math.Atan2(((vy + Lf * omega) / vx - delta), 1.0);
        double Fry = -Cr * Math.Atan2((vy - Lr * omega) / vx, 1.0);

        // Calculate rolling resistance and aerodynamic drag
        double R_x = c_r1 * vx;
        double F_aero = c_a * Math.Pow(vx, 2);
        double F_load = F_aero + R_x;

        // Update velocities and angular velocity
        vx += (throttle - Ffy * Math.Sin(delta) / m - F_load / m + vy * omega) * dt;

        vy += (Fry / m + Ffy * Math.Cos(delta) / m - vx * omega) * dt;
        omega += (Ffy * Lf * Math.Cos(delta) - Fry * Lr) / Iz * dt;

        //Negative Velocity Bad For Model
        if (vx<0)
        {
            vx = 0;
        }


    }

    // Properties to access the state variables
    public double X { get => x; }
    public double Y { get => y; }
    public double Yaw { get => yaw; }
    public double Vx { get => vx; }
    public double Vy { get => vy; }
    public double Omega { get => omega; }


}

//Thanks
//https://github.com/DongChen06/PathTrackingBicycle/blob/master/bicyclemodel.py

