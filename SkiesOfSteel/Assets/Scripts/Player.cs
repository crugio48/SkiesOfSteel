
using Unity.Collections;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    private NetworkVariable<FixedString32Bytes> _username = new NetworkVariable<FixedString32Bytes>();

    public void SetUsername(FixedString32Bytes username)
    {
        _username.Value = username;
    }

    public FixedString32Bytes GetUsername()
    {
        return _username.Value;
    }
}
