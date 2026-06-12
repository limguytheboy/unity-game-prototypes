using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    public bool Grounding;

    private void OnTriggerStay(Collider other)
    {
        // Check that the object isn't the player and isn't the held item
        if (CheckItem(other))
        {
            Grounding = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If the item leaving is not the held item and not the player, reset grounding
        if (CheckItem(other))
        {
            Grounding = false;
        }
    }

    private bool CheckItem(Collider other)
    {
        // Returns false if the collider matches the currently held item, true otherwise
        return other.gameObject.layer != LayerMask.NameToLayer("Item") && other.gameObject.layer != LayerMask.NameToLayer("Player");
    }
}
