using System;
using System.Collections.Generic;

[Serializable]
public class HandState
{
    public string name; // to store the name of the hand state
    public float[] quaternionValues;
    public float[] fingerValues; 
    public bool includesQuaternion; // to indicate which components are included 
    public bool includesFingers; // to indicate which components are included
    public int includedQuaternionComponents; // to indicate which quaternion components are included,


    public HandState(string name, float[] quaternionValues, float[] fingerValues, bool includesQuaternion, bool includesFingers, int includedQuaternionComponents = 15) // Default to including all components
    {
        this.quaternionValues = quaternionValues;
        this.fingerValues = fingerValues;
        this.includesQuaternion = includesQuaternion;
        this.includesFingers = includesFingers;
        this.includedQuaternionComponents = includedQuaternionComponents;
        this.name = name;
    }
}

[Serializable]
public class HandStateCollection
{
    public List<HandState> handStates = new List<HandState>();
}