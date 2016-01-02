using UnityEngine;

namespace Assets.Environment.DayNightCircle
{
    public class SetSunLight : MonoBehaviour {

        public Renderer lightwall;

        Material sky;

        public Renderer water;

        public ParticleSystem stars;
        public Transform worldProbe;

        // Use this for initialization
        void Start () 
        {

            sky = RenderSettings.skybox;

        }

        // Update is called once per frame
        void Update () 
        {
            stars.transform.rotation = transform.rotation;
	
            Vector3 tvec = Camera.main.transform.position;
            worldProbe.transform.position = tvec;

            water.material.mainTextureOffset = new Vector2(Time.time / 100, 0);
            water.material.SetTextureOffset("_DetailAlbedoMap", new Vector2(0, Time.time / 80));
        }
    }
}
