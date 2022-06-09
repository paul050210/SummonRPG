using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullDetector1 : MonoBehaviour
{
    public string targetTag = string.Empty;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(targetTag))
        {
            gameObject.SendMessageUpwards("OnCkTarget", other.gameObject, SendMessageOptions.DontRequireReceiver);
        }
    }
}
