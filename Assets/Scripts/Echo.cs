using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Echo : PoolObject
{
	public Color Color { get; set; }
    public const float Width = 0.5f;

    private float timeElapsed = 0;
    private MeshRenderer mr;
    private MeshFilter mf;

    private Material echoMaterial;
    private Material echoHighlightMaterial;

    int echoHitMask;

    private static float margin = 0.01f;
    //0.1 is the lowest dot product rendered by the decal
    // a • b = |a||b|cos(theta)
    // theta = 1.47 rad = 84.2 deg
    // Tan(84.2f) = 9.5;
    private static float tan = 9.5f;
    private static float maxPerVertexDiff = tan * 0.125f; // 8 spaces

    private static readonly Dictionary<int, List<int>> seedAdjacencyMatrix = new Dictionary<int, List<int>>();
    private Dictionary<int, Vertex> vertexInfo;

    [SerializeField, ReadOnly]
    private float Size;
    private float Lifetime;
    private float FadeTime;

    public void Init(ContactPoint cp, float size = 1f, bool detailedEcho = false, float lifetime = 6f, float fadeTime = 4f)
    {
        var cpi = new ContactPointInfo()
        {
            HitObject = cp.otherCollider,
            Point = cp.point,
            Normal = cp.normal
        };
        Init(cpi, size, detailedEcho, lifetime, fadeTime);
    }

    public void Init(RaycastHit hit, float size = 1f, bool detailedEcho = false, float lifetime = 6f, float fadeTime = 4f)
    {
        var cpi = new ContactPointInfo()
        {
            HitObject = hit.collider,
            Point = hit.point,
            Normal = hit.normal
        };
        Init(cpi, size, detailedEcho, lifetime, fadeTime);
    }

    private void Init(ContactPointInfo cp, float size, bool detailedEcho, float lifetime, float fadeTime)
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

            vertexInfo = new Dictionary<int, Vertex>();
            foreach (var d in seedAdjacencyMatrix)
            {
                vertexInfo.Add(d.Key, new Vertex() { Neighbors = d.Value, Index = d.Key });
            }
        }

        transform.forward = -cp.Normal;
        Size = size;
        Lifetime = lifetime;
        FadeTime = fadeTime;

        Vector3 origin = cp.Point - transform.forward * margin;

        if (detailedEcho)
        {
            var hitRb = cp.HitObject.GetComponent<Rigidbody>();
            bool isDynamic = hitRb != null && !hitRb.isKinematic;

            var m = mf.mesh;
            float maxDist = margin;

            foreach (var d in vertexInfo)
            {
                d.Value.Processed = false;
                d.Value.Depth = -1;
            }

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

            Action<int> ExploreVertex = null;
            ExploreVertex = i =>
            {
                Vertex vertex;
                if (!vertexInfo.TryGetValue(i, out vertex) || vertex.Processed) return;

                Vector3 oc = m.vertices[i];
                uv2[i] = Vector2.one * -1f;
                uv3[i] = Vector2.zero;
                Vector3 c = oc * size;
                float max = Mathf.Max(Mathf.Abs(c.x), Mathf.Abs(c.y));
                c = transform.rotation * c;

                float dist = margin +  max * maxPerVertexDiff;
                dist = Mathf.Max(dist, 0.03f);

                Vector3 start = origin + c;

                // DEBUG
                //var rn = transform.rotation * m.normals[i];
                //Debug.DrawRay(start, rn * 0.1f, new Color(rn.x, rn.y, rn.z), 5f);
                //Debug.DrawRay(start, rn * 0.1f, new Color(-uv2[i].x, -uv2[i].y, 0), 5f);
                //Debug.DrawRay(start, transform.forward * dist * 0.1f, Color.Lerp(Color.white, Color.red, max * 2), 5f);

                RaycastHit hit;
                if (Physics.Raycast(start, transform.forward, out hit, dist, echoHitMask, QueryTriggerInteraction.Ignore) && Vector3.Dot(hit.normal, cp.Normal) > 0.1f)
                {
                    // TODO: Check if dynamic rb (use dict for caching),
                    // if static, can hit other static elements
                    // if dynamic, can hit objects sharing parent
                    if (!isDynamic || hit.collider == cp.HitObject)
                    {
                        bool validHit = true;
                        if (hit.distance > maxDist)
                        {
                            if (hit.distance > maxDist + maxPerVertexDiff)
                            {
                                foreach (var n in vertexInfo[i].Neighbors)
                                {
                                    ExploreVertex(n);
                                }
                                if (hit.distance > maxDist + maxPerVertexDiff)
                                {
                                    validHit = false;
                                }
                            }
                            if (validHit)
                            {
                                maxDist = hit.distance;
                            }
                        }

                        if (validHit)
                        {
                            uv2[i] = new Vector2(oc.x + 0.5f, oc.y + 0.5f);
                            uv3[i] = new Vector2(hit.distance - margin, 0);
                            vertex.Depth = hit.distance;
                        }
                    }
                }
                vertex.Processed = true;
            };

            foreach (var v in vertexInfo)
            {
                ExploreVertex(v.Key);
            }

            m.uv2 = uv2;
            m.uv3 = uv3;
            m.RecalculateNormals();
            mf.mesh = m;
            transform.localScale = new Vector3(size, size, maxDist + margin * 3f);
        }
        else
        {
            transform.localScale = new Vector3(size, size, margin * 3f);
        }

        transform.parent = cp.HitObject.transform;
        transform.position = origin;
	}

	protected override void Awake()
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
        base.Awake();
    }

	protected virtual void Start()
    {
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
            if (i != index && vn.z < 0.1f && m.normals[i].z < -0.1f && (vn - v).sqrMagnitude < 0.03125f)
            {
                if (!explored.Contains(i))
                {
                    GetNeighbors(m, i, explored);
                }
                neighbors.Add(i);
            }
            if (neighbors.Count == 8) break;
        }
        seedAdjacencyMatrix.Add(index, neighbors);
    }

    protected virtual void Update()
    {
        SetMaterialProperties();
        timeElapsed += Time.deltaTime;
        if (timeElapsed > Lifetime)
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

        if (echoHighlightMaterial != null)
        {
            float sTime = Lifetime - FadeTime;
            maxRadius = Mathf.Clamp01(timeElapsed * 1.15f) - Mathf.InverseLerp(sTime, Lifetime, timeElapsed) * 1.5f;

            echoHighlightMaterial.SetFloat("_MaxRadius", maxRadius * 1.5f);
            echoHighlightMaterial.SetFloat("_Shrinking", Mathf.Sign(timeElapsed - sTime));
        }
    }

    private class Vertex
    {
        public List<int> Neighbors { get; set; }
        public bool Processed { get; set; }
        public float Depth { get; set; }
        public int Index { get; set; }
    }

    private class ContactPointInfo
    {
        public Vector3 Normal { get; set; }
        public Vector3 Point { get; set; }
        public Collider HitObject { get; set; }
    }
}
