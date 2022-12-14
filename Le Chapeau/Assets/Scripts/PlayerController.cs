using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public float jumpForce;
    public GameObject hatObject;

    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;

    [PunRPC]
    public void Initialize (Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;

        GameManager.instance.players[id - 1] = this;

        // give the first player the hat
        if(id == 1)
            GameManager.instance.GiveHat(id, true);

        // if this isnt our local player, disable physics as that's
        // controlled by the user and synced to all other clients
        if(!photonView.IsMine)
            rig.isKinematic = true;
    }

    void Update()
    {
        // the host will check if the player has won
        if(PhotonNetwork.IsMasterClient)
        {
            if(curHatTime >= GameManager.instance.timeToWin && !GameManager.instance.gameEnded)
            {
                GameManager.instance.gameEnded = true;
                GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, id);
            }
        }

        if(photonView.IsMine)
        {
            Move();

            if(Input.GetKeyDown(KeyCode.Space))
                TryJump();
            
            // track amount of time we're wearing the hat
            if(hatObject.activeInHierarchy)
                curHatTime += Time.deltaTime;
        }
    }

    // move the player along the x and z axis'
    void Move()
    {
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;

        rig.velocity = new Vector3(x, rig.velocity.y, z);
    }

    // check if we're grounded and if so, jump
    void TryJump()
    {
        // create a ray which shoots below us
        Ray ray = new Ray(transform.position, Vector3.down);

        // if we hit something then we're grounded, so jump
        if(Physics.Raycast(ray, 0.7f))
        {
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // sets the players hat active or not
    public void SetHat(bool hasHat)
    {
        hatObject.SetActive(hasHat);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(!photonView.IsMine)
            return;
        
        // did we hit another player?
        if(collision.gameObject.CompareTag("Player"))
        {
            // do they have the hat?
            if(GameManager.instance.GetPlayer(collision.gameObject).id == GameManager.instance.playerWithHat)
            {
                // can we get the hat?
                if(GameManager.instance.CanGetHat())
                {
                    // give us the hat
                    GameManager.instance.photonView.RPC("GiveHat", RpcTarget.All, id, false);
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(curHatTime);
        }
        else if(stream.IsReading)
        {
            curHatTime = (float)stream.ReceiveNext();
        }
    }
}
