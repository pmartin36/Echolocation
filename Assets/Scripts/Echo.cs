using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Echo : PoolCreator
{
	public Color Color { get; set; }
    public const float Width = 0.5f;

    private float timeElapsed = 0;
    private MeshRenderer mr;
    private MeshFilter mf;

    int echoHitMask;

    public void Init(ContactPoint cp, float size = 1f, bool detailedEcho = false)
    {
        if (echoHitMask == 0) {
            echoHitMask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Equipment"));
        }
        mf = mf ?? GetComponent<MeshFilter>();
        mr = mr ?? GetComponent<MeshRenderer>();

        transform.forward = -cp.normal;

		float margin = 0.05f;

        var hitRb = cp.otherCollider.GetComponent<Rigidbody>();
        bool isDynamic = hitRb != null && !hitRb.isKinematic;

		//0.1 is the lowest dot product rendered by the decal
		// a • b = |a||b|cos(theta)
		// theta = 1.47 rad = 84.2 deg
		float tan = 9.5f;// Mathf.Tan(84.2f);
		var m = mf.mesh;
		float maxDist = margin;
		Vector2[] uv2 = m.uv2.Length == m.vertexCount ? m.uv2 : new Vector2[m.vertexCount];
		Vector3 origin = cp.point - transform.forward * margin;
		for (int i = 0; i < m.vertexCount; i++)
		{
			var c = m.vertices[i];
			if (c.z > 0.1f) continue;
			c *= size;
			float max = Mathf.Max(Mathf.Abs(c.x), Mathf.Abs(c.y));
			c = transform.rotation * c;
			uv2[i] = Vector2.zero;

			float dist = margin + tan * max;
			Vector3 start = origin + c;
			RaycastHit hit;
			//Debug.DrawRay(start, transform.forward * dist * 0.1f, Color.Lerp(Color.white, Color.red, max * 2), 5f);
			if (Physics.Raycast(start, transform.forward, out hit, dist, echoHitMask, QueryTriggerInteraction.Ignore))
			{
				// TODO: Check if dynamic rb (use dict for caching),
				// if static, can hit other static elements
				// if dynamic, can hit objects sharing parent
				if (!isDynamic || hit.collider == cp.otherCollider)
				{
					if (maxDist < hit.distance)
					{
						maxDist = hit.distance;
					}
					uv2[i] = Vector2.one * -1;
				}
			}
		}
		m.uv2 = uv2;
		mf.mesh = m;

		transform.localScale = new Vector3(size, size, maxDist+margin);
        transform.parent = cp.otherCollider.transform;
        transform.position = origin;
	}

    protected virtual void Start()
    {
        SetMaterialProperties();
    }

    protected virtual void Update()
    {
        SetMaterialProperties();
        timeElapsed += Time.deltaTime;
        if (timeElapsed > 30f)
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
        float maxRadius = -0.1f + Mathf.InverseLerp(0.2f, 1f, timeElapsed * 0.8f) * 1.1f;
        var r = new Vector4(
            maxRadius,
            0.25f + Mathf.InverseLerp(0f, 0.5f, timeElapsed * 0.8f) * 0.75f,
            timeElapsed,
            1
        );
        mr.material.SetVector("_Radius", r);

        maxRadius = Mathf.Clamp01(timeElapsed * 1.1f) - Mathf.InverseLerp(28f, 30f, timeElapsed);
        maxRadius = Mathf.Clamp01(maxRadius) * 0.1f;
    }
}
