using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalBallisticsController : MonoBehaviour
{
	[Tooltip("Temperature at Altitude 0 being sea level")]
	public AnimationCurve temperature;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public float GetTemperature(float height)
	{
		float temp = temperature.Evaluate(height);

		return temp;
	}

}
