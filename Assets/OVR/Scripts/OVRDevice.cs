/************************************************************************************

Filename    :   OVRDevice.cs
Content     :   Interface for the Oculus Rift Device
Created     :   February 14, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/
using UnityEngine;
using System;
using System.Runtime.InteropServices;

//-------------------------------------------------------------------------------------
// ***** OVRDevice
//
// OVRDevice is the main interface to the Oculus Rift hardware. It includes wrapper functions
// for  all exported C++ functions, as well as helper functions that use the stored Oculus
// variables to help set up camera behavior.
//
// This component is added to the OVRCameraController prefab. It can be part of any 
// game object that one sees fit to place it. However, it should only be declared once,
// since there are public members that allow for tweaking certain Rift values in the
// Unity inspector.
//
public class OVRDevice : MonoBehaviour 
{
	// Imported functions from 
	// OVRPlugin.dll 	(PC)
	// OVRPlugin.so 	(Linux, Android)
	// OVRPlugin.bundle (OSX)
	
	// SENSOR FUNCTIONS
	[DllImport ("OculusPlugin")]
	private static extern bool OVR_Initialize();
	[DllImport ("OculusPlugin")]
	private static extern bool OVR_Destroy();
	[DllImport ("OculusPlugin")]
    private static extern int OVR_GetSensorCount();
	[DllImport ("OculusPlugin")]
	private static extern bool OVR_IsHMDPresent();
	[DllImport ("OculusPlugin")]
	private static extern bool OVR_IsSensorPresent(int sensor);
	[DllImport ("OculusPlugin")]
    private static extern bool OVR_GetSensorOrientation(int sensorID, 
														ref float w, 
														ref float x, 
														ref float y, 
														ref float z);
	[DllImport ("OculusPlugin")]
    private static extern bool OVR_GetSensorPredictedOrientation(int sensorID, 
															     ref float w, 
																 ref float x, 
																 ref float y, 
																 ref float z);
	[DllImport ("OculusPlugin")]
    private static extern bool OVR_GetSensorPredictionTime(int sensorID, ref float predictionTime);
	[DllImport ("OculusPlugin")]
    private static extern bool OVR_SetSensorPredictionTime(int sensorID, float predictionTime);
	[DllImport ("OculusPlugin")]
    private static extern bool OVR_GetSensorAccelGain(int sensorID, ref float accelGain);
	[DllImport ("OculusPlugin")]
    private static extern bool OVR_SetSensorAccelGain(int sensorID, float accelGain);
	[DllImport ("OculusPlugin")]
    private static extern bool OVR_ResetSensorOrientation(int sensorID);	
	
	// DISPLAY FUNCTIONS
	[DllImport ("OculusPlugin")]
	private static extern System.IntPtr OVR_GetDisplayDeviceName();  
	[DllImport ("OculusPlugin")]
	private static extern bool OVR_GetScreenResolution(ref int hResolution, ref int vResolution);
	[DllImport ("OculusPlugin")]
	private static extern bool OVR_GetScreenSize(ref float hSize, ref float vSize);
	[DllImport ("OculusPlugin")]
	private static extern bool OVR_GetEyeToScreenDistance(ref float eyeToScreenDistance);
	[DllImport ("OculusPlugin")]
	private static extern bool OVR_GetEyeOffset(ref float leftEye, ref float rightEye);
	[DllImport ("OculusPlugin")]
	private static extern bool OVR_GetScreenVCenter(ref float vCenter);
	[DllImport ("OculusPlugin")]
	private static extern bool OVR_GetDistortionCoefficients(ref float k0, 
														     ref float k1, 
															 ref float k2, 
															 ref float k3);
	
	// LATENCY TEST FUNCTIONS
	[DllImport ("OculusPlugin")]
    private static extern void OVR_ProcessLatencyInputs();
	[DllImport ("OculusPlugin")]
    private static extern bool OVR_DisplayLatencyScreenColor(ref byte r, 
															ref byte g, 
															ref byte b);
	[DllImport ("OculusPlugin")]
    private static extern System.IntPtr OVR_GetLatencyResultsString();
	
	
	// PUBLIC
	public float InitialPredictionTime 							= 0.05f; // 50 ms
	public float InitialAccelGain  								= 0.05f; // default value
	
	// STATIC 
	private static bool  OVRInit 								= false;
	
	public static int    SensorCount 					 	    = 0;
	
	public static String DisplayDeviceName;
	
	public static int    HResolution, VResolution 				= 0;	 // pixels
	public static float  HScreenSize, VScreenSize 				= 0.0f;	 // meters
	public static float  EyeToScreenDistance  					= 0.0f;  // meters
	public static float  LeftEyeOffset, RightEyeOffset			= 0.0f;  // meters
	public static float  ScreenVCenter 							= 0.0f;	 // meters 
	public static float  DistK0, DistK1, DistK2, DistK3 		= 0.0f;
	
	// The physical offset of the lenses, used for shifting both IPD and lens distortion
	private static float LensOffsetLeft, LensOffsetRight   	= 0.0f;
	
	// Fit to top of the image (default is 5" display)
    private static float DistortionFitX 						= 0.0f;
    private static float DistortionFitY 						= 1.0f;
	
	// Copied from initialized public variables set in editor
	private static float PredictionTime 						= 0.0f;
	private static float AccelGain 								= 0.0f;
	
	// Used to reduce the size of render distortion and give better fidelity
	// Accessed with a public static function
	private static float  DistortionFitScale 					= 0.7f;  // Optimized for DK1 (7")
	
	
	// * * * * * * * * * * * * *

	// Awake
	void Awake () 
	{	
		OVRInit = OVR_Initialize();
		
		if(OVRInit == false) 
			return;

        SensorCount = OVR_GetSensorCount();
		
		// * * * * * * *
		// DISPLAY SETUP
		
		// We will get the HMD so that we can eventually target it within Unity
		DisplayDeviceName += Marshal.PtrToStringAnsi(OVR_GetDisplayDeviceName());
		
		OVR_GetScreenResolution (ref HResolution, ref VResolution);
		OVR_GetScreenSize (ref HScreenSize, ref VScreenSize);
		OVR_GetEyeToScreenDistance(ref EyeToScreenDistance);
		OVR_GetEyeOffset (ref LeftEyeOffset, ref RightEyeOffset);
		OVR_GetScreenVCenter (ref ScreenVCenter);
		OVR_GetDistortionCoefficients( ref DistK0, ref DistK1, ref DistK2, ref DistK3);
	
		// Distortion fit parameters based on if we are using a 5" (Prototype, DK2+) or 7" (DK1) 
		if (HScreenSize < 0.140f) 	// 5.5"
		{
			DistortionFitX = 0.0f;
			DistortionFitY = 1.0f;
			
			// Don't shrink as much (5.5" has denser pixels)
			DistortionFitScale = 1.0f;
		}
    	else 						// 7"
		{
			DistortionFitX = -1.0f;
			DistortionFitY =  0.0f;
		}
		
		// Calculate the lens offsets for each eye and store 
		CalculatePhysicalIPDOffsets(ref LensOffsetLeft, ref LensOffsetRight);
		
		// * * * * * * *
		// SENSOR SETUP
		
		// PredictionTime set, to init sensor directly
		if(PredictionTime > 0.0f)
            OVR_SetSensorPredictionTime(0, PredictionTime);
		else
			SetPredictionTime(0, InitialPredictionTime);	
		
		// AcelGain set, used to correct gyro with accel. 
		// Default value is appropriate for typical use.
		if(AccelGain > 0.0f)
            OVR_SetSensorAccelGain(0, AccelGain);
		else
			SetAccelGain(0, InitialAccelGain);	
		
		// Always do a reset of the Sensor when we init
		ResetOrientation(0);
	}
   
	// Start (Note: make sure to always have a Start function for classes that have
	// editors attached to them)
	void Start()
	{
	}
	
	// Destroy
	void OnDestroy()
	{
		OVR_Destroy();
		OVRInit = false;
	}
	
	
	// * * * * * * * * * * * *
	// PUBLIC FUNCTIONS
	// * * * * * * * * * * * *
	
	// Inited - Check to see if system has been initialized
	public static bool IsInitialized()
	{
		return OVRInit;
	}
	
	// HMDPreset
	public static bool IsHMDPresent()
	{
		return OVR_IsHMDPresent();
	}

	// SensorPreset
	public static bool IsSensorPresent(int sensor)
	{
		return OVR_IsSensorPresent(sensor);
	}
	
	// GetOrientation
	public static bool GetOrientation(ref Quaternion q)
	{
		float w = 0, x = 0, y = 0, z = 0;

        if (OVR_GetSensorOrientation(0, ref w, ref x, ref y, ref z) == true)
		{
			q.w =  w;		
		
			// Change the co-ordinate system from right-handed to Unity left-handed
			/*
			q.x =  x; 
			q.y =  y;
			q.z =  -z; 
			q = Quaternion.Inverse(q);
			*/
		
			// The following does the exact same conversion as above
			q.x = -x; 
			q.y = -y;
			q.z =  z;	
		
			return true;
		}
		
		return false;
	}
	
	// GetPredictedOrientation
	public static bool GetPredictedOrientation(ref Quaternion q)
	{
		float w = 0, x = 0, y = 0, z = 0;

        if (OVR_GetSensorPredictedOrientation(0, ref w, ref x, ref y, ref z) == true)
		{

			q.w =  w;		
			q.x = -x; 
			q.y = -y;
			q.z =  z;	
		
			return true;
		}
		
		return false;

	}		
	
	// ResetOrientation
	public static bool ResetOrientation(int sensor)
	{
        return OVR_ResetSensorOrientation(sensor);
	}
	
	// GetPredictionTime
	public static float GetPredictionTime(int sensor)
	{		
		// return OVRSensorsGetPredictionTime(sensor, ref predictonTime);
		return PredictionTime;
	}

	// SetPredictionTime
	public static bool SetPredictionTime(int sensor, float predictionTime)
	{
		if ( (predictionTime > 0.0f) &&
             (OVR_SetSensorPredictionTime(sensor, predictionTime) == true))
		{
			PredictionTime = predictionTime;
			return true;
		}
		
		return false;
	}
	
	// GetAccelGain
	public static float GetAccelGain(int sensor)
	{		
		return AccelGain;
	}

	// SetAccelGain
	public static bool SetAccelGain(int sensor, float accelGain)
	{
		if ( (accelGain > 0.0f) &&
             (OVR_SetSensorAccelGain(sensor, accelGain) == true))
		{
			AccelGain = accelGain;
			return true;
		}
		
		return false;
	}
	
	// GetDistortionCorrectionCoefficients
	public static bool GetDistortionCorrectionCoefficients(ref float k0, 
														   ref float k1, 
														   ref float k2, 
														   ref float k3)
	{
		if(!OVRInit)
			return false;
		
		k0 = DistK0;
		k1 = DistK1;
		k2 = DistK2;
		k3 = DistK3;
		
		return true;
	}
	
	// SetDistortionCorrectionCoefficients
	public static bool SetDistortionCorrectionCoefficients(float k0, 
														   float k1, 
														   float k2, 
														   float k3)
	{
		if(!OVRInit)
			return false;
		
		DistK0 = k0;
		DistK1 = k1;
		DistK2 = k2;
		DistK3 = k3;
		
		return true;
	}
	
	// GetPhysicalLensOffsets
	public static bool GetPhysicalLensOffsets(ref float lensOffsetLeft, 
											  ref float lensOffsetRight)
	{
		if(!OVRInit)
			return false;
		
		lensOffsetLeft  = LensOffsetLeft;
		lensOffsetRight = LensOffsetRight;	
		
		return true;
	}
	
	// GetIPD
	public static bool GetIPD(ref float IPD)
	{
		if(!OVRInit)
			return false;
		
		IPD = LeftEyeOffset + RightEyeOffset;
		
		return true;
	}
	
	// GetPhysicalLensOffsetsFromIPD
	public static bool GetPhysicalLensOffsetsFromIPD(float IPD, 
													 ref float LensOffsetLeft, 
													 ref float LensOffsetRight)
	{
		LensOffsetLeft  = 0.0f;
		LensOffsetRight = 0.0f;
		
		if(!OVRInit)
			return false;
		
		float halfIPD = IPD * 0.5f;
		float halfHSS = HScreenSize * 0.5f;
		LensOffsetLeft = (((halfHSS - halfIPD ) / halfHSS) * 2.0f) - 1.0f;
		LensOffsetRight = ((halfIPD / halfHSS) * 2.0f) - 1.0f;
		
		return true;
	}
	
	// CalculateAspectRatio
	public static float CalculateAspectRatio()
	{
		if(Application.isEditor)
			return (Screen.width * 0.5f) / Screen.height;
		else
			return (HResolution * 0.5f) / VResolution;
	}
	
	// VerticalFOV
	// Compute Vertical FOV based on distance, distortion, etc.
    // Distance from vertical center to render vertical edge perceived through the lens.
    // This will be larger then normal screen size due to magnification & distortion.
	public static float VerticalFOV()
	{
		if(!OVRInit)
		{
			return 90.0f;
		}
			
    	float percievedHalfScreenDistance = (VScreenSize / 2) * DistortionScale();
    	float VFov = Mathf.Rad2Deg * 2.0f * 
			         Mathf.Atan(percievedHalfScreenDistance / EyeToScreenDistance);	
		
		return VFov;
	}
	
	// DistortionScale - Used to adjust size of shader based on 
	// shader K values to maximize screen size
	public static float DistortionScale()
	{
		if(OVRInit)
		{
			float ds = 0.0f;
		
			// Compute distortion scale from DistortionFitX & DistortionFitY.
    		// Fit value of 0.0 means "no fit".
    		if ((Mathf.Abs(DistortionFitX) < 0.0001f) &&  (Math.Abs(DistortionFitY) < 0.0001f))
    		{
        		ds = 1.0f;
    		}
    		else
    		{
        		// Convert fit value to distortion-centered coordinates before fit radius
        		// calculation.
        		float stereoAspect = 0.5f * Screen.width / Screen.height;
        		float dx           = (DistortionFitX * DistortionFitScale) - LensOffsetLeft;
        		float dy           = (DistortionFitY * DistortionFitScale) / stereoAspect;
        		float fitRadius    = Mathf.Sqrt(dx * dx + dy * dy);
        		ds  			   = CalcScale(fitRadius);
    		}	
			
			if(ds != 0.0f)
				return ds;
			
		}
		
		return 1.0f; // no scale
	}
	
	// LatencyProcessInputs
    public static void ProcessLatencyInputs()
	{
        OVR_ProcessLatencyInputs();
	}
	
	// LatencyProcessInputs
    public static bool DisplayLatencyScreenColor(ref byte r, ref byte g, ref byte b)
	{
        return OVR_DisplayLatencyScreenColor(ref r, ref g, ref b);
	}
	
	// LatencyGetResultsString
    public static System.IntPtr GetLatencyResultsString()
	{
        return OVR_GetLatencyResultsString();
	}
	
	// Computes scale that should be applied to the input render texture
    // before distortion to fit the result in the same screen size.
    // The 'fitRadius' parameter specifies the distance away from distortion center at
    // which the input and output coordinates will match, assuming [-1,1] range.
    static float CalcScale(float fitRadius)
    {
        float s = fitRadius;
        // This should match distortion equation used in shader.
        float ssq   = s * s;
        float scale = s * (DistK0 + DistK1 * ssq + DistK2 * ssq * ssq + DistK3 * ssq * ssq * ssq);
        return scale / fitRadius;
    }
	
	// CalculatePhysicalIPDOffsets - Used to offset both the IPD of camera (perspective shift) 
	// and distortion shift
	static bool CalculatePhysicalIPDOffsets(ref float leftOffset, ref float rightOffset)
	{
		leftOffset  = 0.0f;
		rightOffset = 0.0f;
		
		if(!OVRInit)
			return false;
		
		float halfHSS = HScreenSize * 0.5f;
		leftOffset =  (((halfHSS - LeftEyeOffset) / halfHSS) * 2.0f) - 1.0f;
		rightOffset = ((RightEyeOffset / halfHSS) * 2.0f) - 1.0f;
		
		return true;
	}
	
}
