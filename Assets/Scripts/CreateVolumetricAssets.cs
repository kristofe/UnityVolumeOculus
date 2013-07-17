using UnityEngine;
using UnityEditor;
using System.Collections;

public class CreateVolumetricAssets : MonoBehaviour
{
    public Texture3D tex3D;
    public int n;
    public Light _light;
    public float absorption;
    public float lightIntensityScale;
    //public bool onlyGenerateTexture3D;
    public string texture3DName = "pyroclasticNoise";
    private Mesh m;

    // Use this for initialization
    public void Start() {
        Debug.Log(SystemInfo.graphicsDeviceVersion);
        if (SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL") == false) {
            Debug.LogError("The Volumetric Renderer Only Supports OpenGL!!!!");
        }
        else {
            Debug.Log("OpenGL Renderer Detected");
        }

        CreateMesh();
        //tex3D = Generate3DTexture();

        Vector4 localEyePos = transform.worldToLocalMatrix.MultiplyPoint(
                                 Camera.main.transform.position);
        Vector4 localLightPos = transform.worldToLocalMatrix.MultiplyPoint(
                                 _light.transform.position);
        renderer.material.SetVector("g_eyePos", localEyePos);
        renderer.material.SetVector("g_lightPos", localLightPos);
        renderer.material.SetFloat("g_lightIntensity", _light.intensity);
        renderer.material.SetFloat("g_absorption", absorption);
    }

    void OnWillRenderObject() {
        Vector4 localEyePos = transform.worldToLocalMatrix.MultiplyPoint(
                              Camera.main.transform.position);
        localEyePos += new Vector4(0.5f, 0.5f, 0.5f, 0.0f);

        ///localEyePos = new Vector3(0.0f, 0.0f, -1.3f);

        renderer.material.SetVector("g_eyePos", localEyePos);


        Vector4 localLightPos = transform.worldToLocalMatrix.MultiplyPoint(
                                 _light.transform.position);
        renderer.material.SetVector("g_lightPos", localLightPos);
        renderer.material.SetFloat("g_lightIntensity", _light.intensity *
                                    lightIntensityScale);
    }



    private Mesh CreateMesh() {
        m = new Mesh();
        CreateCube(m);
        m.RecalculateBounds();
        MeshFilter mf = (MeshFilter)transform.GetComponent(typeof(MeshFilter));
        mf.mesh = m;
        return m;
    }

