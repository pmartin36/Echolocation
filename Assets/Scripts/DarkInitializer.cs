using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkInitializer : MonoBehaviour
{
    public Texture Noise;
    public Texture LUT;

    public Color Color;
    [Range(0,1)]
    public float Dark;

    void Start()
    {
        Shader.SetGlobalTexture("_DarkNoise", Noise);
        Shader.SetGlobalTexture("_DarkLUT", LUT);
        Shader.SetGlobalFloat("_Dark", Dark);
        Shader.SetGlobalColor("_DarkColor", Color);
    }

	void OnApplicationQuit()
	{
        Shader.SetGlobalFloat("_Dark", 0f);
    }

	public void SetColor(Color c)
    {
        Color = c;
        Shader.SetGlobalColor("_DarkColor", c);
    }

    public void SetDark(float d)
    {
        Dark = d;
        Shader.SetGlobalFloat("_Dark", d);
    }
}
