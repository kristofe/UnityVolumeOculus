/************************************************************************************

Filename    :   OVRCamera.cs
Content     :   Interface to camera class
Created     :   January 8, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/

//#define MSAA_ENABLED // Not available in Unity yet

using UnityEngine;
using System.Runtime.InteropServices;

[RequireComponent(typeof(Camera))]

//-------------------------------------------------------------------------------------
// ***** OVRCamera
//
// OVRCamera is used to render into a Unity Camera class. 
// This component handles reading the Rift tracker and positioning the camera position
// and rotation. It also is responsible for properly rendering the final output, which
// also the final lens correction pass.
//
public class OVRCamera : OVRComponent
{		
	
	// PRIVATE MEMBERS
	
	// If CameraTextureScale is not 1.0f, we will render to this texture 
	private RenderTexture	CameraTexture	  	= null;

	// PRIVATE STATIC MEMBERS
		
	// rotation from sensor, used by all cameras 
	// Final translation of cam location dependant on EyePosition from neck
	private static Quaternion DirQ = Quaternion.identity;	
	
	// Default material, just blit texture over to final buffer
	static Material		  BlitMaterial			= null;	

	// Color only material, used for drawing quads on-screen
	static Material 	  ColorOnlyMaterial     = null;
	static Color 		  QuadColor 			= Color.red;
	
	
	// PUBLIC MEMBERS
	
	// camera position...	
	// From root of camera to neck (translation only)
	[HideInInspector]
	public Vector3 NeckPosition = new Vector3(0.0f, 0.7f,  0.0f);	
	// From neck to eye (rotation and translation; x will be different for each eye)
	[HideInInspector]
	public Vector3 EyePosition  = new Vector3(0.0f, 0.09f, 0.16f);
	
	
	// PUBLIC STATIC MEMBERS
	
	// Scaled size of final render buffer
	// A value of 1 will not create a render buffer but will render directly to final
	// backbuffer
 	public static float		CameraTextureScale 	= 1.0f;

	// Set externally by player controller to tell the cameras which way to face
	// And if we want rotation of parent to be set by camera (Y only)
	public static float   	YRotation          	= 0.0f;
	public static bool    	SetParentYRotation 	= false;
	public static Quaternion OrientationOffset  = Quaternion.identity;
	
	// Use this to decide where rendering should take place
	// Setting to true allows for better latency, but some systems
	// (such as Pro water) will break
	public static bool    	CallCameraInPreRender = false; 
	
	// Use this to turn on wire-mode
	public static bool   	WireMode  			= false;
	
	// Use this to turn on/off Prediction
	public static bool    	PredictionOn 		= true;
	
	
// * * * * * * * * * * * * *

	// Awake
	new void Awake()
	{
		base.Awake ();
		
		// Material used to blit from one render texture to another
		if(BlitMaterial == null)
		{
			BlitMaterial = new Material (
				"Shader \"BlitCopy\" {\n" +
				"	SubShader { Pass {\n" +
				" 		ZTest Off Cull Off ZWrite Off Fog { Mode Off }\n" +
				"		SetTexture [_MainTex] { combine texture}"	+
				"	}}\n" +
				"Fallback Off }"
			);
		}
		
		// Material used for drawing color only polys into a render texture
		// Used by Latency tester
		if(ColorOnlyMaterial == null)
		{
			ColorOnlyMaterial = new Material (

			    "Shader \"Solid Color\" {\n" +
    			"Properties {\n" +
                "_Color (\"Color\", Color) = (1,1,1)\n" +
                "}\n" +
    			"SubShader {\n" +
    			"Color [_Color]\n" +
    			"Pass {}\n" +
    			"}\n" +
    			"}"		
			);
		}	
	}

	// Start
	new void Start()
	{
		base.Start ();		
		
		// NOTE: MSAA TEXTURES NOT AVAILABLE YET
		// Set CameraTextureScale (increases the size of the texture we are rendering into
		// for a better pixel match when post processing the image through lens distortion)
#if MSAA_ENABLED
		CameraTextureScale = OVRDevice.DistortionScale();
#endif		
		// If CameraTextureScale is not 1.0f, create a new texture and assign to target texture
		// Otherwise, fall back to normal camera rendering
		if((CameraTexture == null) && (CameraTextureScale > 1.0f))
		{
			int w = (int)(Screen.width / 2.0f * CameraTextureScale);
			int h = (int)(Screen.height * CameraTextureScale);
			CameraTexture = new RenderTexture(  w, h, 24); // 24 bit colorspace
			
			// NOTE: MSAA TEXTURES NOT AVAILABLE YET
			// This value should be the default for MSAA textures
			//CameraTexture.antiAliasing = 2; 
			// Set it within the project
#if MSAA_ENABLED
			CameraTexture.antiAliasing = QualitySettings.antiAliasing;
#endif
		}
	}

	// Update
	new void Update()
	{
		base.Update ();
	}
	
	// OnPreCull
	void OnPreCull()
	{
		// NOTE: Setting the camera here increases latency, but ensures
		// that all Unity sub-systems that rely on camera location before
		// being set to render are satisfied. 
		if(CallCameraInPreRender == false)
			SetCameraOrientation();
	
	}
	
	// OnPreRender
	void OnPreRender()
	{
		// NOTE: Better latency performance here, but messes up water rendering and other
		// systems that rely on the camera to be set before PreCull takes place.
		if(CallCameraInPreRender == true)
			SetCameraOrientation();
		
		if(WireMode == true)
			GL.wireframe = true;
		
		// Set new buffers and clear color and depth
		if(CameraTexture != null)
		{
			Graphics.SetRenderTarget(CameraTexture);
			GL.Clear (true, true, gameObject.camera.backgroundColor);
		}
	}
	
	// OnPostRender
	void OnPostRender()
	{
		if(WireMode == true)
			GL.wireframe = false;
	}
	
	// OnRenderImage
	void  OnRenderImage (RenderTexture source, RenderTexture destination)
	{		
		bool flipImage = true;
		
		// Use either source input or CameraTexutre, if it exists
		RenderTexture SourceTexture = source;
		
		if (CameraTexture != null)
		{
			SourceTexture = CameraTexture;
			flipImage = false; // If MSAA is on, this will be true
		}
		else
		{
			// Check if quality settings are set
			if(QualitySettings.antiAliasing == 0)
			{
				flipImage = false; // If MSAA is on, this will be true
			}
		}
		
		// Render into source texture before lens correction
		Camera c = gameObject.camera;
		RenderPreLensCorrection(ref c, ref SourceTexture);
		
		// Replace null material with lens correction material
		Material material = GetComponent<OVRLensCorrection>().GetMaterial();
		
		
		// Draw to final destination
		Blit(SourceTexture, null, material, flipImage);
		
		// Run latency test by drawing out quads to the destination buffer
		LatencyTest(destination);
		
	}
	
	// SetCameraOrientation
	void SetCameraOrientation()
	{
		Quaternion q   = Quaternion.identity;
		Vector3    dir = Vector3.forward;		
		
		// Main camera has a depth of 0, so it will be rendered first
		if(gameObject.camera.depth == 0.0f)
		{			
			// If desired, update parent transform y rotation here
			// This is useful if we want to track the current location of
			// of the head.
			// TODO: Future support for x and z, and possibly change to a quaternion
			if(SetParentYRotation == true)
			{
				Vector3 a = gameObject.camera.transform.rotation.eulerAngles;
				a.x = 0; 
				a.z = 0;
				gameObject.transform.parent.transform.eulerAngles = a;
			}
				
			// Read sensor here (prediction on or off)
			if(PredictionOn == false)
				OVRDevice.GetOrientation(ref DirQ);
			else
				OVRDevice.GetPredictedOrientation(ref DirQ);
			
			// This needs to go as close to reading Rift orientation inputs
			OVRDevice.ProcessLatencyInputs();			
		}
		
		// Calculate the rotation Y offset that is getting updated externally
		// (i.e. like a controller rotation)
		q = Quaternion.Euler(0.0f, YRotation, 0.0f);
		dir = q * Vector3.forward;
		q.SetLookRotation(dir, Vector3.up);
	
		// Multiply the offset orientation first
		q = OrientationOffset * q;
		
		// Multiply in the current HeadQuat (q is now the latest best rotation)
		q = q * DirQ;
		
		// * * *
		// Update camera rotation
		gameObject.camera.transform.rotation = q;
		
		// * * *
		// Update camera position (first add Offset to parent transform)
		gameObject.camera.transform.position = 
		gameObject.camera.transform.parent.transform.position + NeckPosition;
	
		// Adjust neck by taking eye position and transforming through q
		gameObject.camera.transform.position += q * EyePosition;

		// PGG Alternate calculation for above...
		//Vector3 EyePositionNoX = EyePosition; EyePositionNoX.x = 0.0f;
		//gameObject.camera.transform.position += q * EyePositionNoX;	
		//gameObject.camera.ResetWorldToCameraMatrix();
		//Matrix4x4 m = camera.worldToCameraMatrix;
		//Matrix4x4 tm = Matrix4x4.identity;
		//tm.SetColumn (3, new Vector4 (-EyePosition.x, 0.0f, 0.0f, 1));
		//gameObject.camera.worldToCameraMatrix  = tm * m;
		
	}
	
	// CreatePerspectiveMatrix
	// We will create our own perspective matrix
	void CreatePerspectiveMatrix(ref Matrix4x4 m)
	{
		float nearClip = gameObject.camera.nearClipPlane;
		float farClip = gameObject.camera.farClipPlane;
		float tanHalfFov = Mathf.Tan(Mathf.Deg2Rad * gameObject.camera.fov * 0.5f);
		float ar = gameObject.camera.aspect;
		m.m00 = 1.0f / (ar * tanHalfFov);
		m.m11 = 1.0f / tanHalfFov;
		m.m22 = farClip / (nearClip - farClip);
    	m.m32 = -1.0f;
    	m.m23 = (farClip * nearClip) / (nearClip - farClip);
		m.m33 = 0.0f;
	}

	// Blit - Copies one render texture onto another through a material
	// flip will flip the render horizontally
	void Blit (RenderTexture source, RenderTexture dest, Material m, bool flip) 
	{
		Material material = m;
		
		// Default to blitting material if one doesn't get passed in
		if(material == null)
			material = BlitMaterial;
		
		// Make the destination texture the target for all rendering
		RenderTexture.active = dest;  		
		
		// Assign the source texture to a property from a shader
		source.SetGlobalShaderProperty ("_MainTex");	
		
		// Set up the simple Matrix
		GL.PushMatrix ();
		GL.LoadOrtho ();
		for(int i = 0; i < material.passCount; i++)
		{
			material.SetPass(i);
			DrawQuad(flip);
		}
		GL.PopMatrix ();
	}
	
	// DrawQuad
	void DrawQuad(bool flip)
	{
		GL.Begin (GL.QUADS);
		
		if(flip == true)
		{
			GL.TexCoord2( 0.0f, 1.0f ); GL.Vertex3( 0.0f, 0.0f, 0.1f );
			GL.TexCoord2( 1.0f, 1.0f ); GL.Vertex3( 1.0f, 0.0f, 0.1f );
			GL.TexCoord2( 1.0f, 0.0f ); GL.Vertex3( 1.0f, 1.0f, 0.1f );
			GL.TexCoord2( 0.0f, 0.0f ); GL.Vertex3( 0.0f, 1.0f, 0.1f );
		}
		else
		{
			GL.TexCoord2( 0.0f, 0.0f ); GL.Vertex3( 0.0f, 0.0f, 0.1f );
			GL.TexCoord2( 1.0f, 0.0f ); GL.Vertex3( 1.0f, 0.0f, 0.1f );
			GL.TexCoord2( 1.0f, 1.0f ); GL.Vertex3( 1.0f, 1.0f, 0.1f );
			GL.TexCoord2( 0.0f, 1.0f ); GL.Vertex3( 0.0f, 1.0f, 0.1f );
		}
		
		GL.End();
	}
	

	
	// LatencyTest
	void LatencyTest(RenderTexture dest)
	{
		byte r = 0,g = 0, b = 0;
		
		// See if we get a string back to send to the debug out
		string s = Marshal.PtrToStringAnsi(OVRDevice.GetLatencyResultsString());
		if (s != null)
		{
			string result = 
			"\n\n---------------------\nLATENCY TEST RESULTS:\n---------------------\n";
			result += s;
			result += "\n\n\n";
			print(result);
		}
		
		if(OVRDevice.DisplayLatencyScreenColor(ref r, ref g, ref b) == false)
			return;
		
		RenderTexture.active = dest;  		
		Material material = ColorOnlyMaterial;
		QuadColor.r = (float)r / 255.0f;
		QuadColor.g = (float)g / 255.0f;
		QuadColor.b = (float)b / 255.0f;
		material.SetColor("_Color", QuadColor);
		GL.PushMatrix();
    	material.SetPass(0);
    	GL.LoadOrtho();
    	GL.Begin(GL.QUADS);
    	GL.Vertex3(0.3f,0.3f,0);
    	GL.Vertex3(0.3f,0.7f,0);
    	GL.Vertex3(0.7f,0.7f,0);
    	GL.Vertex3(0.7f,0.3f,0);
    	GL.End();
    	GL.PopMatrix();
		
	}


	///////////////////////////////////////////////////////////
	// PUBLIC FUNCTIONS
	///////////////////////////////////////////////////////////
		
	// RenderPreLensCorrection
	public virtual void RenderPreLensCorrection(ref Camera camera, ref RenderTexture target)
	{
		// Render into target here.
		// A GUI system should be rendered here.
		// One can query the camera to decide which is left and which is right
	}
	
	// SetPerspectiveOffset
	public void SetPerspectiveOffset(ref Vector3 offset)
	{
		// NOTE: Unity skyboxes do not currently use the projection matrix, so
		// if one wants to use a skybox with the Rift it must be implemented 
		// manually
		gameObject.camera.ResetProjectionMatrix();
		Matrix4x4 m = Matrix4x4.identity;// = gameObject.camera.projectionMatrix;
		CreatePerspectiveMatrix(ref m);
		Matrix4x4 tm = Matrix4x4.identity;
		tm.SetColumn (3, new Vector4 (offset.x, offset.y, 0.0f, 1));
		gameObject.camera.projectionMatrix = tm * m;
	}
	
	
}
