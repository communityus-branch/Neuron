using UnityEngine;
using System.Collections;

namespace OceanBRDF
{
    /// <summary>
    /// The ocean game object.
    /// Is responsible for creating the ocean mesh and other
    /// resources. Controls the wave spectrum object that
    /// will create the actual height maps.
    /// 
    /// The following wind speed, wave amp and wave age settings 
    /// can be used to control the look of the waves from rough 
    /// seas to calm lakes.
    ///
    /// WARNING - not all combinations of numbers makes sense 
    /// and the waves will not always look correct.
    /// </summary>
    public class Ocean : MonoBehaviour
    {

        /// <summary>
        /// The ocean shader just needs the suns direction.
        /// </summary>
        [SerializeField]
        GameObject m_sun;

        /// <summary>
        /// The material to render the ocean mesh with.
        /// The script needs to bind the data it generates
        /// like the height maps to the material.
        /// </summary>
        [SerializeField]
        Material m_oceanMat;

        /// <summary>
        /// Color of the sea.
        /// </summary>
        [SerializeField]
        Color m_seaColor = new Color(10.0f / 255.0f, 40.0f / 255.0f, 120.0f / 255.0f, 1.0f);

        /// <summary>
        /// World space y position for the see level.
        /// </summary>
        [SerializeField]
        float m_seaLevel = 0.0f;

        /// <summary>
        /// Ansiotrophic filtering on wave textures.
        /// </summary>
        [SerializeField]
        int m_ansio = 2;

        /// <summary>
        /// The distance that mipmap levels on wave textures 
        /// fades to highest mipmap.  A neg number will disable this.
        /// </summary>
        [SerializeField]
        float m_lodFadeDist = 2000.0f;

        /// <summary>
        /// The resolution of the grid used for the ocean.
        /// </summary>
        [SerializeField]
        int m_resolution = 128;

        /// <summary>
        /// A higher number will push more of the mesh verts 
        /// closer to center of grid were player is. 
        /// Must be >= 1.
        /// </summary>
        [SerializeField]
        float m_bias = 2.0f;

        /// <summary>
        /// Fourier grid size. Must be pow2 number.
        /// </summary>
        [SerializeField]
        int m_fourierGridSize = 128;

        /// <summary>
        /// A higher wind speed gives greater swell to the waves.
        /// </summary>
        [SerializeField]
        float m_windSpeed = 8.0f;

        /// <summary>
        /// Scales the height of the waves.
        /// </summary>
        //[SerializeField]
        float m_waveAmp = 1.0f;

        /// <summary>
        /// A lower number means the waves last longer and 
        /// will build up larger waves.
        /// </summary>
        [SerializeField]
        float m_inverseWaveAge = 0.84f;

        /// <summary>
        /// The waves are made up of 4 layers of heights
        /// at different wave lengths. These grid sizes 
        /// are basically the wave length for each layer.
        /// </summary>
        [SerializeField]
        Vector4 m_gridSizes = new Vector4(5488, 392, 28, 2);

        /// <summary>
        /// The game object that contains the mesh.
        /// </summary>
        GameObject m_grid;

        /// <summary>
        /// Wave object to generate the height maps.
        /// </summary>
        WaveSpectrumGPU m_waves;

        int m_frameCount = 0;

