using UnityEngine;
using System.Collections;

public class Matrix4 {
	public float[] m;
	
	private int IX(int i, int j){
		return i + 4* j;
	}
	
	public Matrix4(){
		m = new float[16];
		identity();
	}
	
	public void identity(){
		for(int i = 0; i < 4; ++i){
			for(int j = 0; j < 4; ++j){
				if(i == j) 
					m[IX(i,j)] = 1.0f;
				else
					m[IX(i,j)] = 0.0f;
			}
		}
	}
	
	public void rotationXY(float radians){
		/*
		1  0  0  0
		0  1  0  0
		0  0  c  -s
		0  0  s  c
		*/
		identity();
		float c = Mathf.Cos(radians);
		float s = Mathf.Sin(radians);
		m[IX(2,2)] = c;
		m[IX(2,3)] = -s;
		m[IX(3,2)] = s;
		m[IX(3,3)] = c;
		
	}
	
	
	public void rotationZX(float radians){
		/*
		1  0  0  0
		0  c  0  -s
		0  0  1  0
		0  s  0  c
		*/
		identity();
		float c = Mathf.Cos(radians);
		float s = Mathf.Sin(radians);
		m[IX(1,1)] = c;
		m[IX(2,3)] = -s;
		m[IX(3,1)] = s;
		m[IX(3,3)] = c;
		
	}

	
	public void rotationXW(float radians){
		/*
		1  0  0  0
		0  c  -s 0
		0  s  c  0
		0  0  0  1
		*/
		identity();
		float c = Mathf.Cos(radians);
		float s = Mathf.Sin(radians);
		m[IX(1,1)] = c;
		m[IX(1,2)] = -s;
		m[IX(2,1)] = s;
		m[IX(2,2)] = c;
		
	}
	
	public void rotationYZ(float radians){
		/*
		c  0  0  -s
		0  1  0  0
		0  0  1  0
		s  0  0  c
		*/
		identity();
		float c = Mathf.Cos(radians);
		float s = Mathf.Sin(radians);
		m[IX(0,0)] = c;
		m[IX(0,3)] = -s;
		m[IX(3,0)] = s;
		m[IX(3,3)] = c;
		
	}
	
	public void rotationWY(float radians){
		/*
		c  0  -s 0
		0  1  0  0
		s  0  c  0
		0  0  0  1
		*/
		identity();
		float c = Mathf.Cos(radians);
		float s = Mathf.Sin(radians);
		m[IX(0,0)] = c;
		m[IX(0,2)] = -s;
		m[IX(2,0)] = s;
		m[IX(2,2)] = c;
		
	}
	
	public void rotationZW(float radians){
		/*
		c  -s 0  0
		s  c  0  0
		0  0  1  0
		0  0  0  1
		*/
		identity();
		float c = Mathf.Cos(radians);
		float s = Mathf.Sin(radians);
		m[IX(0,0)] = c;
		m[IX(0,1)] = -s;
		m[IX(1,0)] = s;
		m[IX(1,1)] = c;
		
	}
	
	public Vector4 multiplyVec4(Vector4 v){
		Vector4 r = new Vector4();
		r.x = Vector4.Dot(v,new Vector4(m[IX(0,0)], m[IX(1,0)], m[IX(2,0)], m[IX(3,0)]));
		r.y = Vector4.Dot(v,new Vector4(m[IX(0,1)], m[IX(1,1)], m[IX(2,1)], m[IX(3,1)]));
		r.z = Vector4.Dot(v,new Vector4(m[IX(0,2)], m[IX(1,2)], m[IX(2,2)], m[IX(3,2)]));
		r.w = Vector4.Dot(v,new Vector4(m[IX(0,3)], m[IX(1,3)], m[IX(2,3)], m[IX(3,3)]));
		return r;
	}
}
