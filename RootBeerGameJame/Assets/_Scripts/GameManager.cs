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

    public AudioSource mainMusic;
    public AudioSource menuMusic;

    // Start is called before the first frame update
    void Start()
    {
        a = vendingMachine.GetComponent<Animator>();
        //ResetGame();
        Reset();

        text.text = "WASD TO MOVE\nMOUSE TO LOOK\nSPACE TO SHOOT\n\nPRESS SPACE TO CONTINUE";
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

        mainMusic.Stop();
        menuMusic.Play();
    }

    public void StartGameStart()
    {
        vendingMachine.Reset();

        a.SetTrigger("Start");

        player.Activate(false);

        player.Spawn(spawn.position);

        text.text = "";
    }

    public void StartGame()
    {
        player.Activate(true);

        mainMusic.Play();
        menuMusic.Stop();
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
