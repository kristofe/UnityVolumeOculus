/************************************************************************************

Filename    :   OVRCrosshair.cs
Content     :   Implements a hud cross-hair
Created     :   January 8, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/
using UnityEngine;

//-------------------------------------------------------------------------------------
// ***** OVRCrosshair
//
// OVRCrosshair is a component that adds a stereoscoppic cross-hair into a scene.
// It currently draws into the scene after the view has been rendered, therefore there
// is no distortion correction on it.
// 
// The static member CursorOnScreen allows other systems to know if the Cursor is visable
// or not.
// 
public class OVRCrosshair : MonoBehaviour
{
	// crosshair texture
	public Texture ImageCrosshair 	  = null;
	public float   StereoSpread  	  = 0.0f;
	public float   FadeTime			  = 1.0f;
	public float   FadeScale      	  = 0.8f;
	public float   CrosshairDistance  = 0.0f;
	
	public float   DeadZoneX          = 75.0f;
	public float   DeadZoneY          = 75.0f;
	public float   ScaleSpeedX	      = 7.0f;
	public float   ScaleSpeedY	 	  = 7.0f;
	
	private bool   DisplayCrosshair;
	private bool   CollisionWithGeometry;
	private float  FadeVal;
	private Camera MainCam;
	private float  LensOffsetLeft     = 0.0f;
	private float  LensOffsetRight    = 0.0f;
	
	private float XL = 0.0f;
	private float YL = 0.0f;
	
	// Start
	void Start()
	{
		DisplayCrosshair 		= false;
		CollisionWithGeometry 	= false;
		FadeVal 		 		= 0.0f;
		MainCam          		= Camera.main;
		
		// Initialize screen location of cursor
		XL = Screen.width * 0.25f;
		YL = Screen.height * 0.5f;
		
		// Get the values for both IPD and lens distortion correction shift
		OVRDevice.GetPhysicalLensOffsets(ref LensOffsetLeft, ref LensOffsetRight);
	}
	
	// Update
	void Update()
	{
		// Do not do these tests within OnGUI since they will be called twice
		ShouldDisplayCrosshair();
		CollisionWithGeometryCheck();
	}
	
	// OnGUI
	void OnGUI()
	{		
		if ((DisplayCrosshair == true) && (CollisionWithGeometry == false))
			FadeVal += Time.deltaTime / FadeTime;
		else
			FadeVal -= Time.deltaTime / FadeTime;
		
		FadeVal = Mathf.Clamp(FadeVal, 0.0f, 1.0f);
		
		// Check to see if crosshair influences mouse rotation
		OVRPlayerController.AllowMouseRotation = false;
		
		if ((ImageCrosshair != null) && (FadeVal != 0.0f))
		{
			// Assume cursor is on-screen (unless it goes into the dead-zone)
			// Other systems will check this to see if it is false for example 
			// allowing rotation to take place
			OVRPlayerController.AllowMouseRotation = true;

			GUI.color = new Color(1, 1, 1, FadeVal * FadeScale);
			
			float ah = StereoSpread / 2.0f  // required to adjust for physical lens shift
			      - 0.5f * ((LensOffsetLeft * (float)Screen.width / 2));
			
			// Calculate X
			XL += Input.GetAxis("Mouse X") * 0.5f * ScaleSpeedX;
			if(XL < DeadZoneX) 
			{
				OVRPlayerController.AllowMouseRotation = false;
				XL = DeadZoneX - 0.001f;	
			}
			else if (XL > (Screen.width * 0.5f) - DeadZoneX)
			{
				OVRPlayerController.AllowMouseRotation = false;
				XL = Screen.width * 0.5f - DeadZoneX + 0.001f;
			}
			
			// Calculate Y
			YL -= Input.GetAxis("Mouse Y") * ScaleSpeedY;
			if(YL < DeadZoneY) 
			{
				//CursorOnScreen = false;
				if(YL < 0.0f) YL = 0.0f;
			}
			else if (YL > Screen.height - DeadZoneY)
			{
				//CursorOnScreen = false;
				if(YL > Screen.height) YL = Screen.height;
			}
			
			// Finally draw cursor
			if(OVRPlayerController.AllowMouseRotation == true)
			{
				// Left
				GUI.DrawTexture(new Rect(	XL - (ImageCrosshair.width * 0.5f) - ah ,
					                     	YL - (ImageCrosshair.height * 0.5f), 
											ImageCrosshair.width,
											ImageCrosshair.height), 
											ImageCrosshair);
				
				float XR = XL + Screen.width * 0.5f;
				float YR = YL;
				
				// Right
				GUI.DrawTexture(new Rect(	XR - (ImageCrosshair.width * 0.5f) + ah,
											YR - (ImageCrosshair.height * 0.5f), 
											ImageCrosshair.width,
											ImageCrosshair.height), 
											ImageCrosshair);
			}
				
			GUI.color = Color.white;
		}
	}
	
	// ShouldDisplayCrosshair
	bool ShouldDisplayCrosshair()
	{	
		if(Input.GetKeyDown (KeyCode.C))
		{
			if(DisplayCrosshair == false)
			{
				DisplayCrosshair = true;
				
				// Always initialize screen location of cursor to center
				XL = Screen.width * 0.25f;
				YL = Screen.height * 0.5f;
			}
			else
				DisplayCrosshair = false;
		}
					
		return DisplayCrosshair;
	}
	
	// CollisionWithGeometry
	bool CollisionWithGeometryCheck()
	{
		CollisionWithGeometry = false;
		
		Vector3 startPos = MainCam.transform.position;
		Vector3 dir = Vector3.forward;
		dir = MainCam.transform.rotation * dir;
		dir *= CrosshairDistance;
		Vector3 endPos = startPos + dir;
		
		RaycastHit hit;
		if (Physics.Linecast(startPos, endPos, out hit)) 
		{
			if (!hit.collider.isTrigger)
			{
				CollisionWithGeometry = true;
			}
		}
		
		return CollisionWithGeometry;
	}

}
