using UnityEngine;

[CreateAssetMenu(fileName = "New_Recipe", menuName = "Recipe")]
public class Recipe : ScriptableObject {

    public Items[] inputs;
    public Items[] outputs;

    public float craftingTime = 3f;
}
