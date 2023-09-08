using System;
using System.IO;
using UnityEngine;
//using UnityEngine.Windows;
using YamlDotNet.RepresentationModel;

public class VehicleParam
{

    // Param struct
    public Param.Inertia Inertia;
    public Param.Kinematic Kinematic;
    public Param.Tire Tire;
    public Param.Aero Aero;
    public Param.InputRanges InputRanges;


    // Constructor to parse the YAML file and initialize parameters
    public VehicleParam(string yamlFilePath)
    {
        ParseYaml(yamlFilePath);
    }

    // Method to parse the YAML file and populate the parameters
    private void ParseYaml(string yamlFilePath)
    {
        string yamlContent = File.ReadAllText(yamlFilePath);
        var input = new StringReader(yamlContent);
        var yaml = new YamlStream();
        yaml.Load(input);
        
        // Extract the root node (YamlMappingNode) from the YAML document
        var root = (YamlMappingNode)yaml.Documents[0].RootNode;

        // Parse and populate the respective parameters using helper methods
        Inertia = ParseParamInertia(root["inertia"] as YamlMappingNode);
        Kinematic = ParseParamKinematic(root["kinematics"] as YamlMappingNode);
        Tire = ParseParamTire(root["tire"] as YamlMappingNode);
        Aero = ParseParamAero(root["aero"] as YamlMappingNode);
        InputRanges = ParseParamInputRanges(root["input_ranges"] as YamlMappingNode);

    }

    private Param.Inertia ParseParamInertia(YamlMappingNode node)
    {
        var inertia = new Param.Inertia();
        inertia.m = Convert.ToDouble(node["m"].ToString());
        inertia.g = Convert.ToDouble(node["g"].ToString());
        inertia.I_z = Convert.ToDouble(node["I_z"].ToString());
        inertia.C_f = Convert.ToDouble(node["Cf"].ToString());
        inertia.C_r = Convert.ToDouble(node["Cr"].ToString());
        return inertia;
    }

    private Param.Kinematic ParseParamKinematic(YamlMappingNode node)
    {
        var kinematic = new Param.Kinematic();
        kinematic.l = Convert.ToDouble(node["l"].ToString());
        kinematic.b_F = Convert.ToDouble(node["b_F"].ToString());
        kinematic.b_R = Convert.ToDouble(node["b_R"].ToString());
        kinematic.w_front = Convert.ToDouble(node["w_front"].ToString());
        kinematic.l_F = kinematic.l * (1 - kinematic.w_front);
        kinematic.l_R = kinematic.l * kinematic.w_front;
        kinematic.axle_width = Convert.ToDouble(node["axle_width"].ToString());
        return kinematic;
    }

    private Param.Tire ParseParamTire(YamlMappingNode node)
    {
        var tire = new Param.Tire();
        tire.tire_coefficient = Convert.ToDouble(node["tire_coefficient"].ToString());
        tire.B = Convert.ToDouble(node["B"].ToString()) / tire.tire_coefficient;
        tire.C = Convert.ToDouble(node["C"].ToString());
        tire.D = Convert.ToDouble(node["D"].ToString()) * tire.tire_coefficient;
        tire.E = Convert.ToDouble(node["E"].ToString());
        tire.radius = Convert.ToDouble(node["radius"].ToString());
        return tire;
    }

    private Param.Aero ParseParamAero(YamlMappingNode node)
    {
        var aero = new Param.Aero();
        aero.c_down = Convert.ToDouble(node["C_Down"].ToString());
        aero.c_drag = Convert.ToDouble(node["C_drag"].ToString());
        return aero;
    }

    private Param.InputRanges ParseParamInputRanges(YamlMappingNode node)
    {
        var inputRanges = new Param.InputRanges();
        var accNode = node["acceleration"] as YamlMappingNode;

        inputRanges.acc = new Param.InputRanges.Range();
        inputRanges.acc.min = Convert.ToDouble(accNode["min"].ToString());
        inputRanges.acc.max = Convert.ToDouble(accNode["max"].ToString());

        var velNode = node["velocity"] as YamlMappingNode;
        inputRanges.vel = new Param.InputRanges.Range();
        inputRanges.vel.min = Convert.ToDouble(velNode["min"].ToString());
        inputRanges.vel.max = Convert.ToDouble(velNode["max"].ToString());

        var steeringNode = node["steering"] as YamlMappingNode;
        inputRanges.delta = new Param.InputRanges.Range();
        inputRanges.delta.min = Convert.ToDouble(steeringNode["min"].ToString());
        inputRanges.delta.max = Convert.ToDouble(steeringNode["max"].ToString());

        return inputRanges;
    }
}
