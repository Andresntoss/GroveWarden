using UnityEngine;
using UnityEngine.EventSystems; 

public class Centralize : MonoBehaviour
{
    void Update()
    {
        // Se o script estiver desativado (enabled = false), o Update não roda.
        transform.position += (transform.parent.position - transform.position) * 5 * Time.deltaTime;
    }
}