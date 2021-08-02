using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Echo : PoolCreator
{
	public Color Color { get; set; }
    public const float Width = 0.5f;

    private float timeElapsed = 0;
    private MeshRenderer mr;
    private MeshFilter mf;

    private Material echoMaterial;
    private Material echoHighlightMaterial;

    int echoHitMask;

    private static Dictionary<int, Vertex> adjacencyMatrix;

    public void Init(ContactPoint cp, float size = 1f, bool detailedEcho = false)
    {
        if (echoHitMask == 0) {
            echoHitMask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Equipment"));
        }
        mf = mf ?? GetComponent<MeshFilter>();
        if (mr == null)
        {
            mr = GetComponent<MeshRenderer>();
            foreach (Material mt in mr.materials)
            {
                if (mt.name.Contains("Highlight"))
                {
                    echoHighlightMaterial = mt;
                }
                else
                {
                    echoMaterial = mt;
                }
            }
        }

        transform.forward = -cp.normal;

		float margin = 0.01f;
        Vector3 origin = cp.point - transform.forward * margin;

        if (detailedEcho)
        {
            var hitRb = cp.otherCollider.GetComponent<Rigidbody>();
            bool isDynamic = hitRb != null && !hitRb.isKinematic;

            //0.1 is the lowest dot product rendered by the decal
            // a • b = |a||b|cos(theta)
            // theta = 1.47 rad = 84.2 deg
            float tan = 9.5f;// Mathf.Tan(84.2f);
            var m = mf.mesh;
            float maxDist = margin;

            //var n = new List<Vector3>(m.vertices).Select((vv,ii) => new { v = vv, n = m.normals[ii], i = ii }).OrderBy(v => v.v.sqrMagnitude).ToList();

            Vector2[] uv2 = m.uv2;
            Vector2[] uv3 = m.uv3;
            if (m.uv2.Length != m.vertexCount)
            {
                uv2 = new Vector2[m.vertexCount];
                for (int i = 0; i < m.vertexCount; i++)
                {
                    uv2[i] = Vector2.one * -1f;
                }

                uv3 = new Vector2[m.vertexCount];
            };
            for (int i = 0; i < m.vertexCount; i++)
            {
                Vector3 oc = m.vertices[i];
                if (oc.z > 0.1f || m.normals[i].z > -0.1f) continue;
                //if (oc.z < 0.1f || m.normals[i].z < 0.1f) continue;
                uv2[i] = Vector2.one * -1f;
                uv3[i] = Vector2.zero;
                Vector3 c = oc * size;
                float max = Mathf.Max(Mathf.Abs(c.x), Mathf.Abs(c.y));
                c = transform.rotation * c;

                float dist = margin + tan * max;
                dist = Mathf.Max(dist, 0.03f);

                Vector3 start = origin + c;

                // DEBUG
                //var rn = transform.rotation * m.normals[i];
                //Debug.DrawRay(start, rn * 0.1f, new Color(rn.x, rn.y, rn.z), 5f);
                //Debug.DrawRay(start, rn * 0.1f, new Color(-uv2[i].x, -uv2[i].y, 0), 5f);
                //Debug.DrawRay(start, transform.forward * dist * 0.1f, Color.Lerp(Color.white, Color.red, max * 2), 5f);

                RaycastHit hit;
                if (Physics.Raycast(start, transform.forward, out hit, dist, echoHitMask, QueryTriggerInteraction.Ignore))
                {
                    // TODO: Check if dynamic rb (use dict for caching),
                    // if static, can hit other static elements
                    // if dynamic, can hit objects sharing parent
                    if (!isDynamic || hit.collider == cp.otherCollider)
                    {
                        if (hit.distance < maxDist)
                        {
                            maxDist = hit.distance;
                        }
                        uv2[i] = new Vector2(oc.x + 0.5f, oc.y + 0.5f);
                        uv3[i] = new Vector2(hit.distance - margin, 0);
                    }
                }
                else
                {
                    Debug.Log("what");
                }
            }
            m.uv2 = uv2;
            m.uv3 = uv3;
            m.RecalculateNormals();
            mf.mesh = m;
            transform.localScale = new Vector3(size, size, maxDist + margin);
        }
        else
        {
            transform.localScale = new Vector3(size, size, margin * 2);
        }

        transform.parent = cp.otherCollider.transform;
        transform.position = origin;
	}

    protected virtual void Start()
    {
        if (Seed)
        {
            mf = mf ?? GetComponent<MeshFilter>();
            var m = mf.mesh;
            for (int i = 0; i < m.vertexCount; i++)
            {
                var oc = m.vertices[i];
                if (oc.z < 0.1f && m.normals[i].z < -0.1f)
                {
                    GetNeighbors(m, i, new HashSet<int>());
                    break;
                }
            }
        }
        SetMaterialProperties();
    }

    private void GetNeighbors(Mesh m, int index, HashSet<int> explored)
    {
        explored.Add(index);
        var v = m.vertices[index];
        List<int> neighbors = new List<int>();
        for (int i = 0; i < m.vertexCount; i++)
        {
            Vector3 vn = m.vertices[i];
            if (i != index && (vn - v).sqrMagnitude < 0.03125f)
            {
                if (!explored.Contains(i))
                {
                    GetNeighbors(m, i, explored);
                }
                neighbors.Add(i);
            }
            if (neighbors.Count == 8) break;
        }
        adjacencyMatrix.Add(index, new Vertex() { Neighbors = neighbors });
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
        echoMaterial.SetVector("_Radius", r);

        maxRadius = Mathf.Clamp01(timeElapsed * 1.1f) - Mathf.InverseLerp(25f, 30f, timeElapsed);
        echoHighlightMaterial?.SetFloat("_MaxRadius", maxRadius * 1.25f);
    }

    private class Vertex
    {
        public List<int> Neighbors { get; set; }
        public bool Processed { get; set; }
        public float Depth { get; set; }
    }
}
