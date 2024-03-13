public class HandState
{
    public float[] quaternionValues;
    public float[] fingerValues;
    public bool includesQuaternion;
    public bool includesFingers;
    public int includedQuaternionComponents; // New field to indicate which quaternion components are included

    public HandState(float[] quaternionValues, float[] fingerValues, bool includesQuaternion, bool includesFingers, int includedQuaternionComponents = 15) // Default to including all components
    {
        this.quaternionValues = quaternionValues;
        this.fingerValues = fingerValues;
        this.includesQuaternion = includesQuaternion;
        this.includesFingers = includesFingers;
        this.includedQuaternionComponents = includedQuaternionComponents;
    }
}