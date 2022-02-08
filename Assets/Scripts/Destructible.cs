using UnityEngine;
using UnityEngine.Events;
using Mirror;
public class Destructible : NetworkBehaviour
{

    public UnityAction<int> HitPointChange;


   [SerializeField]
   private int m_MaxHitPoint;


    [SerializeField]
    private GameObject m_DestroySfx;

    public int MaxHitPoint => m_MaxHitPoint;

    public int HitPoint => currentHitPoint;
    private int currentHitPoint;


    [SyncVar(hook = nameof(ChangeHitPoint))]
    private int syncCurrentHitPoint;

    public override void OnStartServer()
    {
        base.OnStartServer();

        syncCurrentHitPoint = m_MaxHitPoint;
        currentHitPoint = m_MaxHitPoint;


    }

    [Server]
    public void SvApplyDamage(int damage)
    {
        syncCurrentHitPoint -= damage;


        if (syncCurrentHitPoint <= 0)
        {
            if (m_DestroySfx != null)
            {
              GameObject sfx =   Instantiate(m_DestroySfx, transform.position, Quaternion.identity);
              NetworkServer.Spawn(sfx);
            }

            Destroy(gameObject);
        }
    }


    private void ChangeHitPoint(int oldValue, int newValue)
    {
        currentHitPoint = newValue;
        HitPointChange?.Invoke(newValue);
    }

    [SyncVar(hook =  "T")]
    public NetworkIdentity Owner;

    private void T(NetworkIdentity oldValue, NetworkIdentity newVAlue)
    {

    }
}
