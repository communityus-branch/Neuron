using UnityEngine;

namespace Assets.Environment.DayNightCircle
{
    public class LightWall : MonoBehaviour {

        Renderer renderer;
        Material mat;

        // Use this for initialization
        void Start () 
        {
	
            renderer = GetComponent<Renderer>();
            //mat = renderer.material;

            //mat.SetColor("_EmissionColor", Color.black);

        }
	
        // Update is called once per frame
        void Update () 
        {
            if (!Input.GetKeyDown(KeyCode.L)) return;
            Color final = Color.white * Mathf.LinearToGammaSpace(4);
            renderer.material.SetColor("_EmissionColor", final);
            DynamicGI.SetEmissive(renderer, final);
        }
    }
}
