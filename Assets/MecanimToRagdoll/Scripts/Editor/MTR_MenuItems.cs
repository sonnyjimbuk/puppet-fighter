using UnityEngine;
using UnityEditor;

public class MenuItems
{
    [MenuItem("Component/Physics/Mecanim to ragdoll")]
    private static void AddMecanimToRagdoll()
    {
        Undo.AddComponent<MecanimToRagdoll>(Selection.activeGameObject);
    }

    [MenuItem("Component/Physics/Mecanim to ragdoll", true)]
    static bool ValidateAddMecanimToRagdoll()
    {
        GameObject GO = Selection.activeGameObject;
        if (GO)
        {
            Animator A = GO.GetComponent<Animator>();
            if (A)
            {
                if (A.isHuman)
                {
                    if (!GO.GetComponent<MecanimToRagdoll>() && !GO.GetComponent<MTR_Control>())
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;
    }
}