
using System.Collections.Generic;
using System.Linq;
using System;
using System.Numerics;


public class PathGeneration
{
    public PathConfig config;

    public PathGeneration(PathConfig config)
    {
        this.config = new PathConfig(config.Seed, config.MinCornerRadius , config.MaxFrequency, config.Amplitude,
            config.CheckSelfIntersection, config.StartingAmplitude, config.RelativeAccuracy, config.Margin, config.StartingStraightLength
            , config.StartingStraightDownsample, config.MinConeSpacing, config.MaxConeSpacing, config.TrackWidth, config.ConeSpacingBias,config.StartingConeSpacing);

    }


    public PathGeneration()
    {
        config = new PathConfig();

    }


    private static double[] ComputeCornerRadii(double dt, Complex[] dPdt)
    {
        Complex[] ddPdt = new Complex[dPdt.Length];
        for (int i = 0; i < dPdt.Length - 1; i++)
        {
            ddPdt[i] = (dPdt[i + 1] - dPdt[i]) / dt;
        }
        ddPdt[dPdt.Length - 1] = (dPdt[0] - dPdt[dPdt.Length - 1]) / dt;

        double[] cornerRadii = new double[dPdt.Length];
        for (int i = 0; i < dPdt.Length; i++)
        {
            cornerRadii[i] = Math.Pow(Complex.Abs(dPdt[i]), 3) / (Complex.Conjugate(dPdt[i]) * ddPdt[i]).Imaginary;
        }
        return cornerRadii;
    }

    public (Complex[] points, Complex[] normals, double[] cornerRadii) GeneratePathWithParams(int nPoints)
    {
        //Take Values From Config When Possible
        double minCornerRadius = config.MinCornerRadius;
        int maxFrequency = config.MaxFrequency;
        double amplitude = config.Amplitude;

        //Random Obj
        Random rng = new Random();

        Complex[] z = SampleUnitCircle(nPoints);

        Complex[] SampleUnitCircle(int nPoints)
        {
            Complex[] z = new Complex[nPoints];
            for (int t = 0; t < nPoints; t++)
            {
                double angle = 2 * Math.PI * t / nPoints;
                z[t] = Complex.Exp(Complex.ImaginaryOne * angle);
            }
            return z;
        }

        Complex[] waves = new Complex[nPoints];
        Complex[] dwaves = new Complex[nPoints];
        Complex[] zPow = new Complex[nPoints];

        for (int frequency = 2; frequency <= maxFrequency; frequency++)
        {
            Complex phase = Complex.Exp(Complex.ImaginaryOne * 2 * Math.PI * rng.NextDouble());

            for (int t = 0; t < nPoints; t++)
            {
                zPow[t] = Complex.Pow(z[t], frequency);
            }
            for (int t = 0; t < nPoints; t++)
            {
                waves[t] += z[t] * (zPow[t] / (phase * (frequency + 1)) + phase / (zPow[t] * (frequency - 1)));
                dwaves[t] += (zPow[t] / phase) - (phase / zPow[t]);
            }
        }

        Complex[] dPdt = new Complex[nPoints];
        Complex[] normals = new Complex[nPoints];


        Complex[] ScaleComplexArray(Complex[] inputArray, double amplitude)
        {
            Complex[] scaledArray = new Complex[inputArray.Length];

            for (int i = 0; i < inputArray.Length; i++)
            {
                scaledArray[i] = inputArray[i] * amplitude;
            }

            return scaledArray;
        }

        Complex[] AddComplexArrays(Complex[] array1, Complex[] array2)
        {
            if (array1.Length != array2.Length)
            {
                throw new ArgumentException("Arrays must have the same length.");
            }

            Complex[] sumArray = new Complex[array1.Length];

            for (int i = 0; i < array1.Length; i++)
            {
                sumArray[i] = array1[i] + array2[i];
            }

            return sumArray;
        }


        Complex[] scaledwaves = ScaleComplexArray(waves, amplitude);
        Complex[] points = AddComplexArrays(z, scaledwaves);

        for (int t = 0; t < nPoints; t++)
        {
            dPdt[t] = (Complex.ImaginaryOne * z[t]) * (1 + amplitude * dwaves[t]);
        }

        for (int t = 0; t < nPoints; t++)
        {
            normals[t] = (Complex.ImaginaryOne * dPdt[t]) / Complex.Abs(dPdt[t]);
        }

        double[] cornerRadii = ComputeCornerRadii(2 * Math.PI / nPoints, dPdt);

        double minValue = double.MaxValue;
        foreach (double radius in cornerRadii)
        {
            double absoluteValue = Math.Abs(radius);
            if (absoluteValue < minValue)
            {
                minValue = absoluteValue;
            }
        }

        double scale = minCornerRadius / minValue;

        //Scaled Vars
        Complex[] scaledPoints = new Complex[nPoints];
        double[] scaledCornerRadii = new double[nPoints];

        for (int t = 0; t<nPoints; t++)
        {
            scaledPoints[t] = new Complex(points[t].Real*scale, points[t].Imaginary*scale);
            scaledCornerRadii[t] = cornerRadii[t] * scale;
        }

        return (scaledPoints, normals, scaledCornerRadii);
    }

