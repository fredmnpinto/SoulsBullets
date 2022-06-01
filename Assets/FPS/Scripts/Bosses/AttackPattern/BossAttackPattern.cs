using System;
using Unity.VisualScripting;
using UnityEngine;

namespace FPS.Scripts.Bosses
{
	public abstract class BossAttackPattern : MonoBehaviour
	{
		protected enum AttackType
		{
			RegularAttack,
			OutOfRangeAttack,
			NearTargetAttack,
		}

		public abstract void Attack();

		public abstract bool CanAttack();

		protected abstract void OnEnterCritical();
	}
}