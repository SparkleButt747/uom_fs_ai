using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Drawing;

public class LoadTrack : MonoBehaviour
{
    // file path of the CSV file
    private string csvfilepath;
    string path_yellow_n_black = "Assets/models/yellow_and_black_v1.fbx";
    string path_blue_n_white = "Assets/models/blue_and_white_v2.fbx";

    // Define Locations Of Files
    string pythonExecutable = "";
    string pythonScriptPath = "python/generate_track_csv.py";
    string workingDirectory = "";

    // Start is called before the first frame update
    void Start()
    {
        GenerateTrack();
    }

    GameObject ImportModelFromFBX(string path)
    {

        UnityEngine.Object model = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        GameObject modelInstance = Instantiate(model) as GameObject;
        return modelInstance;
    }

    void RemoveObjectsWithTag(string tagToRemove)
    {
        // Find all game objects with the specified tag
        GameObject[] objectsToRemove = GameObject.FindGameObjectsWithTag(tagToRemove);

        // Destroy each game object
        foreach (GameObject obj in objectsToRemove)
        {
            Destroy(obj);
        }
    }

    public void GenerateTrack()
    {
        //Check Whether A Track Already Exists And Remove The Cones
        RemoveObjectsWithTag("trackBoundary");

        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = pythonExecutable;
        start.Arguments = pythonScriptPath;
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.WorkingDirectory = workingDirectory;


        // Generate The CSV File From Python
        using (Process process = Process.Start(start))
        {
            StreamReader outputStream = process.StandardOutput;
            StreamReader errorStream = process.StandardError;

            string output = outputStream.ReadToEnd();
            string error = errorStream.ReadToEnd();

            process.WaitForExit();

            if (!(process.ExitCode == 0))
            {
                // Process encountered an error
                UnityEngine.Debug.LogError("Process exited with an error");
                UnityEngine.Debug.LogError("Error: " + error);
            }

            csvfilepath = "Assets/" + Regex.Replace(output, "[^0-9]", "") + "track0.csv";
        }


        // read the file using StreamReader
        using (StreamReader reader = new StreamReader(csvfilepath))
        {

            // read each line in the CSV file
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                //Debug.Log(line);

                // split the line by comma
                string[] values = line.Split(',');

                // parse the x, y and direction values
                float x = float.Parse(values[1]);
                float z = float.Parse(values[2]);

                // create a Vector3 point
                Vector3 point = new Vector3(x, 0.1f, z);

                string color = values[0];

                if (color == "blue" )
                {
                    //Debug.Log("The color is blue");
                    GameObject blue_n_white = ImportModelFromFBX(path_blue_n_white);
                    blue_n_white.tag = "trackBoundary";

                    blue_n_white.AddComponent<MeshFilter>();

                    blue_n_white.AddComponent<MeshRenderer>();

                    blue_n_white.AddComponent<BoxCollider>();
                    blue_n_white.GetComponent<BoxCollider>().size = new Vector3(0.25f, 0.25f, 0.4f);
                    blue_n_white.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, 0.2f);

                    blue_n_white.AddComponent<Rigidbody>();

                    blue_n_white.transform.position = point;
                    blue_n_white.transform.Rotate(-90, 0, 0);
                    blue_n_white.transform.localScale = Vector3.one * 2f;
                }
                if (color == "yellow")
                {
                    //Debug.Log("The color is yellow");
                    GameObject yellow_n_black = ImportModelFromFBX(path_yellow_n_black);
                    yellow_n_black.tag = "trackBoundary";

                    yellow_n_black.AddComponent<MeshFilter>();

                    yellow_n_black.AddComponent<MeshRenderer>();

                    yellow_n_black.AddComponent<BoxCollider>();
                    yellow_n_black.GetComponent<BoxCollider>().size = new Vector3(0.25f, 0.25f, 0.4f);
                    yellow_n_black.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, 0.2f);

                    yellow_n_black.AddComponent<Rigidbody>();

                    yellow_n_black.transform.position = point;
                    yellow_n_black.transform.Rotate(-90, 0, 0);
                    yellow_n_black.transform.localScale = Vector3.one * 2f;
                }

            }


        }

        // Remove file after loading the track
        try
        {
            File.Delete(csvfilepath);
            Console.WriteLine("File deleted successfully.");
        }
        catch (IOException e)
        {
            Console.WriteLine("An error occurred while deleting the file: " + e.Message);
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine("Access to the file was denied: " + e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: " + e.Message);
        }


        //Close Starting Gap
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.tag = "trackBoundary";
        box.AddComponent<BoxCollider>();

        box.AddComponent<Rigidbody>();

        box.transform.position = new Vector3(5f, 1f, 2.5f);
        box.transform.Rotate(90, 0, 0);
        box.transform.localScale = new Vector3(2.25f, 0.5f, 1f);


        GameObject box2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box2.tag = "trackBoundary";
        box2.AddComponent<BoxCollider>();

        box2.AddComponent<Rigidbody>();

        box2.transform.position = new Vector3(5f, 1f, -2.5f);
        box2.transform.Rotate(90, 0, 0);
        box2.transform.localScale = new Vector3(2.25f, 0.5f, 1f);
    }
}







