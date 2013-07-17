/************************************************************************************

Filename    :   OVRLensCorrection.cs
Content     :   Full screen image effect. 
				This script is used to add full-screen lens correction on a camera
				component
Created     :   January 17, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/

using UnityEngine;

[AddComponentMenu("Image Effects/OVRLensCorrection")]

//-------------------------------------------------------------------------------------
// ***** OVRLensCorrection
//
// OVRLensCorrection contains the variables required to set material properties
// for the lens correction image effect.
//
public class OVRLensCorrection : OVRImageEffectBase 
{
	[HideInInspector]
	public Vector2 _Center       = new Vector2(0.5f, 0.5f);
	[HideInInspector]
	public Vector2 _ScaleIn      = new Vector2(1.0f,  1.0f);
	[HideInInspector]
	public Vector2 _Scale        = new Vector2(1.0f, 1.0f);	
	[HideInInspector]
	public Vector4 _HmdWarpParam = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
	
	// Called by camera to get lens correction values
	public Material GetMaterial()
	{
		// Set material properties
		material.SetVector("_Center",		_Center);
		material.SetVector("_Scale",		_Scale);
		material.SetVector("_ScaleIn",		_ScaleIn);
		material.SetVector("_HmdWarpParam",	_HmdWarpParam);
		
		return material;
	}
}