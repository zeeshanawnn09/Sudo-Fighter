using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float cameraSpeed = 0.125f;
    public Vector3 offset;

    public Transform[] characters; // An array which will have all the characters


    //This function checks the active player in the game
    Transform ActiveCharacter()
    {
        foreach (Transform player in characters)
        {
            if (player.gameObject.activeInHierarchy)
            {
                return player;
            }
        }

        return null;

    }

    void LateUpdate()
    {
        if (characters == null || characters.Length == 0)
        {
            return;
        }

        Transform activePlayer = ActiveCharacter();

        if (activePlayer == null)

            return;

        Vector3 CameraPos = activePlayer.position + offset;
        CameraPos.y = transform.position.y;

        Vector3 CameraSmoothness = Vector3.Lerp(transform.position, CameraPos, cameraSpeed);
        transform.position = CameraSmoothness;
    }
}
