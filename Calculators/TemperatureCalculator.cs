using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bullets {


public class TemperatureCalculator : MonoBehaviour
{
    // game object to calculate temperature for
    public GameObject targetObject;

    // temperature variables
    public float temperature = 15.0f; // default value for 15 degrees Celsius

    void Update()
    {
        // calculate the altitude of the target object
        float altitude = targetObject.transform.position.y;

        // calculate the temperature in Kelvin at the target object's altitude
        // this is a simplified equation that only takes into account the altitude and a standard atmosphere model
        // for a more accurate calculation, other factors such as humidity and air pressure would also need to be considered
        temperature = 15.0f - 0.0065f * altitude;
    }
}



}
