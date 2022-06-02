using System;
using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Bomb : MonoBehaviour
{
	[SerializeField] private LayerMask hitMask;
	[SerializeField] private LayerMask ownerMask;
	[SerializeField] private float damage;
	[SerializeField] private AudioClip explosionSfx;
	[SerializeField] private AudioClip spawnSfx;
	[SerializeField] private float lifeTime;
	private float _timeToDie;

	private Rigidbody _rigidbody;
	private AudioSource _audioSource;

	private DamageArea _damageArea;
	private ObjectPooler _objectPooler;

	// Start is called before the first frame update
	void Start()
	{
		OnInit();
	}

	void OnInit()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_damageArea = GetComponent<DamageArea>();
		_audioSource = GetComponent<AudioSource>();

		_objectPooler = ObjectPooler.Instance;

		_timeToDie = Time.time + lifeTime;
	}

	// Update is called once per frame
	void Update()
	{
	}

	private void FixedUpdate()
	{
		if (IsHittingTarget() || _timeToDie <= Time.time)
		{
			OnHit();
		}
	}

	private void OnEnable()
	{
		OnInit();
		PlaySpawnSfx();
	}

	/*
	* Faz barulho quando a bomba e "spawnada"
	*/
	private void PlaySpawnSfx()
	{
		AudioUtility.CreateSFX(spawnSfx, transform.position, AudioUtility.AudioGroups.EnemyAttack, 0f, 1f);
	}

	/*
	* Som da explosão da bomba
	*/
	private void PlayExplosionSfx()
	{
		AudioUtility.CreateSFX(explosionSfx, transform.position, AudioUtility.AudioGroups.EnemyAttack, 0f, 1f);
	}

	private bool IsHittable(GameObject other)
	{
		return hitMask == (hitMask | (1 << other.layer));
	}

	/*
	* Quando a bomba recebe um hit, ela explode
	*/
	void OnHit()
	{
		Explode();

		gameObject.SetActive(false);
	}

	void Explode()
	{
		PlayExplosionSfx();
		_damageArea.InflictDamageInArea(damage, transform.position, hitMask, QueryTriggerInteraction.Collide,
			gameObject);
	}

	bool IsHittingTarget()
	{
		RaycastHit[] hits = Physics.SphereCastAll(transform.position, _damageArea.AreaOfEffectDistance,
			transform.position.normalized, 0f, hitMask,
			QueryTriggerInteraction.Collide);
		foreach (var hit in hits)
		{
			if (IsHittable(hit.collider.gameObject))
			{
				return true;
			}
		}

		return false;
	}
}