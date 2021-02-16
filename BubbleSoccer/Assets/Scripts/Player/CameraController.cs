using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    [SerializeField]
    private Vector3 offSetFromPlayer;

    [SerializeField]
    private float timeToMoveToGoal = 1f;
    private bool lost = false;
    private float counter = 0f;
    private float defaultFixedTimeScale = 0f;
    private void LateUpdate()
    {
        if (player && !lost)
        {
            transform.position = player.position + offSetFromPlayer;
        }

    }

    public void EnemyKickedLostGame(Vector3 position)
    {
        lost = true;
        StartCoroutine(MoveToGoal(position));
    }
    public void PlayerFell(Vector3 position)
    {
        lost = true;
        transform.position = position + offSetFromPlayer;
    }
    public void ResetCamera()
    {
        lost = false;
    }
    IEnumerator MoveToGoal(Vector3 newPosition)
    {
        newPosition += offSetFromPlayer;
        Time.timeScale = 0.1f;
        defaultFixedTimeScale = Time.fixedDeltaTime;
        Time.fixedDeltaTime *= Time.timeScale;
        counter = 0f;
        while (counter <= timeToMoveToGoal)
        {
            counter += (Time.deltaTime * 10);
            transform.position = Vector3.Lerp(transform.position, newPosition, counter/timeToMoveToGoal);
            yield return null;
        }
        transform.position = newPosition;
        yield return new WaitForSeconds(0.5f);
        Time.fixedDeltaTime = defaultFixedTimeScale;
    }
}
