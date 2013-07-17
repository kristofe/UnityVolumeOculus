/************************************************************************************

Filename    :   OVRMainMenu.cs
Content     :   Main script to run various Unity scenes
Created     :   January 8, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/
using UnityEngine;
using System.Collections;
using XInputDotNetPure; 

//-------------------------------------------------------------------------------------
// ***** OVRMainMenu
//
// OVRMainMenu is used to control the loading of different scenes. It also renders out 
// a menu that allows a user to modify various Rift settings, and allow for storing 
// these settings for recall later.
// 
// A user of this component can add as many scenes that they would like to be able to 
// have access to.
//
// OVRMainMenu is currently attached to the OVRPlayerController prefab for convenience, 
// but can safely removed from it and added to another GameObject that is used for general 
// Unity logic.
//
public class OVRMainMenu : MonoBehaviour
{
	// PUBLIC
	public float 	FadeInTime    		= 3.0f;
	public Texture 	FadeInTexture 		= null;
	public Font 	FontReplaceSmall	= null;
	public Font 	FontReplaceLarge	= null;
	
	// Scenes to show onscreen
	public string [] SceneNames;
	public string [] Scenes;
		
	// Spacing for scenes menu
	private int    	StartX			= 240;
	private int    	StartY			= 300;
	private int    	WidthX			= 300;
	private int    	WidthY			= 28;
	private int    	StepY			= 30;
	private int    	StereoSpreadX 	= -40;
	
	// Spacing for variables that users can change
	private int    	VRVarsSX		= 300;
	private int		VRVarsSY		= 425;
	private int    	VRVarsWidthX 	= 170;
	private int    	VRVarsWidthY 	= 28;
		
	
	
	// Handle to camera controller
	private OVRCameraController CameraController = null;
	
	// Controller buttons
	private bool  PrevStartDown;
	private bool  PrevHatDown;
	private bool  PrevHatUp;
	
	private bool  ShowVRVars;
	
	private bool  OldSpaceHit;
	
	// FPS 
	private float  UpdateInterval 	= 0.5f;
	private float  Accum   			= 0; 	
	private int    Frames  			= 0; 	
	private float  TimeLeft			= 0; 				
	private string strFPS			= "FPS: 0";
	
	// IPD shift from physical IPD
	public float   IPDIncrement		= 0.0025f;
	private string strIPD 			= "IPD: 0.000";	
	
	// Prediction (in ms)
	public float   PredictionIncrement = 0.001f; // 1 ms
	private string strPrediction       = "Pred: OFF";	
	
	// FOV Variables
	public float   FOVIncrement		= 0.2f;
	private string strFOV     		= "FOV: 0.0f";
	
	// Distortion Variables
	public float   DistKIncrement   = 0.001f;
	private string strDistortion 	= "Dist k1: 0.00f k2 0.00f";
	
	// Height adjustment
	public float   HeightIncrement   = 0.01f;
	private string strHeight     	 = "Height: 0.0f";
	
	// Speed and rotation adjustment
	public float   SpeedRotationIncrement   	= 0.05f;
	private string strSpeedRotationMultipler    = "Spd. X: 0.0f Rot. X: 0.0f";
	
	private bool   LoadingLevel 	= false;	
	private float  AlphaFadeValue	= 1.0f;
	private int    CurrentLevel		= 0;
	
	// Rift detection
	private bool   HMDPresent           = false;
	private bool   SensorPresent        = false;
	private float  RiftPresentTimeout   = 0.0f;
	private string strRiftPresent		= "";
	
	// Create a delegate for update functions
	private delegate void updateFunctions();
	private updateFunctions UpdateFunctions;
	
	// STATIC VARIABLES
	
	// Can be checked to see if level selection is showing 
	// (used to disable systems like movement input etc.)
	public static bool     	sShowLevels   = false;	

	

	// * * * * * * * * * * * * *

	// Awake
	void Awake()
	{
		OVRCameraController[] CameraControllers;
		CameraControllers = gameObject.GetComponentsInChildren<OVRCameraController>();
		
		if(CameraControllers.Length == 0)
			Debug.LogWarning("No OVRCameraController attached.");
		else if (CameraControllers.Length > 1)
			Debug.LogWarning("More then 1 OVRCameraController attached.");
		else
			CameraController = CameraControllers[0];	
	}
	
	// Start
	void Start()
	{
		AlphaFadeValue = 1.0f;	
		CurrentLevel   = 0;
		PrevStartDown  = false;
		PrevHatDown    = false;
		PrevHatUp      = false;
		ShowVRVars	   = false;
		OldSpaceHit    = false;
		strFPS         = "FPS: 0";
		LoadingLevel   = false;	
		
		sShowLevels    = false;
		
		// Ensure that camera controller variables have been properly
		// initialized before we start reading them
		if(CameraController != null)
			CameraController.InitCameraControllerVariables();
		
		// Save default values initially
		StoreSnapshot("DEFAULT");
		
		// Make sure to hide cursor 
		Screen.showCursor = false; 
		Screen.lockCursor = true;
		
		// Add delegates to update; useful for ordering menu tasks, if required
		UpdateFunctions += UpdateFPS;
		UpdateFunctions += UpdateIPD;
		UpdateFunctions += UpdatePrediction;
		UpdateFunctions += UpdateFOV;
		UpdateFunctions += UpdateDistortionCoefs;
		UpdateFunctions += UpdateHeightOffset;
		UpdateFunctions += UpdateSpeedAndRotationMultiplier;
		UpdateFunctions += UpdateSelectCurrentLevel;
		UpdateFunctions += UpdateHandleSnapshots;
		UpdateFunctions += UpdateResetOrientation;
		
		// Check for HMD and sensor
		CheckIfRiftPresent();
		
		// Init static members
		sShowLevels = false;
	}
	
	// Update
	void Update()
	{		
		if(LoadingLevel == true)
			return;
		
		UpdateFunctions();
	
		// Toggle Fullscreen
		if(Input.GetKeyDown(KeyCode.F11))
			Screen.fullScreen = !Screen.fullScreen;
		
		// Escape Application
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
	}
	
	// UpdateFPS
	void UpdateFPS()
	{
    	TimeLeft -= Time.deltaTime;
   	 	Accum += Time.timeScale/Time.deltaTime;
    	++Frames;
 
    	// Interval ended - update GUI text and start new interval
    	if( TimeLeft <= 0.0 )
    	{
        	// display two fractional digits (f2 format)
			float fps = Accum / Frames;
			
			if(ShowVRVars == true)// limit gc
				strFPS = System.String.Format("FPS: {0:F2}",fps);

       		TimeLeft += UpdateInterval;
        	Accum = 0.0f;
        	Frames = 0;
    	}
	}
	
	// UpdateIPD
	void UpdateIPD()
	{
		if(Input.GetKeyDown (KeyCode.Equals))
		{
			float ipd = 0;
			CameraController.GetIPD(ref ipd);
			ipd += IPDIncrement;
			CameraController.SetIPD (ipd);
		}
		else if(Input.GetKeyDown (KeyCode.Minus))
		{
			float ipd = 0;
			CameraController.GetIPD(ref ipd);
			ipd -= IPDIncrement;
			CameraController.SetIPD (ipd);
		}
		
		if(ShowVRVars == true)// limit gc
		{	
			float ipd = 0;
			CameraController.GetIPD (ref ipd);
			strIPD = System.String.Format("IPD (ms): {0:F4}", ipd * 1000.0f);
		}
	}
	
	// UpdatePrediction
	void UpdatePrediction()
	{
		// Turn prediction on/off
		if(Input.GetKeyDown (KeyCode.P))
		{		
			if( OVRCamera.PredictionOn == false) 
				OVRCamera.PredictionOn = true;
			else
				OVRCamera.PredictionOn = false;
		}
		
		// Update prediction value (only if prediction is on)
		if(OVRCamera.PredictionOn == true)
		{
			float pt = OVRDevice.GetPredictionTime(0); 
			if(Input.GetKeyDown (KeyCode.Comma))
				pt -= PredictionIncrement;
			else if(Input.GetKeyDown (KeyCode.Period))
				pt += PredictionIncrement;
			
			OVRDevice.SetPredictionTime(0, pt);
			
			// re-get the prediction time to make sure it took
			pt = OVRDevice.GetPredictionTime(0) * 1000.0f;
			
			if(ShowVRVars == true)// limit gc
				strPrediction = System.String.Format ("Pred (ms): {0:F3}", pt);								 
		}
		else
		{
			strPrediction = "Pred: OFF";
		}
	}
	
	// UpdateFOV
	void UpdateFOV()
	{
		if(Input.GetKeyDown (KeyCode.LeftBracket))
		{
			float cfov = 0;
			CameraController.GetVerticalFOV(ref cfov);
			cfov -= FOVIncrement;
			CameraController.SetVerticalFOV(cfov);
		}
		else if (Input.GetKeyDown (KeyCode.RightBracket))
		{
			float cfov = 0;
			CameraController.GetVerticalFOV(ref cfov);
			cfov += FOVIncrement;
			CameraController.SetVerticalFOV(cfov);
		}
		
		if(ShowVRVars == true)// limit gc
		{
			float cfov = 0;
			CameraController.GetVerticalFOV(ref cfov);
			strFOV = System.String.Format ("FOV (deg): {0:F3}", cfov);
		}
	}
	
	// UpdateDistortionCoefs
	void UpdateDistortionCoefs()
	{
	 	float Dk0 = 0.0f;
		float Dk1 = 0.0f;
		float Dk2 = 0.0f;
		float Dk3 = 0.0f;
		
		// * * * * * * * * *
		// Get the distortion coefficients to apply to shader
		CameraController.GetDistortionCoefs(ref Dk0, ref Dk1, ref Dk2, ref Dk3);
		
		if(Input.GetKeyDown(KeyCode.Alpha1))
			Dk1 -= DistKIncrement;
		else if (Input.GetKeyDown(KeyCode.Alpha2))
			Dk1 += DistKIncrement;
			
		if(Input.GetKeyDown(KeyCode.Alpha3))
			Dk2 -= DistKIncrement;
		else if (Input.GetKeyDown(KeyCode.Alpha4))
			Dk2 += DistKIncrement;
		
		CameraController.SetDistortionCoefs(Dk0, Dk1, Dk2, Dk3);
		
		if(ShowVRVars == true)// limit gc
			strDistortion = 
			System.String.Format ("DST k1: {0:F3} k2 {1:F3}", Dk1, Dk2);
	}
	
	// UpdateHeightOffset
	void UpdateHeightOffset()
	{
		if(Input.GetKeyDown(KeyCode.Alpha5))
		{	
			Vector3 neckPosition = Vector3.zero;
			CameraController.GetNeckPosition(ref neckPosition);
			neckPosition.y -= HeightIncrement;
			CameraController.SetNeckPosition(neckPosition);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			Vector3 neckPosition = Vector3.zero;;
			CameraController.GetNeckPosition(ref neckPosition);
			neckPosition.y += HeightIncrement;
			CameraController.SetNeckPosition(neckPosition);
		}
			
		if(ShowVRVars == true)// limit gc
		{
			Vector3 rootPosition = new Vector3(0.0f, 1.0f, 0.0f);
			Vector3 neckPosition = Vector3.zero;
			CameraController.GetNeckPosition(ref neckPosition);
			Vector3 eyePosition = Vector3.zero;
			CameraController.GetEyeCenterPosition(ref eyePosition);

			// default capsule is 2.0m, but the center offset is at 1.0m
			float ph = rootPosition.y + neckPosition.y + eyePosition.y;  

			strHeight = System.String.Format ("Player Height (m): {0:F3}", ph);
		}
	}
	
	// UpdateSpeedAndRotationMultiplier
	void UpdateSpeedAndRotationMultiplier()
	{
		if(Input.GetKeyDown(KeyCode.Alpha7))
			OVRPlayerController.MoveScaleMultiplier -= SpeedRotationIncrement;
		else if (Input.GetKeyDown(KeyCode.Alpha8))
			OVRPlayerController.MoveScaleMultiplier += SpeedRotationIncrement;		

		if(Input.GetKeyDown(KeyCode.Alpha9))
			OVRPlayerController.RotationScaleMultiplier -= SpeedRotationIncrement;
		else if (Input.GetKeyDown(KeyCode.Alpha0))
			OVRPlayerController.RotationScaleMultiplier += SpeedRotationIncrement;		
		
		if(ShowVRVars == true)// limit gc
			strSpeedRotationMultipler = System.String.Format ("Spd.X: {0:F2} Rot.X: {1:F2}", 
									OVRPlayerController.MoveScaleMultiplier, 
									OVRPlayerController.RotationScaleMultiplier);
	}
	

	
	// UpdateSelectCurrentLevel
	void UpdateSelectCurrentLevel()
	{
		ShowLevels();
				
		if(sShowLevels == false)
			return;
			
		CurrentLevel = GetCurrentLevel();
		
		if((Scenes.Length != 0) && 
		   ((OVRGamepadController.GetButtonA() == true) ||
			 Input.GetKeyDown(KeyCode.Return)))
		{
			Application.LoadLevel(Scenes[CurrentLevel]);
		}
	}
	
	// ShowLevels
	bool ShowLevels()
	{
		if(Scenes.Length == 0)
		{
			sShowLevels = false;
			return sShowLevels;
		}
		
		bool curStartDown = false;
		if(OVRGamepadController.GetButtonStart() == true)
			curStartDown = true;
		
		if((PrevStartDown == false) && (curStartDown == true) ||
			Input.GetKeyDown(KeyCode.RightShift) )
		{
			if(sShowLevels == true) 
				sShowLevels = false;
			else 
				sShowLevels = true;
		}
		
		PrevStartDown = curStartDown;
		
		return sShowLevels;
	}
	
	// GetCurrentLevel
	int GetCurrentLevel()
	{
		bool curHatDown = false;
		if(OVRGamepadController.GetDPadDown() == true)
			curHatDown = true;
		
		bool curHatUp = false;
		if(OVRGamepadController.GetDPadDown() == true)
			curHatUp = true;
		
		if((PrevHatDown == false) && (curHatDown == true) ||
			Input.GetKeyDown(KeyCode.DownArrow))
		{
			CurrentLevel = (CurrentLevel + 1) % SceneNames.Length;	
		}
		else if((PrevHatUp == false) && (curHatUp == true) ||
			Input.GetKeyDown(KeyCode.UpArrow))
		{
			CurrentLevel--;	
			if(CurrentLevel < 0)
				CurrentLevel = SceneNames.Length - 1;
		}
					
		PrevHatDown = curHatDown;
		PrevHatUp = curHatUp;
		
		return CurrentLevel;
	}
	
	// GUI
	
	// OnGUI
 	void OnGUI()
 	{
		if(LoadingLevel == true)
			return;
		
		// Fade in screen
		if(AlphaFadeValue > 0.0f)
		{
  			AlphaFadeValue -= Mathf.Clamp01(Time.deltaTime / FadeInTime);
			if(AlphaFadeValue < 0.0f) AlphaFadeValue = 0.0f;
  			GUI.color = new Color(0, 0, 0, AlphaFadeValue);
  			GUI.DrawTexture( new Rect(0, 0, Screen.width, Screen.height ), FadeInTexture ); 
		}
		else
		{						
			// If true, we are displaying information about the Rift not being detected
			// So do not show anything else
			if(ShowRiftDetected() == true)
				return;
			
			//if(GUI
			
			GUIShowLevels();
			GUIShowVRVariables();
		}
 	}
	
	// GUIShowLevels
	void GUIShowLevels()
	{
		if(sShowLevels == true)
		{   
			// Darken the background by rendering fade texture 
			GUI.color = new Color(0, 0, 0, 0.7f);
  			GUI.DrawTexture( new Rect(0, 0, Screen.width, Screen.height ), FadeInTexture );
 			GUI.color = Color.white;
		
			for (int i = 0; i < SceneNames.Length; i++)
			{
				Color c;
				if(i == CurrentLevel)
					c = Color.yellow;
				else
					c = Color.black;
				
				int y   = StartY + (i * StepY);
				
				GUIStereoBox (StartX, y, WidthX, WidthY, ref SceneNames[i], c);
			}
		}				
	}
	
	// GUIShowVRVariables
	void GUIShowVRVariables()
	{
		bool SpaceHit = Input.GetKey("space");
		if ((OldSpaceHit == false) && (SpaceHit == true))
		{
			if(ShowVRVars == true) 
				ShowVRVars = false;
			else 
				ShowVRVars = true;
		}
		
		OldSpaceHit = SpaceHit;
		
		if(ShowVRVars == false)
			return;
		
		int y   = VRVarsSY;
		
		// FPS
		GUIStereoBox (VRVarsSX, y, VRVarsWidthX, VRVarsWidthY, 
						 ref strFPS, Color.green);
		y += StepY;
		// Prediction code
		GUIStereoBox (VRVarsSX, y, VRVarsWidthX, VRVarsWidthY, 
						 ref strPrediction, Color.white);
		// IPD
		y += StepY;
		GUIStereoBox (VRVarsSX, y, VRVarsWidthX, VRVarsWidthY, 
						 ref strIPD, Color.yellow);
		// FOV
		y += StepY;
		GUIStereoBox (VRVarsSX, y, VRVarsWidthX, VRVarsWidthY, 
						 ref strFOV, Color.white);
		// Player Height
		y += StepY;
		GUIStereoBox (VRVarsSX, y, VRVarsWidthX, VRVarsWidthY, 
						 ref strHeight, Color.yellow);
		// Speed Rotation Multiplier
		y += StepY;
		GUIStereoBox (VRVarsSX, y, VRVarsWidthX, VRVarsWidthY, 
						 ref strSpeedRotationMultipler, Color.white);
		// Distortion k values
		y += StepY;
		GUIStereoBox (VRVarsSX, y, VRVarsWidthX, VRVarsWidthY, 
						 ref strDistortion, Color.red);	
	}
	
	// GUIStereoBox - Values based on pixels in DK1 resolution of W: (1280 / 2) H: 800
	void GUIStereoBox(int X, int Y, int wX, int wY, ref string text, Color color)
	{
		float ploLeft = 0, ploRight = 0;
		float sSX = (float)Screen.width / 1280.0f;
		
		float sSY = ((float)Screen.height / 800.0f);
		
		CameraController.GetPhysicalLensOffsets(ref ploLeft, ref ploRight); 
		int xL = (int)((float)X * sSX);
		int sSpreadX = (int)((float)StereoSpreadX * sSX);
		int xR = (Screen.width / 2) + xL + sSpreadX - 
			      // required to adjust for physical lens shift
			      (int)(ploLeft * (float)Screen.width / 2);
		int y = (int)((float)Y * sSY);
		
		GUI.contentColor = color;
		
		int sWX = (int)((float)wX * sSX);
		int sWY = (int)((float)wY * sSY);
		
		// Change font size based on screen scale
		if(Screen.height > 800)
			GUI.skin.font = FontReplaceLarge;
		else
			GUI.skin.font = FontReplaceSmall;
		
		GUI.Box(new Rect(xL, y, sWX, sWY), text);
		GUI.Box(new Rect(xR, y, sWX, sWY), text);			
	}

	// SNAPSHOT MANAGEMENT
	
	// UpdateHandleSnapshots
	void UpdateHandleSnapshots()
	{
		// Default shapshot
		if(Input.GetKeyDown(KeyCode.F2))
			LoadSnapshot ("DEFAULT");
		
		// Snapshot 1
		if(Input.GetKeyDown(KeyCode.F3))
		{	
			if(Input.GetKey(KeyCode.Tab))
				StoreSnapshot ("SNAPSHOT1");
			else
				LoadSnapshot ("SNAPSHOT1");
		}
		
		// Snapshot 2
		if(Input.GetKeyDown(KeyCode.F4))
		{	
			if(Input.GetKey(KeyCode.Tab))
				StoreSnapshot ("SNAPSHOT2");
			else
				LoadSnapshot ("SNAPSHOT2");
		}
		
		// Snapshot 3
		if(Input.GetKeyDown(KeyCode.F5))
		{	
			if(Input.GetKey(KeyCode.Tab))
				StoreSnapshot ("SNAPSHOT3");
			else
				LoadSnapshot ("SNAPSHOT3");
		}
		
	}
	
	// StoreSnapshot
	bool StoreSnapshot(string snapshotName)
	{
		float f = 0;
		
		OVRPresetManager.SetCurrentPreset(snapshotName);
		
		CameraController.GetIPD(ref f);
		OVRPresetManager.SetPropertyFloat("IPD", ref f);
	
		f = OVRDevice.GetPredictionTime(0);
		OVRPresetManager.SetPropertyFloat("PREDICTION", ref f);
		
		CameraController.GetVerticalFOV(ref f);
		OVRPresetManager.SetPropertyFloat("FOV", ref f);
		
		Vector3 neckPosition = Vector3.zero;
		CameraController.GetNeckPosition(ref neckPosition);
		OVRPresetManager.SetPropertyFloat("HEIGHT", ref neckPosition.y);
		
		OVRPresetManager.SetPropertyFloat("SPEEDMULT", 
										  ref OVRPlayerController.MoveScaleMultiplier);
		
		OVRPresetManager.SetPropertyFloat("ROTMULT", 
										  ref OVRPlayerController.RotationScaleMultiplier);
		
		float Dk0 = 0.0f;
		float Dk1 = 0.0f;
		float Dk2 = 0.0f;
		float Dk3 = 0.0f;		
		CameraController.GetDistortionCoefs(ref Dk0, ref Dk1, ref Dk2, ref Dk3);
		
		OVRPresetManager.SetPropertyFloat("DISTORTIONK0", ref Dk0);
		OVRPresetManager.SetPropertyFloat("DISTORTIONK1", ref Dk1);
		OVRPresetManager.SetPropertyFloat("DISTORTIONK2", ref Dk2);
		OVRPresetManager.SetPropertyFloat("DISTORTIONK3", ref Dk3);
		
		return true;
	}
	
	// LoadSnapshot
	bool LoadSnapshot(string snapshotName)
	{
		float f = 0;
		
		OVRPresetManager.SetCurrentPreset(snapshotName);
		
		if(OVRPresetManager.GetPropertyFloat("IPD", ref f) == true)
			CameraController.SetIPD(f);
		
		if(OVRPresetManager.GetPropertyFloat("PREDICTION", ref f) == true)
			OVRDevice.SetPredictionTime(0, f);
		
		if(OVRPresetManager.GetPropertyFloat("FOV", ref f) == true)
			CameraController.SetVerticalFOV(f);
		
		if(OVRPresetManager.GetPropertyFloat("HEIGHT", ref f) == true)
		{
			Vector3 neckPosition = Vector3.zero;
			CameraController.GetNeckPosition(ref neckPosition);
			neckPosition.y = f;
			CameraController.SetNeckPosition(neckPosition);
		}

		if(OVRPresetManager.GetPropertyFloat("SPEEDMULT", ref f) == true)
			OVRPlayerController.MoveScaleMultiplier = f;

		if(OVRPresetManager.GetPropertyFloat("ROTMULT", ref f) == true)
			OVRPlayerController.RotationScaleMultiplier = f;
		
		float Dk0 = 0.0f;
		float Dk1 = 0.0f;
		float Dk2 = 0.0f;
		float Dk3 = 0.0f;
		CameraController.GetDistortionCoefs(ref Dk0, ref Dk1, ref Dk2, ref Dk3);
		
		if(OVRPresetManager.GetPropertyFloat("DISTORTIONK0", ref f) == true)
			Dk0 = f;
		if(OVRPresetManager.GetPropertyFloat("DISTORTIONK1", ref f) == true)
			Dk1 = f;
		if(OVRPresetManager.GetPropertyFloat("DISTORTIONK2", ref f) == true)
			Dk2 = f;
		if(OVRPresetManager.GetPropertyFloat("DISTORTIONK3", ref f) == true)
			Dk3 = f;
		
		CameraController.SetDistortionCoefs(Dk0, Dk1, Dk2, Dk3);
		
		return true;
	}
	
	// RIFT DETECTION
	
	// CheckIfRiftPresent
	// Checks to see if HMD and / or sensor is available, and displays a 
	// message if it is not
	void CheckIfRiftPresent()
	{
		HMDPresent = OVRDevice.IsHMDPresent();
		SensorPresent = OVRDevice.IsSensorPresent(0); // 0 is the main head sensor
		
		if((HMDPresent == false) || (SensorPresent == false))
		{
			RiftPresentTimeout = 10.0f; // Keep message up for 10 seconds
			
			if((HMDPresent == false) && (SensorPresent == false))
				strRiftPresent = "NO HMD AND SENSOR DETECTED";
			else if (HMDPresent == false)
				strRiftPresent = "NO HMD DETECTED";
			else if (SensorPresent == false)
				strRiftPresent = "NO SENSOR DETECTED";
		}
	}
	
	// ShowRiftDetected
	bool ShowRiftDetected()
	{
		if(RiftPresentTimeout > 0.0f)
		{
			RiftPresentTimeout -= Time.deltaTime;
			if(RiftPresentTimeout < 0.0f)
				RiftPresentTimeout = 0.0f;
						
			GUIStereoBox (StartX, StartY, WidthX, WidthY, ref strRiftPresent, Color.white);
			
			return true;
		}
		
		return false;
	}
	
	// RIFT RESET ORIENTATION
	
	// UpdateResetOrientation
	void UpdateResetOrientation()
	{
		if( ((sShowLevels == false) && (OVRGamepadController.GetDPadDown () == true)) ||
			(Input.GetKeyDown(KeyCode.B) == true) )
		{
			OVRDevice.ResetOrientation(0);
		}
	}
	
}
