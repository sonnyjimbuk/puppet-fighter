using UnityEngine;

public class AnimationOnOff : MonoBehaviour
{
    public GameObject[] GO;
    private bool Animate = true;

    private void OnGUI()
    {
        string A = "Ragdoll";
        if (Animate)
            A = "Animate";
        if (GUI.Button(new Rect(10, 10, 100, 25), A))
        {
            if (Animate)
                Animate = false;
            else
                Animate = true;
            foreach (GameObject go in GO)
                if (go && go.TryGetComponent(out MTR_Control MTRC))
                    MTRC.Animate = Animate;
        }
    }
}