using UnityEngine;

// Повесь на игрока
// Переназначает кости брони на кости скелета игрока
public class SkinnedArmorEquip : MonoBehaviour
{
    [Header("Корневая кость скелета игрока")]
    public Transform skeletonRoot; // перетащи сюда PT_Hips или корневую кость игрока

    // Singleton
    public static SkinnedArmorEquip Instance { get; private set; }

    void Awake() => Instance = this;

    // ── Надеть броню — переназначить кости ──
    public GameObject EquipSkinnedArmor(GameObject armorPrefab)
    {
        if (armorPrefab == null || skeletonRoot == null)
        {
            Debug.LogError("SkinnedArmorEquip: armorPrefab или skeletonRoot не задан!");
            return null;
        }

        // Спавним броню как дочерний объект корневой кости
        GameObject armorInstance = Instantiate(armorPrefab, skeletonRoot.parent);
        armorInstance.transform.localPosition = Vector3.zero;
        armorInstance.transform.localRotation = Quaternion.identity;
        armorInstance.transform.localScale    = Vector3.one;

        // Собираем все кости скелета игрока в словарь по имени
        Transform[] playerBones = skeletonRoot.GetComponentsInChildren<Transform>();
        var boneMap = new System.Collections.Generic.Dictionary<string, Transform>();
        foreach (Transform bone in playerBones)
            boneMap[bone.name] = bone;

        // Переназначаем кости на каждом SkinnedMeshRenderer брони
        SkinnedMeshRenderer[] armorRenderers = armorInstance.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in armorRenderers)
        {
            Transform[] newBones = new Transform[renderer.bones.Length];
            for (int i = 0; i < renderer.bones.Length; i++)
            {
                string boneName = renderer.bones[i].name;
                if (boneMap.TryGetValue(boneName, out Transform playerBone))
                    newBones[i] = playerBone;
                else
                {
                    Debug.LogWarning($"Кость '{boneName}' не найдена в скелете игрока!");
                    newBones[i] = renderer.bones[i]; // оставляем оригинальную
                }
            }
            renderer.bones = newBones;

            // Переназначаем rootBone
            if (renderer.rootBone != null && boneMap.TryGetValue(renderer.rootBone.name, out Transform rootBone))
                renderer.rootBone = rootBone;
        }

        Debug.Log($"✅ Броня {armorPrefab.name} надета и кости переназначены!");
        return armorInstance;
    }
}