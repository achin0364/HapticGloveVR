using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Linq;
using System.Collections;
using Unity.Collections;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Management;
using UnityEngine.XR.Hands.Samples.VisualizerSample;
using System.IO.Ports;

public class HandTracking : MonoBehaviour
{
    public SerialPort stream;
    public string command_off = "010101"; //turns off all fingers
    public string command_index = "011001"; //turns on index finger
    public GameManager gameManager;

    public HandVisualizer handVisualizer;
    HandVisualizer.HandGameObjects m_LeftHandGameObjects;
    bool debugTrue = false;
    List<Vector3> points;
    GameObject[] joints;

    void Start()
    {
        stream = new SerialPort("COM4", 9600);
        stream.ReadTimeout = 50;
        stream.Open();
    }

    void Update()
    {
        while (m_LeftHandGameObjects == null)
        {
            m_LeftHandGameObjects = handVisualizer.m_LeftHandGameObjects;
        }
        if (m_LeftHandGameObjects != null && debugTrue == false)
        {
            debug("FOUND");
            debug(m_LeftHandGameObjects.m_DrawJoints.Length.ToString());
            joints = m_LeftHandGameObjects.m_DrawJoints;
            debugTrue = true;
        }

        if (joints.Length > 0) //detects joints in contact with boxes
        {
            points = new List<Vector3>();
            for (int i = 0; i < joints.Length; i++)
            {
                if (i == 10) {
                    debug("Joint #" + i + " Position: " + joints[i].transform.position);
                    points.Add(joints[i].transform.position);
                    if (stream.IsOpen) {
                        stream.Write(command_index); //turn on index finger
                    }
                }
            }
        }
        else {
            if (stream.IsOpen) {
                stream.Write(command_off); //turn off all fingers
            }
        }

        Dictionary<int, GameObject> overlaps = gameManager.CheckOverlap(points);
        if (overlaps.Count > 0) {
            gameManager.DisplayText(overlaps);
            // loop through dictionary items and change the color of the handMarkers object to the color of the GameObject renderer
            foreach (KeyValuePair<int, GameObject> item in overlaps)
            {
                joints[item.Key].GetComponent<Renderer>().material.color = item.Value.GetComponent<Renderer>().material.color;
            }
            List<int> untouched = Enumerable.Range(0, 26).Except(overlaps.Keys).ToList();
            StartCoroutine(waiter(untouched));
        }
    }

    // delay 1 second then change the color back to white if the joint is not overlapping with any object
    IEnumerator waiter(List<int> untouched)
    {
        yield return new WaitForSeconds(1);
        foreach (int i in untouched)
        {
            joints[i].GetComponent<Renderer>().material.color = Color.white;
        }
    }

    void debug(string message)
    {
        Debug.Log(message);
    }
}