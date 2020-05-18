using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimplePF2D;

public class PathFollower : MonoBehaviour
{
    public float speed = 1.0f;
    public float distToTargetNextPoint = 0.1f;

    protected Path path;
    private Vector3 nextPoint;
    private bool isStationary = true;
    protected PathManager pathMan;

    void Start()
    {
        path = new Path(GameObject.Find("Grid").GetComponent<SimplePathFinding2D>());
        pathMan = GameObject.Find("PathManager").GetComponent<PathManager>();
        nextPoint = Vector3.zero;
    }

    void Update()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            Vector3Int mousePos = Controls.GetMousePos();
            path.CreatePath(transform.position, mousePos);
        }*/
    }

    Vector3 MoveTowardsNextPoint()
    {
        Vector3 direction = (nextPoint - transform.position).normalized;
        return transform.position + (direction * speed * Time.deltaTime);
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

    protected virtual void MoveAlongPath()
    {
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
                if (delta.magnitude <= distToTargetNextPoint && !path.GetNextPoint(ref nextPoint))
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

    protected virtual void CreatePath(Vector3 start, Vector3 end)
    {
        path.CreatePath(start, end);
    }
}
