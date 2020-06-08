using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIShipController : MonoBehaviour {

    GameMaster gameMaster;

    //[SerializeField] bool hostileToPlayer = true;
    private GameObject player;
    private ShipCore core;
    private ShipTurret turret;
    private GameObject shipPivot;
    private int playerMask = (1 << 8) + (1 << 10) + (1 << 11);
    [SerializeField] private float targetOrbitDistance = 50f;

    void Start() {
        gameMaster = GameObject.Find("GameMaster").GetComponent<GameMaster>();
        player = GameObject.Find("Corvette (player)"); //bad
        core = GetComponent<ShipCore>();
        turret = transform.Find("Ship Pivot").Find("Corvette Body").Find("Corvette Turret").GetComponent<ShipTurret>(); //bad
        shipPivot = transform.Find("Ship Pivot").gameObject; //bad
    }
    
    void Update() {
        // Cheap AI Control
        core.SetThrottle(1f);
        Vector3 firstDir = (player.transform.position - transform.position).normalized;
        firstDir = Quaternion.Euler(0f, -shipPivot.transform.eulerAngles.y, 0f) * firstDir;
        Vector3 orbitOffset;
        if (firstDir.x > 0f) {
            orbitOffset = Quaternion.Euler(0f, -90f, 0f) * (player.transform.position - transform.position).normalized;
        } else {
            orbitOffset = Quaternion.Euler(0f, 90f, 0f) * (player.transform.position - transform.position).normalized;
        }
        Vector3 targetDir = (player.transform.position + orbitOffset * targetOrbitDistance - transform.position).normalized;
        targetDir = Quaternion.Euler(0f, -shipPivot.transform.eulerAngles.y, 0f) * targetDir;
        if (targetDir.x > 0f) {
            core.SetRudder(1f);
        } else {
            core.SetRudder(-1f);
        }
        turret.UpdateAim(player.transform.position + Quaternion.Euler(0f, player.transform.Find("Ship Pivot").eulerAngles.y, 0f) * new Vector3(0f, 0.25f, -0.65f), player.GetComponent<ShipCore>().GetVelocity());
        if (turret.IfOnTarget()) {
            turret.FireCannon(playerMask);
        }
        if (core.GetHealth() == 0f) {
            //player.GetComponent<PlayerShipController>().IncreaseScore();
            gameMaster.EnemyDestroyed();
            Destroy(gameObject);
        }
    }
}
