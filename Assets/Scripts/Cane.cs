using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cane : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody Rigidbody;
    private Transform holder;

    private PastMovementInfo PastMovementInfo;

    [SerializeField]
    private CapsuleCollider Handle;

    [SerializeField]
    private SphereCollider Tip;

    private float TimeSinceLastCheck = 10f;
    private bool CheckStayCollisions = false;
    private Dictionary<Collider, List<ContactPointInfo>> RecentCollisions = new Dictionary<Collider, List<ContactPointInfo>>();

    private LayerMask layerMask;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        PastMovementInfo = new PastMovementInfo(0.5f, this.transform);
        layerMask = ~(1 >> LayerMask.NameToLayer("Equipment") | 1 >> LayerMask.NameToLayer("Player"));
        transform.parent = null;
        StartCoroutine(ContactPointEviction());
    }

    void Update()
    {
        CheckStayCollisions = TimeSinceLastCheck > 0.25f;
        TimeSinceLastCheck += Time.deltaTime;
        
        PastMovementInfo.Update(this.transform);
    }

    public void Bind(Transform t) {
        holder = t;
    }

    private List<ContactPointInfo> GetClosestContactPoints(Collision collision)
    {
        ContactPoint[] cps = new ContactPoint[collision.contactCount];
        collision.GetContacts(cps);
        List<ContactPointInfo> validContactPoints = new List<ContactPointInfo>();
        foreach (var cp in cps)
        {
            var dist = Vector3.Distance(cp.thisCollider.transform.position, cp.point);
            var cpi = new ContactPointInfo()
            {
                ContactPoint = cp,
                DistanceFromCenterAtCollision = dist,
                CollisionTime = Time.time,
                OffsetFromBase = cp.point - transform.position
            };
            if (!RecentCollisions.ContainsKey(cp.thisCollider))
            {
                RecentCollisions.Add(cp.thisCollider, new List<ContactPointInfo>() { cpi });
                validContactPoints.Add(cpi);
            }
            else
            {
                bool validContact = true;
                var collisions = RecentCollisions[cp.thisCollider];
                // check recent contact to see if object has moved enough
                for (int i = 0; i < collisions.Count; i++)
                {
                    var c = collisions[i];
                    if (c.GetHashCode() == cpi.GetHashCode()) {
                        if ((c.DistanceMovedSinceCollision < ContactPointInfo.EvictionDistance && Vector3.Dot(cp.normal, c.ContactPoint.normal) > 0.9f)
                            || c.DistanceMovedSinceCollision < 0.25f)
                        {
                            validContact = false;
                        }
                        else
                        {
                            collisions.RemoveAt(i);
                        }
                    }
                }

                if (validContact)
                {
                    // check other contact this frame on same object 
                    for(int i = 0; i < validContactPoints.Count; i++)
                    {
                        var othercpi = validContactPoints[i];
                        if (othercpi.GetHashCode() == cp.GetHashCode())
                        {
                            if (othercpi.DistanceFromCenterAtCollision < cpi.DistanceFromCenterAtCollision)
                            {
                                validContact = false;
                            }
                            else
                            {
                                validContactPoints.RemoveAt(i);
                            }
                        }

                    }

                    if (validContact)
                    {
                        validContactPoints.Add(cpi);
                        collisions.Add(cpi);
                    }
                }
            }
        }
        return validContactPoints;
    }

    public void OnCollisionEnter(Collision collision)
	{
        foreach (var cp in GetClosestContactPoints(collision))
        {
		    Echo e = PoolManager.Instance.Next<Echo>("Echo");
            e.Init(cp.ContactPoint, 1f, true);
            
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (CheckStayCollisions)
        {
            // it might be valuable to have a counter on each contactpointinfo for consistency,
            // however, this is more performant
            foreach (var cp in GetClosestContactPoints(collision))
            {
                Echo e = PoolManager.Instance.Next<Echo>("Echo");
                e.Init(cp.ContactPoint, 1f, false);
            }
            TimeSinceLastCheck = 0f;
        }
    }

	public void OnCollisionExit(Collision collision)
	{
		
	}

    private IEnumerator ContactPointEviction()
    {
        var evictionDelay = new WaitForSeconds(0.25f);
        var lastPosition = transform.position;
        var lastRotation = transform.rotation;
        while (gameObject.activeInHierarchy)
        {
            foreach (var entry in RecentCollisions)
            {
                for (int i = entry.Value.Count - 1; i >= 0; i--)
                {
                    ContactPointInfo cp = entry.Value[i];
                    Quaternion diff = transform.rotation * Quaternion.Inverse(lastRotation);
                    Vector3 rotatedPoint = diff * cp.OffsetFromBase;

                    var d = (transform.position - lastPosition).magnitude 
                        + (cp.OffsetFromBase - rotatedPoint).magnitude;
                    cp.DistanceMovedSinceCollision += d;

                    if (cp.DistanceMovedSinceCollision > ContactPointInfo.EvictionDistance)
                    {
                        entry.Value.RemoveAt(i);
                    }
                }
            }

            lastPosition = transform.position;
            lastRotation = transform.rotation;
            yield return evictionDelay;
        }
    }

    private class ContactPointInfo
    {
        public static float EvictionDistance = 2f;
        public ContactPoint ContactPoint { get; set; }
        public float DistanceFromCenterAtCollision { get; set; }
        public float CollisionTime { get; set; }
        public float DistanceMovedSinceCollision { get; set; }
        public Vector3 OffsetFromBase { get; set; }

        public override int GetHashCode()
		{
            return GetCode(ContactPoint);
		}

        public static int GetCode(ContactPoint c) {
            return c.thisCollider.gameObject.GetInstanceID()
                    + c.otherCollider.gameObject.GetInstanceID() >> 16
                    + c.normal.GetHashCode();
        }
	}
}
