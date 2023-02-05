using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public VendingMachine vendingMachine;
    public PlayerKinematicMotor player;
    public Animator a;

    public Transform spawn;

    public bool canStart = true;
    public bool lastJump = false;

    public TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
        a = vendingMachine.GetComponent<Animator>();
        //ResetGame();
        Reset();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetText(string _text)
    {
        text.text = _text;
    }

    public void Reset()
    {
        vendingMachine.Reset();
        canStart = true;

        text.text = "PRESS A";
    }

    public void StartGameStart()
    {
        //disable player

        vendingMachine.Reset();

        a.SetTrigger("Start");

        player.Activate(false);

        player.Spawn(spawn.position);

        text.text = "";
    }

    public void StartGame()
    {
        player.Activate(true);
    }

    public void TryStart(bool b)
    {
        if (lastJump != b)
        {
            lastJump = b;

            if (canStart && b)
            {
                StartGameStart();
                canStart = false;
            }
        }
    }
}
