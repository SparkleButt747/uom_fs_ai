
[System.Serializable]
public class Param
{
    // Struct to hold Inertia parameters
    [System.Serializable]
    public class Inertia
    {
        public double m;
        public double g;
        public double I_z;
        public double C_f;
        public double C_r;
    }

    // Struct to hold Kinematic parameters
    [System.Serializable]
    public class Kinematic
    {
        public double l;
        public double b_F;
        public double b_R;
        public double w_front;
        public double l_F;
        public double l_R;
        public double axle_width;
    }

    // Struct to hold Tire parameters
    [System.Serializable]
    public class Tire
    {
        public double tire_coefficient;
        public double B;
        public double C;
        public double D;
        public double E;
        public double radius;
    }

    // Struct to hold Aero parameters
    [System.Serializable]
    public class Aero
    {
        public double c_down;
        public double c_drag;
    }

    // Struct to hold InputRanges parameters
    [System.Serializable]
    public class InputRanges
    {
        // Sub-struct for defining a range with min and max values
        [System.Serializable]
        public class Range
        {
            public double min;
            public double max;
        }

        public Range acc;
        public Range vel;
        public Range delta;
    }

    // Public fields to hold the respective parameters
    public Inertia inertia;
    public Kinematic kinematic;
    public Tire tire;
    public Aero aero;
    public InputRanges input_ranges;

}
