using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    private SimplePF2D.Path path;
    private Vector3 nextPoint;
    public float speed = 1.0f;
    private bool isStationary = true;

    void Start()
    {
        path = new SimplePF2D.Path(GameObject.Find("Grid").GetComponent<SimplePathFinding2D>());
        nextPoint = Vector3.zero;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int mousePos = Controls.GetMousePos();
            path.CreatePath(transform.position, mousePos);
        }

        if (path.IsGenerated())
        {
            if (isStationary)
            {
                if (path.GetNextPoint(ref nextPoint))
                {
                    transform.position = MoveTowardsNextPoint();
                    isStationary = false;
                }
                else
                {
                    isStationary = true;
                    NormalizePosition();
                }
            }
            else
            {
                Vector2 delta = nextPoint - transform.position;
                if (delta.magnitude <= .2f && !path.GetNextPoint(ref nextPoint))
                {
                    isStationary = true;
                    NormalizePosition();
                }
                if (nextPoint != null)
                {
                    transform.position = MoveTowardsNextPoint();
                }
            }
        }
        else
        {
            isStationary = true;
        }
    }

    Vector3 MoveTowardsNextPoint()
    {
        Vector3 direction = (nextPoint - transform.position).normalized;
        Vector3 pos = transform.position + (direction * speed * Time.deltaTime);
        
        return pos;
    }

    void NormalizePosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Floor(pos.x);
        pos.y = Mathf.Floor(pos.y);
        pos.x += .5f;
        pos.y += .5f;
        transform.position = pos;
    }
}
