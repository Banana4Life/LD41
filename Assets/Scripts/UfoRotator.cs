using UnityEngine;
using UnityEngine.AI;

public class UfoRotator : MonoBehaviour
{
	public float Scaler = 1;
	
	void Update ()
	{
		var agent = GetComponentInParent<NavMeshAgent>();
		if (agent)
		{
			transform.RotateAroundLocal(Vector3.up, agent.velocity.magnitude * Scaler * Time.deltaTime);
		}
	}
}
