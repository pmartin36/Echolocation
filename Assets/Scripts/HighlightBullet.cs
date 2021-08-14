using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightBullet : PoolObject
{
    public Vector3 Direction { get; set; }
    private Rigidbody rb;

    private ParticleSystem burstPS;

    public float Speed = 20f;

    public void Start()
    {
        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
        {
            if (ps.gameObject.name == "Burst")
            {
                burstPS = ps;
            }
        }
    }

    void Update()
    {

    }

    public void Init(Vector3 pos, Vector3 dir)
    {
        rb ??= GetComponent<Rigidbody>();
        transform.position = pos;
        transform.forward = dir;
        Direction = dir;
        rb.AddForce(Direction * Speed, ForceMode.VelocityChange);
    }

	public void OnCollisionEnter(Collision collision)
	{
        ContactPoint cp = collision.GetContact(0);
        Echo e = PoolManager.Instance.Next<Echo>("Echo");
        e.Init(cp, 1f, true, 25f, 5f);
        StartCoroutine(Splatter(cp));
    }

    IEnumerator Splatter(ContactPoint cp)
    {
        float t = Time.time;
        rb.velocity = Vector3.zero;
        transform.position = cp.point;
        transform.forward = -cp.normal;
        burstPS.Emit(100);
        while (Time.time - t < 2f)
        {
            yield return null;
        }
        Recycle();
    }
}
