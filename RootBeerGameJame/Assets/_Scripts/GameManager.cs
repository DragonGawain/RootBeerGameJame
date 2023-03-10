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

        //text.text = "L-Joystick to move\nR-Joystick to look\nA to shoot\n\nPRESS A TO CONTINUE";
        text.text = "WASD to move\nMouse to look\nSpace to shoot\n\nPRESS SPACE TO CONTINUE";

    }

    public void SetText(string _text)
    {
        text.text = _text;
    }

    public void Reset()
    {
        vendingMachine.Reset();
        canStart = true;

        //text.text = "PRESS A";
        text.text = "PRESS SPACE";

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
