using UnityEngine;

namespace Assets.Environment.DayNightCycle
{
    public class LightWall : MonoBehaviour {

        Renderer _renderer;

        // Use this for initialization
        void Start () 
        {

            _renderer = GetComponent<Renderer>();
            //mat = renderer.material;

            //mat.SetColor("_EmissionColor", Color.black);

        }
	
        // Update is called once per frame
        void Update () 
        {
            if (!Input.GetKeyDown(KeyCode.L)) return;
            Color final = Color.white * Mathf.LinearToGammaSpace(4);
            _renderer.material.SetColor("_EmissionColor", final);
            DynamicGI.SetEmissive(_renderer, final);
        }
    }
}
