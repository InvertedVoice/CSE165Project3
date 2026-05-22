using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Debug = UnityEngine.Debug;

public class AgentController : MonoBehaviour
{
    public NavMeshSurface surface;
    public NavMeshAgent agent;
    public Animator anim;
    public Transform target;

    void Start()
    {
        agent.enabled = false;
        Invoke("BakeAndEnable", 3f);
    }

    void BakeAndEnable()
    {
        surface.BuildNavMesh();
        Debug.Log("NavMesh baked!");

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            Debug.Log("Agent snapped to NavMesh at: " + hit.position);
        }

        agent.enabled = true;
    }

    void Update()
    {
        if (!agent.enabled) return;
        if (target != null && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
            anim.SetBool("Walking", agent.velocity.magnitude > 0.05f);
        }
    }

    public void SetDestination(Vector3 pos)
    {
        target.position = pos;
    }
}