
public class WheelsInfo
{

    public float lf_speed; // Left-Front wheel speed
    public float rf_speed; // Right-Front wheel speed
    public float lb_speed; // Left-Rear wheel speed
    public float rb_speed; // Right-Rear wheel speed
    public float steering; // Steering angle

    public WheelsInfo()
    {

    }

    public WheelsInfo(float lf_speed, float rf_speed, float lb_speed, float rb_speed, float steering)
    {
        this.lf_speed = lf_speed;
        this.rf_speed = rf_speed;
        this.lb_speed = lb_speed;
        this.rb_speed = rb_speed;
        this.steering = steering;
    }

    public override string ToString()
    {
        return "LF: " + lf_speed + " | RF: " + rf_speed + " | LB: " + lb_speed + " | RB: " + rb_speed + " | Steering: " + steering;
    }
}
