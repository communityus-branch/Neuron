using UnityEngine;
using System.Collections;
using System.IO;

namespace EncodeDecode
{
    /// <summary>
    /// This class is designed to take 32 bit floating point data and get it out of a 2D render texture.
    /// As there is no way in Unity to load floating point data straight into a render texture (with out dx11) the data for each
    /// channel must be encoded into a ARGB32 format texture and then decoded via a shader into the render texture.
    ///
    /// At the moment there are some conditions that must be meet for this to work
    ///
    /// 1 - The data must be 32 bit floating point but the render texture format can be float or half.
    ///
    /// 2 - The encode/decode step only works on data in the range 0 - 0.9999 but the function will find the highest number and normalize the 
    ///     the data if its over 1 and then un-normalize it in the shader. This way you can have numbers greater than 1. 
    ///     The function will also find the lowest number and if its below 0 it will add this value to all the data so the lowest number is 0.
    ///     This way you can have numbers lower than 0. This only works when copying data into a render texture. 
    ///     When trying to get it out of a render texture you will need to make sure the data is in the range 0 - 0.9999
    ///     as there is not easy way to iterate over the texture and find the min and max values. 
    ///
    /// 3 - When trying encode/decode values it does not seem to work on values equal to 1 so Ive stated the max range as 0.9999.
    ///
    /// 4 - Ive added the ability to load a raw file and copy the data into a render texture. 
    ///     You can load 32 bit or 16 bit data. 16 bit data can be big endian or little endian
    /// </summary>
    public class ReadFloat
    {

        /// <summary>
        /// The material used to encode the render texture.
        /// </summary>
	    Material m_readToFloat;

        /// <summary>
        /// The texture to hold the encoded render texture.
        /// </summary>
        RenderTexture m_encodeTex;

        /// <summary>
        /// A texture to read the encoded data.
        /// </summary>
        Texture2D m_readTex;

        /// <summary>
        /// The current size of the maps.
        /// </summary>
        int m_width, m_height;

        /// <summary>
        /// Create a new object that can read data from a render texture of dimensions w and h.
        /// </summary>
        public ReadFloat(int w, int h)
        {
            m_width = w;
            m_height = h;

            Shader shader = Shader.Find("EncodeFloat/ReadToFloat");

            if (shader == null)
                Debug.Log("Could not find shader EncodeFloat/ReadToFloat. Did you change the shaders name?");

            m_readToFloat = new Material(shader);

            m_encodeTex = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            m_encodeTex.filterMode = FilterMode.Point;

            m_readTex = new Texture2D(w, h, TextureFormat.ARGB32, false, true);

        }

        /// <summary>
        /// Resize to new dimensions.
        /// </summary>
        public void Resize(int w, int h)
        {

            m_width = w;
            m_height = h;

            m_readTex.Resize(w, h, TextureFormat.ARGB32, false);

            m_encodeTex.Release();
            m_encodeTex = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            m_encodeTex.filterMode = FilterMode.Point;

        }

        /// <summary>
        /// Must realse render texture before object destroyed.
        /// </summary>
        public void Release()
        {
            m_encodeTex.Release();
        }
	
	    /// <summary>
        /// This will read the values in tex into data array. Data must be in the range 0 - 0.9999
	    /// </summary>
	    /// <param name="tex">The texture to read from</param>
	    /// <param name="channels">The number of channels in the texture</param>
        /// <param name="data">The data to write into. Size must width * height * channels.</param>
	    public void ReadFromRenderTexture(RenderTexture tex, int channels, float[] data)
	    {
		
		    if(channels < 1 || channels > 4)
		    {
			    Debug.Log("Channels must be 1, 2, 3, or 4");
			    return;
		    }
		
		    int w = tex.width;
		    int h = tex.height;

            if (w != m_width || h != m_height)
            {
                Debug.Log("Render texture not the correct dimensions");
                return;
            }
		
		    Vector4 factor = new Vector4(1.0f, 1.0f/255.0f, 1.0f/65025.0f, 1.0f/160581375.0f);
		
		    for(int i = 0; i < channels; i++)
		    {
			    //enocde data in tex into encodeTex
                Graphics.Blit(tex, m_encodeTex, m_readToFloat, i);
			    //Read encoded values into a normal texture where we can retrive them
                RenderTexture.active = m_encodeTex;
                m_readTex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                m_readTex.Apply();
			    RenderTexture.active = null;
			
			    //decode each pixel in readTex into a single float for the current channel
			    for(int x = 0; x < w; x++)
			    {
				    for(int y = 0; y < h; y++)
				    {
                        data[(x + y * w) * channels + i] = Vector4.Dot(m_readTex.GetPixel(x, y), factor);
				    }
			    }
		    }
	    }
	
    }

}
