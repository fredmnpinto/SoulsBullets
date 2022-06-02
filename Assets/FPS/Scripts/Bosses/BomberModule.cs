using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FPS.Scripts.Bosses
{
	public class BomberModule : MonoBehaviour
	{
		[SerializeField]
		private Transform _droppingSpot;
		[SerializeField]
		private string _bombPoolKey;
		[SerializeField]
		private string _bombMinionPoolKey;

		private ObjectPooler _objectPooler;

		private void Start()
		{
			_objectPooler = ObjectPooler.Instance;
		}

		public void DropBomb()
		{
			SpawnDrop(_bombPoolKey);
		}

		public void DropBombCluster()
		{
			SpawnCluster(_bombPoolKey, 3);
		}

		public void DropMinion()
		{
			SpawnDrop(_bombMinionPoolKey);
		}
		
		public void DropMinionCluster()
		{
			SpawnCluster(_bombMinionPoolKey, 3);
		}

		/*
		 * Intancia um objeto numa localização random
		 *
		 * poolKey - Chave que identifica o tipo de objeto que pretendemos instanciar
		 * randomness - 
		 */
		private void InstaciateAtLocation(string poolKey, float randomness)
		{
			var drop = _objectPooler.Spawn(poolKey);
			

			var randomFactor = Random.insideUnitCircle * randomness;
			
			drop.transform.position = _droppingSpot.position + new Vector3(randomFactor.x, 0, randomFactor.y);
			drop.transform.rotation = _droppingSpot.rotation;
		}
		
		/*
		 * "Spawna" uma unica instancia do objeto
		 *
		 * objKey - Objeto que pretendemos instanciar
		 */
		private void SpawnDrop(string objKey)
		{
			InstaciateAtLocation(objKey, 0f);
		}

		/*
		 * "Spawna" varias instancias do objeto
		 *
		 * objKey - Objeto que pretendemos instanciar
         * count - Numero de objetos que pretendemos instanciar
		 *
		 */
		private void SpawnCluster(string objKey, int count)
		{
			for (int i = 0; i < count; i++)
				InstaciateAtLocation(objKey, Mathf.Pow(2, count));
		}
	}
}