    //Havent Debugged... (Infinite Loop)
    public (Complex[], Complex[], double[]) GeneratePathWithLength(int n_points, double target_track_length)
    {
        //Random Obj
        Random rng = new Random();

        //Take Values From Config When Possible
        double min_corner_radius = config.MinCornerRadius;
        double starting_amplitude = config.StartingAmplitude;
        double rel_accuracy = config.RelativeAccuracy;
        double margin = config.Margin;

        Complex[] z = new Complex[n_points];
        for (int t = 0; t < n_points; t++)
        {
            double angle = 2 * Math.PI * t / n_points;
            z[t] = new Complex(Math.Cos(angle), Math.Sin(angle));
        }

        Complex[] waves = new Complex[n_points];
        Complex[] dwaves = new Complex[n_points];

        int frequency = 1;
        double amplitude = starting_amplitude;

        double trackLength;

        double scale;
        Vector2[] points;
        Complex[] dPdt;

        Vector2[] pathPoints;
        Complex[] normals;
        double[] cornerRadii;

        while (true)
        {
            while (true)
            {
                frequency += 1;
                Complex phase = Complex.Exp(new Complex(0, 2 * Math.PI * rng.NextDouble()));
                Complex[] zPow = new Complex[n_points];
                for (int t = 0; t < n_points; t++)
                {
                    zPow[t] = Complex.Pow(z[t], frequency);
                }
                for (int t = 0; t < n_points; t++)
                {
                    waves[t] += z[t] * (zPow[t] / (phase * (frequency + 1)) + phase / (zPow[t] * (frequency - 1)));
                    dwaves[t] += zPow[t] / phase - phase / zPow[t];
                }

                points = new Vector2[n_points];
                dPdt = new Complex[n_points];
                for (int t = 0; t < n_points; t++)
                {
                    points[t] = new Vector2((float)z[t].Real, (float)z[t].Imaginary) + new Vector2((float)waves[t].Real, (float)waves[t].Imaginary) * (float)amplitude;
                    dPdt[t] = Complex.ImaginaryOne * z[t] * (1 + amplitude * dwaves[t]);
                }
                cornerRadii = ComputeCornerRadii(2 * Math.PI / n_points, dPdt);

                scale = min_corner_radius / cornerRadii.Min();
                trackLength = 0;
                for (int t = 1; t < n_points; t++)
                {
                    trackLength += Vector2.Distance(points[t - 1], points[t]);
                }
                trackLength *= scale;

                if (trackLength >= target_track_length)
                {
                    break;
                }
            }

            double upperAmp = amplitude;
            double lowerAmp = 0;
            double upperOffset = trackLength - target_track_length;
            double lowerOffset = 2 * Math.PI * min_corner_radius - target_track_length;

            while (Math.Abs(trackLength - target_track_length) / target_track_length > rel_accuracy)
            {
                amplitude = ((lowerOffset * upperAmp - upperOffset * lowerAmp) / (lowerOffset - upperOffset));

                points = new Vector2[n_points];
                dPdt = new Complex[n_points];
                for (int t = 0; t < n_points; t++)
                {
                    points[t] = new Vector2((float)z[t].Real, (float)z[t].Imaginary) + new Vector2((float)(amplitude * waves[t].Real), (float)(amplitude * waves[t].Imaginary));
                    dPdt[t] = Complex.ImaginaryOne * z[t] * (1 + amplitude * dwaves[t]);
                }

                cornerRadii = ComputeCornerRadii(2 * Math.PI / n_points, dPdt);

                scale = min_corner_radius / cornerRadii.Min();
                trackLength = 0;
                for (int t = 1; t < n_points; t++)
                {
                    trackLength += Vector2.Distance(points[t - 1], points[t]);
                }
                trackLength *= scale;

                if (trackLength < target_track_length)
                {
                    lowerAmp = amplitude;
                    lowerOffset = trackLength - target_track_length;
                }
                else
                {
                    upperAmp = amplitude;
                    upperOffset = trackLength - target_track_length;
                }
            }

            pathPoints = new Vector2[n_points];
            normals = new Complex[n_points];
            cornerRadii = ComputeCornerRadii(2 * Math.PI / n_points, dPdt);

            for (int t = 0; t < n_points; t++)
            {
                normals[t] = Complex.ImaginaryOne * dPdt[t] / Complex.Abs(dPdt[t]);
            }

            for (int t = 0; t < n_points; t++)
            {
                pathPoints[t] = new Vector2((float)points[t].X * (float)scale, (float)points[t].Y * (float)scale);
                cornerRadii[t] *= scale;
            }

            if (!SelfIntersects(ConvertToComplexFromVectorTwo_Arrays(pathPoints), normals, margin / scale))
            {
                break;
            }
        }

        return (ConvertToComplexFromVectorTwo_Arrays(pathPoints), normals, cornerRadii);
    }


