using System;

[Serializable]
public class PathConfig
{
    public double Seed { get; set; }
    public double MinCornerRadius { get; set; }
    public int MaxFrequency { get; set; }
    public double Amplitude { get; set; }
    public bool CheckSelfIntersection { get; set; }
    public double StartingAmplitude { get; set; }
    public double RelativeAccuracy { get; set; }
    public double Margin { get; set; }
    public double StartingStraightLength { get; set; }
    public int StartingStraightDownsample { get; set; }
    public double MinConeSpacing { get; set; }
    public double MaxConeSpacing { get; set; }
    public double TrackWidth { get; set; }
    public double ConeSpacingBias { get; set; }
    public double StartingConeSpacing { get; set; }
    public int Resolution { get; private set; }
    public double Length { get; private set; }

    // Default constructor with default values
    public PathConfig()
    {
        Seed = new Random().NextDouble();
        MinCornerRadius = 3;
        MaxFrequency = 2;
        Amplitude = 1.0/3.0;
        CheckSelfIntersection = true;
        StartingAmplitude = 0.4;
        RelativeAccuracy = 0.005;
        Margin = 0;
        StartingStraightLength = 6.0;
        StartingStraightDownsample = 2;
        MinConeSpacing = 3.0 * Math.PI / 16.0;
        MaxConeSpacing = 0.6;
        TrackWidth = 5.0;
        ConeSpacingBias = 1;
        StartingConeSpacing = 2.5;
        CalculateResolutionAndLength();
    }

    // Custom constructor to set specific fields
    public PathConfig(double seed, double minCornerRadius, int maxFrequency,
                      double amplitude, bool checkSelfIntersection,
                      double startingAmplitude, double relAccuracy,
                      double margin, double startingStraightLength,
                      int startingStraightDownsample, double minConeSpacing,
                      double maxConeSpacing, double trackWidth,
                      double coneSpacingBias, double startingConeSpacing)
    {
        Seed = seed;
        MinCornerRadius = minCornerRadius;
        MaxFrequency = maxFrequency;
        Amplitude = amplitude;
        CheckSelfIntersection = checkSelfIntersection;
        StartingAmplitude = startingAmplitude;
        RelativeAccuracy = relAccuracy;
        Margin = margin;
        StartingStraightLength = startingStraightLength;
        StartingStraightDownsample = startingStraightDownsample;
        MinConeSpacing = minConeSpacing;
        MaxConeSpacing = maxConeSpacing;
        TrackWidth = trackWidth;
        ConeSpacingBias = coneSpacingBias;
        StartingConeSpacing = startingConeSpacing;
        CalculateResolutionAndLength();
    }

    private void CalculateResolutionAndLength()
    {

        double length;
        
        // This formula for approximating the maximum length was derived experimentally
        double t = Amplitude * MaxFrequency;
        length = ((0.6387 * t + 43.86) * t + 123.1) * t + 35.9;
        Length = length;

        double minSep = MinConeSpacing;
        double maxSep = MaxConeSpacing;
        double r = Math.Log(length,2) / MinCornerRadius;
        Resolution = (int)(4 * length * Math.Max(1 / minSep, r / maxSep));
        
    }
}
