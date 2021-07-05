using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolCreator : MonoBehaviour, IPoolObject
{
	[field: SerializeField]
	public virtual string Key { get; set; }


	[field: SerializeField]
	public virtual int StartingCount { get; set; }

	[SerializeField]
	protected bool Seed;

	public virtual void Recycle()
	{
		PoolManager.Instance.Recycle(this);
		this.gameObject.SetActive(false);
	}

	public virtual void OnActivate()
	{

	}

	protected virtual void Awake()
    {
		if (Seed)
		{
			Seed = false;
			PoolManager.Instance.CreatePool(this);
			Recycle();
		}
    }
}
