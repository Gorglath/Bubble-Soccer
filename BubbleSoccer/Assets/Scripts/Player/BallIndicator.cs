using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallIndicator : MonoBehaviour
{
    [SerializeField]
    private GameObject arrow;

    [SerializeField]
    private float distanceFromCenter = 1f;
    //helpers
    private Transform target;
    private Vector3 screenPosition;
    private Vector3 screenCenter;
    private Vector3 screenBounds;
    private float relation = 0f;
    private float angle = 0f;
    private float cos = 0f;
    private float sin = 0f;
    private float m = 0f;

    private void Update()
    {
        if (target)
        {
            CheckIndicator();
        }
    }
    private void CheckIndicator()
    {
        screenPosition = Camera.main.WorldToScreenPoint(target.position);

        if (screenPosition.z > 0 &&
            screenPosition.x > 0 && screenPosition.x < Screen.width &&
            screenPosition.y > 0 && screenPosition.y < Screen.height)
        {
            if (arrow.activeSelf)
            {
                arrow.SetActive(false);
            }
        }
        else
        {
            arrow.SetActive(true);
            if (screenPosition.z < 0)
            {
                screenPosition *= -1;
            }
            
            screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;

            screenPosition -= screenCenter;

            angle = Mathf.Atan2(screenPosition.y, screenPosition.x);
            angle -= 90f * Mathf.Deg2Rad;

            cos = Mathf.Cos(angle);
            sin = -Mathf.Sin(angle);

            screenPosition = screenCenter + new Vector3(sin * 150, cos * 150, 0);

            m = cos / sin;

            screenBounds = screenCenter * 0.9f;


            if (cos > 0f)
            {
                screenPosition = new Vector3(screenBounds.y / m, screenBounds.y, 0);
            }
            else
            {
                screenPosition = new Vector3(-screenBounds.y / m, -screenBounds.y, 0);
            }

            if (screenPosition.x > screenBounds.x)
            {
                screenPosition = new Vector3(screenBounds.x, screenBounds.x * m, 0);
            }
            else if (screenPosition.x < -screenBounds.x)
            {
                screenPosition = new Vector3(-screenBounds.x, -screenBounds.x * m, 0);
            }

            //screenPosition += screenCenter;

            arrow.transform.localPosition = screenPosition;
            arrow.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
           
        }
    }
    public void SetTarget(Transform arrowTarget)
    {
        target = arrowTarget;
    }
}