        void Start()
        {
            m_waves = new WaveSpectrumGPU(m_fourierGridSize, m_windSpeed, m_waveAmp, m_inverseWaveAge, m_ansio, m_gridSizes);

            if (m_resolution * m_resolution >= 65000)
            {
                m_resolution = (int)Mathf.Sqrt(65000);
                Debug.Log("Grid resolution set to high. Setting resolution to the maxium allowed(" + m_resolution.ToString() + ")");
            }

            if (m_bias < 1.0f)
            {
                m_bias = 1.0f;
                Debug.Log("bias must not be less than 1, changing to 1");
            }

            Mesh mesh = CreateRadialGrid(m_resolution, m_resolution);

            float far = Camera.main.farClipPlane;

            m_grid = new GameObject("Ocean Grid");
            m_grid.AddComponent<MeshFilter>();
            m_grid.AddComponent<MeshRenderer>();
            m_grid.GetComponent<Renderer>().material = m_oceanMat;
            m_grid.GetComponent<MeshFilter>().mesh = mesh;
            m_grid.transform.localScale = new Vector3(far, 1, far);//Make radial grid have a radius equal to far plane

            m_oceanMat.SetVector("_GridSizes", m_waves.gridSizes);
            m_oceanMat.SetFloat("_MaxLod", m_waves.mipMapLevels);
            m_oceanMat.SetTexture("_Variance", m_waves.variance);

        }

        void Update()
        {
            //These are work arounds for some bugs (or design?) in Unity 4.0 - 4.2. 
            //If your running this in a later version they may have been fixed??
            //In a Unity dx9 build graphics blit does not seam to have any effect on the first frame.
            //The waveSpectrum object uses graphics blit to initilize some render textures.
            //Call init() to do this but it must be called on the second frame. Strange.
            //This does not seem to be needed in a dx11 build
            if (m_frameCount == 1)
                m_waves.Init();

            m_frameCount++;

            m_waves.SimulateWaves(Time.realtimeSinceStartup);

            m_oceanMat.SetTexture("_Map0", m_waves.map0);
            m_oceanMat.SetTexture("_Map1", m_waves.map1);
            m_oceanMat.SetTexture("_Map2", m_waves.map2);
            m_oceanMat.SetFloat("_LodFadeDist", m_lodFadeDist);
            m_oceanMat.SetColor("_SeaColor", m_seaColor);
            m_oceanMat.SetVector("_VarianceMax", m_waves.varianceMax);

            //This makes sure the grid is always centered were the player is
            Vector3 pos = Camera.main.transform.position;
            pos.y = m_seaLevel;

            m_grid.transform.localPosition = pos;

        }

        void OnDestroy()
        {
            //Release render texture memory to avoid leaks
            m_waves.Release();
        }

        /// <summary>
        /// Creates a radial grid with the required segments.
        /// </summary>
        Mesh CreateRadialGrid(int segementsX, int segementsY)
        {

            Vector3[] vertices = new Vector3[segementsX * segementsY];
            Vector3[] normals = new Vector3[segementsX * segementsY];
            Vector2[] texcoords = new Vector2[segementsX * segementsY]; //not used atm

            float TAU = Mathf.PI * 2.0f;
            float r;
            for (int x = 0; x < segementsX; x++)
            {
                for (int y = 0; y < segementsY; y++)
                {
                    r = (float)x / (float)(segementsX - 1);
                    r = Mathf.Pow(r, m_bias);

                    normals[x + y * segementsX] = new Vector3(0, 1, 0);

                    vertices[x + y * segementsX].x = r * Mathf.Cos(TAU * (float)y / (float)(segementsY - 1));
                    vertices[x + y * segementsX].y = 0.0f;
                    vertices[x + y * segementsX].z = r * Mathf.Sin(TAU * (float)y / (float)(segementsY - 1));
                }
            }

            int[] indices = new int[segementsX * segementsY * 6];

            int num = 0;
            for (int x = 0; x < segementsX - 1; x++)
            {
                for (int y = 0; y < segementsY - 1; y++)
                {
                    indices[num++] = x + y * segementsX;
                    indices[num++] = x + (y + 1) * segementsX;
                    indices[num++] = (x + 1) + y * segementsX;

                    indices[num++] = x + (y + 1) * segementsX;
                    indices[num++] = (x + 1) + (y + 1) * segementsX;
                    indices[num++] = (x + 1) + y * segementsX;

                }
            }

            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.uv = texcoords;
            mesh.normals = normals;
            mesh.triangles = indices;

            return mesh;

        }

    }
	
}
