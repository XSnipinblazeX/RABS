using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Bullets {




public static class ExternalBallisticsCalculator
{

	//Air pressure and dynamics

	public static float GetDryAirDensity(float airPressure, float Temperature)
	{
		float PdryAir = airPressure / 287.05f * Temperature;

		return PdryAir;
	}
	public static float GetHumidAirDensity(float airPressure, float gasContant, float Temperature, float partialDryPressure, float gasContantDry, float partialPressureWater, float gasConstantWater)
	{

		partialDryPressure = ConvertHPA(partialDryPressure);

		float PhumidAir =	partialDryPressure / gasContantDry * Temperature + partialPressureWater / gasConstantWater * Temperature;

		return PhumidAir;
	}

	public static float GetAirPressure(float p0, float P0, float Height, float Temperature)
	{

		//p0 = air density at sea level
		//P0 = air pressure at sea level
		//temperature should be in Kelvin
		float gravity = 9.81f;
		float e = 2.718281828439f;
		//float c = -p0 * gravity * Height / P0;
		float p = p0 * 273.15f / Temperature * Mathf.Pow(e,  -p0 * gravity * Height / P0);

		return p;
		
	}
	
	//

	//Wind factor calculations

	//unit conversion calulations
	public static float ConvertHPA(float hpa)
	{
		float Pa = hpa / 0.01f;

		return Pa;
	}

	public static float ConvertCelsiusToKelvin(float c)
	{
		float K = c + 273.15f;
		return K;
	}
	public static float ConvertFahrenheitToKelvin(float f)
	{
		float K = (f-32)*5/9+273.15f;
		return K;
	}
}



}
