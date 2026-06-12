using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class ClientNetworkTranform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}


