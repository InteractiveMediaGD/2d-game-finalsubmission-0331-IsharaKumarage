using UnityEngine;

/// <summary>
/// Manages the visual parts of a modular character.
/// Allows swapping sprites for specific body parts and weapons.
/// </summary>
public class CharacterSkinManager : MonoBehaviour
{
    [Header("Body Parts")]
    public SpriteRenderer head;
    public SpriteRenderer torso;
    public SpriteRenderer l_arm;
    public SpriteRenderer r_arm;
    public SpriteRenderer l_leg;
    public SpriteRenderer r_leg;

    [Header("Equipment")]
    public SpriteRenderer weapon;

    [Header("Pivots (For Animation)")]
    public Transform weaponPivot; // Usually the R_Arm or a dedicated Weapon object

    /// <summary>
    /// Changes the sprite of a specific body part.
    /// </summary>
    public void ChangePart(string partName, Sprite newSprite)
    {
        switch (partName.ToLower())
        {
            case "head": if (head) head.sprite = newSprite; break;
            case "torso": if (torso) torso.sprite = newSprite; break;
            case "l_arm": if (l_arm) l_arm.sprite = newSprite; break;
            case "r_arm": if (r_arm) r_arm.sprite = newSprite; break;
            case "l_leg": if (l_leg) l_leg.sprite = newSprite; break;
            case "r_leg": if (r_leg) r_leg.sprite = newSprite; break;
            case "weapon": if (weapon) weapon.sprite = newSprite; break;
        }
    }

    /// <summary>
    /// Convenience method for equipping a new sword.
    /// </summary>
    public void EquipWeapon(Sprite newWeapon)
    {
        if (weapon) weapon.sprite = newWeapon;
    }
}