    void CreateCube(Mesh m) {
        Vector3[] vertices = new Vector3[24];
        Vector2[] uv = new Vector2[24];
        Color[] colors = new Color[24];
        Vector3[] normals = new Vector3[24];
        int[] triangles = new int[36];

        int i = 0;
        int ti = 0;
        //Front
        vertices[i] = new Vector3(-0.5f, -0.5f, -0.5f);
        normals[i] = new Vector3(0, 0, -1);
        colors[i] = new Color(0, 0, 0);

        i++;
        vertices[i] = new Vector3(0.5f, -0.5f, -0.5f);
        normals[i] = new Vector3(0, 0, -1);
        colors[i] = new Color(1, 0, 0);

        i++;
        vertices[i] = new Vector3(0.5f, 0.5f, -0.5f);
        normals[i] = new Vector3(0, 0, -1);
        colors[i] = new Color(1, 1, 0);
        i++;

        vertices[i] = new Vector3(-0.5f, 0.5f, -0.5f);
        normals[i] = new Vector3(0, 0, -1);
        colors[i] = new Color(0, 1, 0);

        i++;

        triangles[ti++] = i - 4;
        triangles[ti++] = i - 2;
        triangles[ti++] = i - 3;
        triangles[ti++] = i - 4;
        triangles[ti++] = i - 1;
        triangles[ti++] = i - 2;

        //Back
        vertices[i] = new Vector3(-0.5f, -0.5f, 0.5f);
        normals[i] = new Vector3(0, 0, -1);
        colors[i] = new Color(0, 0, 1);

        i++;
        vertices[i] = new Vector3(0.5f, -0.5f, 0.5f);
        normals[i] = new Vector3(0, 0, -1);
        colors[i] = new Color(1, 0, 1);

        i++;
        vertices[i] = new Vector3(0.5f, 0.5f, 0.5f);
        normals[i] = new Vector3(0, 0, -1);
        colors[i] = new Color(1, 1, 1);
        i++;

        vertices[i] = new Vector3(-0.5f, 0.5f, 0.5f);
        normals[i] = new Vector3(0, 0, -1);
        colors[i] = new Color(0, 1, 1);

        i++;

        triangles[ti++] = i - 4;
        triangles[ti++] = i - 3;
        triangles[ti++] = i - 2;
        triangles[ti++] = i - 4;
        triangles[ti++] = i - 2;
        triangles[ti++] = i - 1;

        //Top
        vertices[i] = new Vector3(-0.5f, 0.5f, -0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(0, 1, 0);

        i++;
        vertices[i] = new Vector3(0.5f, 0.5f, -0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(1, 1, 0);

        i++;
        vertices[i] = new Vector3(0.5f, 0.5f, 0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(1, 1, 1);

        i++;

        vertices[i] = new Vector3(-0.5f, 0.5f, 0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(0, 1, 1);

        i++;

        triangles[ti++] = i - 4;
        triangles[ti++] = i - 2;
        triangles[ti++] = i - 3;
        triangles[ti++] = i - 4;
        triangles[ti++] = i - 1;
        triangles[ti++] = i - 2;

        //Bottom
        vertices[i] = new Vector3(-0.5f, -0.5f, -0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(0, 0, 0);

        i++;
        vertices[i] = new Vector3(0.5f, -0.5f, -0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(1, 0, 0);

        i++;
        vertices[i] = new Vector3(0.5f, -0.5f, 0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(1, 0, 1);

        i++;

        vertices[i] = new Vector3(-0.5f, -0.5f, 0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(0, 0, 1);

        i++;

        triangles[ti++] = i - 4;
        triangles[ti++] = i - 3;
        triangles[ti++] = i - 2;
        triangles[ti++] = i - 4;
        triangles[ti++] = i - 2;
        triangles[ti++] = i - 1;

        //Right
        vertices[i] = new Vector3(0.5f, -0.5f, -0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(1, 0, 0);

        i++;

        vertices[i] = new Vector3(0.5f, 0.5f, -0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(1, 1, 0);

        i++;

        vertices[i] = new Vector3(0.5f, 0.5f, 0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(1, 1, 1);

        i++;

        vertices[i] = new Vector3(0.5f, -0.5f, 0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(1, 0, 1);

        i++;

        triangles[ti++] = i - 4;
        triangles[ti++] = i - 3;
        triangles[ti++] = i - 2;
        triangles[ti++] = i - 4;
        triangles[ti++] = i - 2;
        triangles[ti++] = i - 1;

        //Left
        vertices[i] = new Vector3(-0.5f, -0.5f, -0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(0, 0, 0);

        i++;

        vertices[i] = new Vector3(-0.5f, 0.5f, -0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(0, 1, 0);

        i++;

        vertices[i] = new Vector3(-0.5f, 0.5f, 0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(0, 1, 1);

        i++;

        vertices[i] = new Vector3(-0.5f, -0.5f, 0.5f);
        normals[i] = new Vector3(0, 1, 0);
        colors[i] = new Color(0, 0, 1);

        i++;

        triangles[ti++] = i - 4;
        triangles[ti++] = i - 2;
        triangles[ti++] = i - 3;
        triangles[ti++] = i - 4;
        triangles[ti++] = i - 1;
        triangles[ti++] = i - 2;

        m.vertices = vertices;
        m.colors = colors; //Putting uv's into the normal channel to get the 
        //Vector3 type
        m.uv = uv;
        m.normals = normals;
        m.triangles = triangles;
    }

    public Texture3D Generate3DTexture() {
        float r = 0.3f;
        Texture3D texture3D = new Texture3D(n, n, n, TextureFormat.ARGB32, true);
        int size = n * n * n;
        Color[] cols = new Color[size];
        int idx = 0;

        Color c = Color.white;
        float frequency = 0.01f / n;
        float center = n / 2.0f + 0.5f;

        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                for (int k = 0; k < n; k++, ++idx) {
                    float dx = center - i;
                    float dy = center - j;
                    float dz = center - k;

                    float off = Mathf.Abs(Perlin.Turbulence(i * frequency,
                                   j * frequency,
                                   k * frequency,
                                   6));

                    float d = Mathf.Sqrt(dx * dx + dy * dy + dz * dz) / (n);
                    //c.r = c.g = c.b = c.a = ((d-off) < r)?1.0f:0.0f;
                    float p = d - off;
                    c.r = c.g = c.b = c.a = Mathf.Clamp01(r - p);
                    cols[idx] = c;
                }
            }
        }

        //for(int i = 0; i < size; i++)
        //	Debug.Log (newC[i]);
        texture3D.SetPixels(cols);
        texture3D.Apply();
        renderer.material.SetTexture("g_densityTex", texture3D);
        texture3D.filterMode = FilterMode.Trilinear;
        texture3D.wrapMode = TextureWrapMode.Clamp;
        texture3D.anisoLevel = 1;

        //Color[] cs = texture3D.GetPixels();
        //for(int i = 0; i < 10; i++)
        //	Debug.Log (cs[i]);

        return texture3D;
    }

}
