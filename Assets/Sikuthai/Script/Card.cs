using UnityEngine;

[System.Serializable]
public class Card : MonoBehaviour
{
    public string name;
    public string suit;
    public string rank;
    public bool s = false;
    

    private void OnMouseDown()
    {
        // Reference the selected card in the game controller
        SikuThiaGameController.instance.playerCardDrop = this;
        string parentName = gameObject.transform.parent.name;
        //print(parentName);

        // Toggle selection state
        s = !s;
        if (parentName.Equals("player-hand"))
        {
            // Apply animations when the card is selected or deselected
            if (s)
            {
                AudioController.Instance.PlaySFX("tap");
                // If selected: Lift the card up, scale it up slightly, and rotate it for emphasis
                LeanTween.moveLocalY(gameObject, 50, 0.2f).setEase(LeanTweenType.easeOutBack);  // Move up with a bounce
                LeanTween.scale(gameObject, new Vector3(1.1f, 1.1f, 1f), 0.2f).setEase(LeanTweenType.easeOutQuad);  // Scale up
                LeanTween.rotateZ(gameObject, Random.Range(-5f, 5f), 0.2f).setEase(LeanTweenType.easeOutQuad);  // Slight random tilt for realism
            }
            else
            {
                AudioController.Instance.PlaySFX("tap");
                // If deselected: Lower the card back, scale it back to normal, and reset rotation
                LeanTween.moveLocalY(gameObject, 0, 0.2f).setEase(LeanTweenType.easeInQuad);  // Move down smoothly
                LeanTween.scale(gameObject, Vector3.one, 0.2f).setEase(LeanTweenType.easeInQuad);  // Scale back to original size
                LeanTween.rotateZ(gameObject, 0, 0.2f).setEase(LeanTweenType.easeInOutQuad);  // Reset rotation to 0
            }

            // Notify the game controller about the selected card
            SikuThiaGameController.instance.SelectCard(this);
        }
    }
}
