
//Shaders start with the Shader keyword followed by shader's name in ""
//Custom/C2E1 means that the shader name is C2E1 and is placed in a shadr group called Custom
//Refer here : http://unity3d.com/support/documentation/Components/SL-Shader.html
Shader "Custom/RayCastGLSL" 
{
	Properties
	{
		g_eyePos ("Eye Position", Vector) = (1.0, 1.0, 1.0, 1.0)
		g_lightPos ("Light Position", Vector) = (1.0, 1.0, 1.0, 1.0)
		g_lightIntensity ("Light Intensity", float) = 1.0
		g_absorption ("Absorption", float) = 1.0
		g_densityTex ("Density Texture", 3D) = "white"{}
		g_tex ("Text Texture", 2D) = "white"{}
		g_ambientLight ("Ambient Light", Color) = (0.2,0.2, 0.2, 1.0)
		
	}
	//Refer here : http://unity3d.com/support/documentation/Components/SL-SubShader.html
	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		//ColorMask RGB
		
		//Refer here : http://unity3d.com/support/documentation/Components/SL-Pass.html
		Pass 
		{
		//Refer here : http://unity3d.com/support/documentation/Components/SL-ShaderPrograms.html
		GLSLPROGRAM
		// Upgrade NOTE: excluded shader from DX11 and Xbox360; 
		// has structs without semantics (struct v2f members position,uvw)
		//#pragma exclude_renderers d3d11 xbox360
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 4.0
		#pragma glsl
		#pragma only_renderers opengl
		#include "UnityCG.glslinc"
 		
 		//uniform keyword not needed in unity but putting it in to be explicit
 		uniform vec4 g_eyePos;
 		uniform vec4 g_lightPos;
 		uniform float g_lightIntensity;
 		uniform float g_absorption;
 		uniform sampler3D g_densityTex;
 		uniform sampler2D g_tex;
 		uniform vec4 g_ambientLight;
 		
 		varying vec3 textureCoordinates;
 
 #ifdef VERTEX
		void main()
		{
		  gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
		  textureCoordinates = vec3(gl_Color.xyz);
		  //textureCoordinates.x = gl_Color.x * 0.5 - 0.5;
		  //textureCoordinates.y = gl_Color.y * 0.5 - 0.5;
		  //textureCoordinates.z = gl_Color.z * 0.5 - 0.5;
		  
		}		
 #endif
		

 #ifdef FRAGMENT		
		void main() 
		{
			//gl_FragColor = texture3D(g_densityTex, textureCoordinates);
			
			
			vec3 eyePos = vec3(g_eyePos.xyz);
			vec3 lightPos = vec3(g_lightPos.xyz);
			float absorption = g_absorption;
			float lightIntensity = g_lightIntensity;
			
			// diagonal of the cube
		    float maxDist = sqrt(3.0);
		
		    int numSamples = 128;
		    float scale = maxDist/float(numSamples);
		
		    int numLightSamples = 64;
		    float lscale = maxDist / float(numLightSamples);
		
		    // assume all coordinates are in texture space
		    vec3 pos = textureCoordinates.xyz;
		    vec3 eyeDir = normalize(pos-eyePos)*scale;
			
			float d = 0.0;
			float T = 1.0;
			vec3 Lo = vec3(0);
			for (int i=0; i < numSamples; ++i)
		    {
		        // sample density
		        float density = texture3D(g_densityTex, pos).x;
		        // skip empty space
		        if (density > 0.0)
		        {
		            // attenuate ray-throughput
		            d += density*scale*absorption;

		        }
		        pos += eyeDir;
		        //pos = clamp(pos,0.0,1.0);
		    }
		
		    gl_FragColor.xyz = vec3(1);
		    gl_FragColor.w = d;
			
			/*
		    // transmittance
		    float T = 1.0;
		    // in-scattered radiance
		    vec3 Lo = vec3(0.0);
			
		    for (int i=0; i < numSamples; ++i)
		    {
		        // sample density
		        float density = texture3D(g_densityTex, pos).x;
		        // skip empty space
		        if (density > 0.0)
		        {
		            // attenuate ray-throughput
		            //T *= 1.0-density*scale*absorption;
		            T *= 1.0-density*scale*absorption;
		            if (T <= 0.01)
		                break;
		
		            // point light dir in texture space
		            vec3 lightDir = normalize(lightPos-pos)*lscale;
		
		            // sample light
		            float Tl = 1.0; // transmittance along light ray
		            vec3 lpos = pos + lightDir;
		
		            for (int s=0; s < numLightSamples; ++s)
		            {
		                float ld = texture3D(g_densityTex, lpos).x;
		                Tl *= 1.0-absorption*lscale*ld;
		                //Tl *= 1.0-absorption*ld;
		
		                if (Tl <= 0.01)
		                    break;
		
		                lpos += lightDir;
		            }
					
		            vec3 Li = vec3(lightIntensity*Tl);
		            Lo += Li*T*density*scale;
		        }
		        pos += eyeDir;
		    }
		
		    gl_FragColor.xyz = Lo + g_ambientLight.xyz;
		    gl_FragColor.w = 1.0-T;
		    */
		    
		}
 #endif
		ENDGLSL
		}
	}
	Fallback "Diffuse"
}