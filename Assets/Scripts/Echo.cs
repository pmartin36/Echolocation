using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Echo : PoolCreator
{
	public Color Color { get; set; }
    public const float Width = 0.5f;

    private float timeElapsed = 0;
    private MeshRenderer mr;

    protected virtual void Start()
    {
        mr = GetComponent<MeshRenderer>();
        SetMaterialProperties();
    }

    protected virtual void Update()
    {
        SetMaterialProperties();
        timeElapsed += Time.deltaTime;
        if (timeElapsed > 5f)
        {
            Recycle();
        }
    }

	public override void OnActivate()
	{
		base.OnActivate();
        timeElapsed = 0f;
    }

    private void SetMaterialProperties()
    {
        var r = new Vector4(
            -0.1f + Mathf.InverseLerp(0.2f, 1f, timeElapsed * 0.8f) * 1.1f,
            0.25f + Mathf.InverseLerp(0f, 0.5f, timeElapsed * 0.8f) * 0.75f,
            timeElapsed,
            1
        );
        mr.material.SetVector("_Radius", r);
    }
}