    private static bool Intersects(Complex p, Complex dp, Complex q, Complex dq)
    {
        // Checks if the two line segments p->(p+dp) and q->(q+dq) intersect

        // map line segment p->(p+dp) to 0+0j->1+0j
        q = (q - p) / dp;
        dq = dq / dp;

        // handle case where dp and dq are parallel
        if (dq.Imaginary == 0)
        {
            return q.Imaginary == 0 && q.Real < 1 && q.Real + dq.Real > 0;
        }
        else
        {
            // check if transformed line segment Q intersects with line 0,0 -> 1,0
            return q.Imaginary * (q.Imaginary + dq.Imaginary) <= 0 && 0 < q.Real - dq.Real * q.Imaginary / dq.Imaginary && q.Real - dq.Real * q.Imaginary / dq.Imaginary < 1;
        }
    }

    private static bool SelfIntersectsBrute(Complex[][] edges)
    {
        // Checks if any of the line segments in `edges` intersect by checking every pair of edges

        for (int i = 0; i < edges.Length; i++)
        {
            Complex[] p_i = edges[i];
            for (int j = i + 1; j < edges.Length; j++)
            {
                Complex[] p_j = edges[j];

                // skip if the edges are adjacent
                if (p_j[0] == p_i[1] || p_j[1] == p_i[0])
                {
                    continue;
                }

                if (Intersects(p_i[0], p_i[1] - p_i[0], p_j[0], p_j[1] - p_j[0]))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static int Side(Complex p, Complex dp, Complex[][] edges)
    {
        // checks whether the edge lies ontop (returns 0), to the left (returns 1), or
        // to the right (returns -1) of the line that goes through p with slope dp

        int[] side0 = edges.Select(e => Math.Sign(((e[0] - p) / dp).Imaginary)).ToArray();
        int[] side1 = edges.Select(e => Math.Sign(((e[1] - p) / dp).Imaginary)).ToArray();

        int sum = 0;
        for (int i = 0; i < edges.Length; i++)
        {
            sum += side0[i] + side1[i];
        }

        return Math.Sign(sum);
    }

    private static bool SelfIntersectsRecurse(Complex[][] edges)
    {
        if (edges.Length <= 8)
            return SelfIntersectsBrute(edges);

        Complex center = new Complex(0, 0);
        foreach (var edge in edges)
        {
            center += edge[0] + edge[1];
        }
        center /= 2 * edges.Length;

        Complex pivot = edges[edges.Length / 2][0];

        for (int n = 0; n < 32; n++)
        {
            int side = Side(center, pivot - center, edges);
            Complex[][] left = edges.Where(e => Side(center, pivot - center, new Complex[][] { e }) >= 0).ToArray();
            Complex[][] right = edges.Where(e => Side(center, pivot - center, new Complex[][] { e }) <= 0).ToArray();

            double leftRatio = Math.Abs((double)left.Length / edges.Length - 1 / 2);
            double rightRatio = Math.Abs((double)right.Length / edges.Length - 1 / 2);

            if (leftRatio + rightRatio < 1 / 8)
            {
                return SelfIntersectsRecurse(left) || SelfIntersectsRecurse(right);
            }

            pivot = edges[new Random().Next(edges.Length)][0];
        }

        return SelfIntersectsBrute(edges);
    }

    private static Complex[][] ToEdges(Complex[] points)
    {
        Complex[][] edges = new Complex[points.Length][];

        for (int i = 0; i < points.Length; i++)
        {
            edges[i] = new Complex[] { points[i], points[(i + 1) % points.Length] };
        }

        return edges;
    }

    private static bool SelfIntersects(Complex[] points, Complex[] slopes, double margin)
    {
        Complex[] normals = new Complex[slopes.Length];
        for (int i = 0; i < slopes.Length; i++)
        {
            normals[i] = Complex.ImaginaryOne * slopes[i] / Complex.Abs(slopes[i]);
        }

        Complex[][] tmp1 = ToEdges(points.Select((p, idx) => p + margin * normals[idx]).ToArray());
        Complex[][] tmp2 = ToEdges(points.Select((p, idx) => p - margin * normals[idx]).ToArray());

        return SelfIntersectsRecurse(tmp1) || SelfIntersectsRecurse(tmp2);
    }


    /*
     * 
     * The Below Code Is Not Used But Still Implemented From Python Just 
     * In Case...
     * 
     */

    /*
    private static double[] CyclicSmooth(int[] indices, Complex[] points, double[] values, double diameter)
    {
        //Zero Based Indexing (PPOF)
        double[] distanceToNext = new double[points.Length];
        for (int t = 0; t < points.Length - 1; t++)
        {
            distanceToNext[t] = Complex.Abs(points[t + 1] - points[t]);
        }
        distanceToNext[points.Length - 1] = Complex.Abs(points[0] - points[points.Length - 1]);

        double[] smoothedValues = new double[indices.Length];
        for (int n = 0; n < indices.Length; n++)
        {
            int i = indices[n];
            double coefSum = 1;

            int curr = (i != 0) ? i - 1 : points.Length - 1;
            double distance = distanceToNext[curr];
            while (distance < diameter)
            {
                double coef = distanceToNext[curr] * Math.Sin(Math.PI * distance / diameter);
                smoothedValues[n] += coef * values[curr];
                coefSum += coef;
                curr = (curr != 0) ? curr - 1 : points.Length - 1;
                distance += distanceToNext[curr];
            }

            smoothedValues[n] /= coefSum;
        }

        return smoothedValues;
    }
    */

    /*
    private Tuple<Vector2[], Complex[], double[]> PickStartingPoint(
    Vector2[] positions, Complex[] normals, double[] cornerRadii,
    int downsample = 2)
    {
        //Take Values From Config When Possible
        double startingStraightLength = config.StartingStraightLength;

        double smoothDiameter = 1.5 * startingStraightLength;

        double[] curvature = new double[cornerRadii.Length / downsample];
        for (int i = 0; i < curvature.Length; i++)
        {
            curvature[i] = Math.Abs(1 / cornerRadii[i * downsample]);
        }

        int[] indices = Enumerable.Range(0, curvature.Length)
                                  .OrderBy(i => curvature[i])
                                  .Take(curvature.Length / 8)
                                  .ToArray();

        int startIndex = downsample * indices[Array.IndexOf(curvature, indices.Min())];
        positions = RollVec2(positions, -startIndex);
        normals = ConvertToComplexFromVectorTwo_Arrays(RollVec2(ConvertToVectorTwoFromComplex_Arrays(normals), -startIndex));
        cornerRadii = RollDou(cornerRadii, -startIndex);

        Vector2 translation = positions[0];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] -= translation;
        }

        Complex rotation = Complex.ImaginaryOne / normals[0];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] *= new Vector2((float)rotation.Real, (float)rotation.Imaginary);
            normals[i] *= rotation;
        }

        return Tuple.Create(positions, normals, cornerRadii);
    }
    */

    public (Complex[], Complex[], Complex[]) PlaceCones(Complex[] positions, Complex[] normals, double[] cornerRadii)
    {

        //Take Values From Config When Possible
        double minCornerRadius = config.MinCornerRadius;
        double minConeSpacing = config.MinConeSpacing;
        double maxConeSpacing = config.MaxConeSpacing;
        double trackWidth = config.TrackWidth;
        double coneSpacingBias = config.ConeSpacingBias;
        double startOffset = 0;
        double startingConeSpacing = config.StartingConeSpacing;

        double minDensity = (1 / maxConeSpacing)+0.1;
        double maxDensity = 1 / minConeSpacing;
        double densityRange = maxDensity - minDensity;

        double c1 = densityRange / 2 * ((1 - coneSpacingBias) * minCornerRadius
                                        - (1 + coneSpacingBias) * trackWidth / 2);
        double c2 = densityRange / 2 * ((1 + coneSpacingBias) * minCornerRadius
                                        - (1 - coneSpacingBias) * trackWidth / 2);

        Complex[] Place(Complex[] points, double[] radii, int side)
        {
            double[] distanceToNext = CalculateDistanceToNext(points);

            double[] CalculateDistanceToNext(Complex[] points)
            {
                double[] differences = new double[points.Length - 1];

                for (int i = 0; i < points.Length - 1; i++)
                {
                    differences[i] = Complex.Abs(points[i + 1] - points[i]);
                }

                double lastToFirstDistance = Complex.Abs(points[0] - points[points.Length - 1]);

                return differences.Concat(new double[] { lastToFirstDistance }).ToArray();
            }

            double[] distanceToPrev = RollDou(distanceToNext, 1);

            double[] coneDensity = new double[radii.Length];
            for (int i = 0; i < radii.Length; i++)
            {
                coneDensity[i] = minDensity + (side * c1 / radii[i]) + (c2 / Math.Abs(radii[i]));
            }

            for (int i = 0; i < coneDensity.Length; i++)
            {
                coneDensity[i] *= distanceToPrev[i];
            }

            double modifiedLength = coneDensity.Sum();
            double threshold = modifiedLength / Math.Round(modifiedLength);

            List<Complex> cones = new List<Complex> { points[0] };
            double current = 0;
            for (int i = 1; i < coneDensity.Length; i++)
            {
                current += coneDensity[i];
                if (current >= threshold)
                {
                    current -= threshold;
                    cones.Add(points[i]);
                }
            }

            return cones.ToArray();
        }


        double[] TranslateValsInArray(double[] array, double val)
        {
            double[] result = new double[array.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = array[i] + val;
            }
            return result;
        }
        
        Complex[] MultiplyByScalarComplex(Complex[] vector, double scalar)
        {
            return vector.Select(v => v * scalar).ToArray();
        }

        Complex[] AddComplexArrays(Complex[] array1, Complex[] array2)
        {
            if (array1.Length != array2.Length)
            {
                throw new ArgumentException("Input arrays must have the same length.");
            }

            return array1.Zip(array2, (a, b) => a + b).ToArray();
        }

        Complex[] SubtractComplexArrays(Complex[] array1, Complex[] array2)
        {
            if (array1.Length != array2.Length)
            {
                throw new ArgumentException("Input arrays must have the same length.");
            }

            return array1.Zip(array2, (a, b) => a - b).ToArray();
        }

        Complex[] lCones = Place(
            AddComplexArrays(positions,
            MultiplyByScalarComplex(normals, trackWidth / 2.0)),
            TranslateValsInArray(cornerRadii, -trackWidth / 2.0),
            1
        );
        Complex[] rCones = Place(
            SubtractComplexArrays(positions,
            MultiplyByScalarComplex(normals, trackWidth / 2.0)),
            TranslateValsInArray(cornerRadii, trackWidth / 2.0),
            -1
        );

        Complex[] startCones = new Complex[] { lCones[0], rCones[0] };


        int carPos = 0;
        double lengthAccum = 0;
        while (lengthAccum < startOffset)
        {
            lengthAccum += Complex.Abs(positions[carPos - 1] - positions[carPos]);
            carPos--;
        }

        (Complex[], Complex[], Complex[])TransformCones(Complex[] l_cones, Complex[] r_cones, Complex[] start_cones, int carPosition, Complex carNormal)
        {
            for (int i = 0; i < l_cones.Length; i++)
            {
                l_cones[i] -= positions[carPosition];
                Complex rotation = Complex.ImaginaryOne / carNormal;
                l_cones[i] *= rotation;
            }

            for (int i = 0; i < r_cones.Length; i++)
            {
                r_cones[i] -= positions[carPosition];
                Complex rotation = Complex.ImaginaryOne / carNormal;
                r_cones[i] *= rotation;
            }

            for (int i = 0; i < start_cones.Length; i++)
            {
                start_cones[i] -= positions[carPosition];
                Complex rotation = Complex.ImaginaryOne / carNormal;
                start_cones[i] *= rotation;
            }

            return (l_cones, r_cones, start_cones);
        } 

        (lCones, rCones, startCones) = TransformCones(lCones, rCones, startCones, carPos, normals[carPos]);
        return (startCones, lCones.Skip(1).ToArray(), rCones.Skip(1).ToArray());
    }



    /*
     Auxillary Functions
     */

    private static Complex[] ConvertToComplexFromVectorTwo_Arrays(Vector2[] vector)
    {
        Complex[] complex = new Complex[vector.Length];
        for (int t = 0; t < vector.Length; t++)
        {
            complex[t] = new Complex(vector[t].X, vector[t].Y);
        }
        return complex;
    }


    private static Vector2[] ConvertToVectorTwoFromComplex_Arrays(Complex[] complex)
    {
        Vector2[] vector = new Vector2[complex.Length];
        for (int t = 0; t < complex.Length; t++)
        {
            vector[t] = new Vector2((float)complex[t].Real, (float)complex[t].Imaginary);
        }
        return vector;
    }

    private static Vector2[] RollVec2(Vector2[] array, int shift)
    {
        Vector2[] result = new Vector2[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            int newIndex = (i - shift + array.Length) % array.Length;
            result[newIndex] = array[i];
        }
        return result;
    }

    private static double[] RollDou(double[] array, int shift)
    {
        double[] result = new double[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            int newIndex = (i - shift + array.Length) % array.Length;
            result[newIndex] = array[i];
        }
        return result;
    }


}
