using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.FPS.AI;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

namespace FPS.Scripts.Bosses
{
	public class BomberAttackPattern : BossAttackPattern
	{
		private const float k_AttackCooldown = 2f;
		private Health _bossHealth;
		private DetectionModule _detectionModule;
		private BomberModule _bomberModule;
		private NavMeshAgent _navMeshAgent;
		private ActorsManager _actorsManager;
		[SerializeField] private float holdStillTime;

		[SerializeField] private float pissedFactor;

		private bool _isPissed;

		private readonly Hashtable _attackList = new();
		private readonly Hashtable _criticalAttackList = new();

		private float _timeOfNextAttack;

		private void Start()
		{
			Transform parent = transform.parent;
			_bossHealth = GetComponentInParent<Health>();
			_detectionModule = parent.GetComponentInChildren<DetectionModule>();
			_bomberModule = parent.Find("WeaponRoot").GetComponentInChildren<BomberModule>();
			_navMeshAgent = parent.GetComponentInParent<NavMeshAgent>();
			_actorsManager = FindObjectOfType<ActorsManager>();

			_bossHealth.OnDamaged += OnDamaged;

			_timeOfNextAttack = Time.time;

			/* Possible regular attacks */
			// Drop a regular bomb
			_attackList[AttackType.RegularAttack] = new Action(() => { _bomberModule.DropBomb(); });

			// Drop a minion to fight the player
			_attackList[AttackType.OutOfRangeAttack] = new Action(() => {
				((Action)_attackList[AttackType.RegularAttack]).Invoke();
			});

			/* Possible attacks when with critical health */
			// Drop a regular bomb but faster
			_criticalAttackList[AttackType.RegularAttack] = new Action(() =>
			{
				Task.Run(() =>
				{
					for (var i = 0; i < 5; i++)
					{
						_bomberModule.DropBombCluster();
						Task.Delay(500);
					}
				});
			});

			// Drop a minion to fight the player
			_criticalAttackList[AttackType.OutOfRangeAttack] = new Action(() => { ((Action)_criticalAttackList[AttackType.RegularAttack]).Invoke(); });
		}

		private void OnDamaged(float totalDamage, GameObject source)
		{
			if (!_isPissed && _bossHealth.IsCritical())
			{
				OnEnterCritical();
			}
		}

		override
			public void Attack()
		{
			ChooseAttack().Invoke();

			ResetCooldown();
		}

		override
			public bool CanAttack()
		{
			return _detectionModule.IsSeeingTarget && _timeOfNextAttack <= Time.time;
		}

		private void ResetCooldown()
		{
			var cdScale = 1f;

			if (_isPissed)
				cdScale = 0.75f;

			_timeOfNextAttack = Time.time + k_AttackCooldown * cdScale;
		}

		private Action ChooseAttack()
		{
			var possibleAttacks = _attackList;

			if (_isPissed) possibleAttacks = _criticalAttackList;

			bool isPlayerFarAway = !_detectionModule.IsTargetInAttackRange;
			if (isPlayerFarAway) return (Action)possibleAttacks[AttackType.OutOfRangeAttack];

			// Else executes the standard attack
			return (Action)possibleAttacks[AttackType.RegularAttack];
		}

		protected override void OnEnterCritical()
		{
			// Task.Run(() =>
			// {
			// 	// Stagger(holdStillTime).Wait();
			//
			// 	OnPissed();
			// });
			
			OnPissed();
		}

		private void OnPissed()
		{
			Debug.Log("Got pissed");
			_isPissed = true;

			_navMeshAgent.acceleration *= pissedFactor;
			_navMeshAgent.speed *= pissedFactor;
			_navMeshAgent.angularSpeed *= 1 / pissedFactor;
		}

		/**
		 * @param time - Time to stagger in seconds
		 */
		private Task Stagger(float time)
		{
			return Task.Run(() =>
			{
				Debug.Log($"Staggered for {time} seconds");
				_navMeshAgent.isStopped = true;

				Task.Delay((int)(time * 1000));

				_navMeshAgent.isStopped = false;
				_navMeshAgent.SetDestination(_actorsManager.Player.transform.position);

				Debug.Log("Pulled himself together");
			});
		}
	}
}