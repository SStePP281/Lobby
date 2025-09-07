using UnityEngine;

public class BaseSpringTrigger : MonoBehaviour
{
    [SerializeField] private Spring spring;

    public virtual void SetNewSpringState(SpringState newState)
    {
        spring.SetNewState(newState);
    }
}
