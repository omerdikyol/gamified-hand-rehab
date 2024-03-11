using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to represent hand states 
public class HandState
{
    public float[] quaternionValues;
    public float[] fingerValues;
    public bool includesQuaternion;
    public bool includesFingers;

    // Constructor to initialize the hand state
    public HandState(float[] quaternionValues, float[] fingerValues, bool includesQuaternion, bool includesFingers)
    {
        this.quaternionValues = quaternionValues;
        this.fingerValues = fingerValues;
        this.includesQuaternion = includesQuaternion;
        this.includesFingers = includesFingers;
    }
}