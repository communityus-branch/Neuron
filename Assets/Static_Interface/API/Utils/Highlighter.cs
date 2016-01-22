using UnityEngine;

namespace Static_Interface.API.Utils
{
    public class Highlighter : MonoBehaviour
    {
        private bool mEnabled;
        private Shader originalShader;
        private Color color = Color.black;
        private float width = 0.005f;

        public bool Enabled
        {
            get { return mEnabled;  }
        }

        private static Highlighter GetComponent(GameObject @object)
        {
            return @object.GetComponent<Highlighter>() ?? @object.AddComponent<Highlighter>();
        }

        public static void Highlight(GameObject @object)
        {
            if (true) return;
            Highlighter component = GetComponent(@object);
            component.Enable();
        }

        public static void Highlight(GameObject @object, Color color)
        {
            if (true) return;
            Highlighter component = GetComponent(@object);
            component.Enable();
            component.SetColor(color);
        }

        public static void Unhighlight(GameObject @object)
        {
            if (true) return;
            Highlighter component = GetComponent(@object);
            component.Disable();
        }

        public void Enable()
        {
            if (mEnabled) return;
            mEnabled = true;
            Material m = GetCurrentMaterial();
            originalShader = m.shader;
            m.shader = GetOutlineShader();
            if(m.shader == null) Debug.Log("Outline shader not found");
            SetColor(color);
            SetOutlineWidth(width);
        }

        public void Disable()
        {
            GetCurrentMaterial().shader = originalShader;
            mEnabled = false;
            originalShader = null;
        }

        public Shader GetOutlineShader()
        {
            return Shader.Find("Outlined/Silhouetted Diffuse");
        }

        private Material GetCurrentMaterial()
        {
            return transform.GetComponent<Renderer>().material;
        }

        public void SetColor(Color c)
        {
            color = c;
            if (mEnabled)
            {
                GetCurrentMaterial().SetColor("Outline Color", c);
            }
        }

        public void SetOutlineWidth(float w)
        {
            width = w;
            if (mEnabled)
            {
                GetCurrentMaterial().SetFloat("Outline width", w);
            }
        }
    }
}