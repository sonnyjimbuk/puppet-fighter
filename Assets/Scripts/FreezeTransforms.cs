using UnityEngine;
using System.Collections.Generic;

// Attach this at runtime to a player to lock its entire transform hierarchy in place.
public class FreezeTransforms : MonoBehaviour
{
    struct Pose { public Vector3 pos; public Quaternion rot; public Vector3 scale; }

    private Transform[] allTransforms;
    private Pose[] poses;

    void Awake()
    {
        // collect all transforms in this subtree
        var list = new List<Transform>(GetComponentsInChildren<Transform>(true));
        allTransforms = list.ToArray();
        poses = new Pose[allTransforms.Length];

        // capture initial poses
        for (int i = 0; i < allTransforms.Length; i++)
        {
            var t = allTransforms[i];
            poses[i].pos = t.position;
            poses[i].rot = t.rotation;
            poses[i].scale = t.localScale;
        }
    }

    // enforce the poses after everything else has run
    void LateUpdate()
    {
        for (int i = 0; i < allTransforms.Length; i++)
        {
            var t = allTransforms[i];
            t.position = poses[i].pos;
            t.rotation = poses[i].rot;
            t.localScale = poses[i].scale;
        }
    }
}
