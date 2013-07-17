using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//HAS TO BE ATTACHED TO THE CAMERA TO ACTUALLY DRAW ANYTHING
//Pro only
//How to get access to it:
// LineDrawer ld = Camera.main.GetComponent<LineDrawer>();

public class LineDrawer : MonoBehaviour {
   private List<Vector3> _lineData;
   private List<Color> _colorData;
   private Color  _color;
   private Material _lineMaterial;

   public void Start(){
      Debug.Log("LineDrawer::Start()");
      _color = Color.white;
      _lineData = new List<Vector3>();
      _colorData = new List<Color>();
      createLineMaterial();
   }

   private void createLineMaterial() {
      if( !_lineMaterial ) {
         _lineMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
            "SubShader { Pass { " +
            "    Blend SrcAlpha OneMinusSrcAlpha " +
            "    ZWrite Off Cull Off Fog { Mode Off } " +
            "    BindChannels {" +
            "      Bind \"vertex\", vertex Bind \"color\", color }" +
            "} } }" );
         _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
         _lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
      }
   }

   public void setColor(Color c){
      _color = c;
   }

   public void drawLine(Vector3 p1, Vector3 p2, Color c1, Color c2){
      _lineData.Add(p1);
      _lineData.Add(p2);
      _colorData.Add(c1);
      _colorData.Add(c2);
   }

   public void drawLine(Vector3 p1, Vector3 p2){
      _lineData.Add(p1);
      _lineData.Add(p2);
      _colorData.Add(_color);
      _colorData.Add(_color);
   }
   
   public void OnPostRender() {
       //Debug.Log("Drawing " + _lineData.Count/2 + " lines");
       // set the current material
       _lineMaterial.SetPass( 0 );
       GL.PushMatrix();
       GL.Begin( GL.LINES );
       for(int i = 0; i < _lineData.Count; ++i){
          GL.Color(_colorData[i]);
          GL.Vertex(_lineData[i]);
       }
       GL.End();
       GL.PopMatrix();

       _lineData.Clear();
       _colorData.Clear();
   }
   
}
