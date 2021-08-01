using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Set in Inspector: Enemy")]
    public float speed = 10f; // Скорость в м/с
    public float fireRate = 0.3f; // Секунд между выстрелами (Не используется)
    public float health = 10;
    public int score = 100; // Очки за уничтожение этого корабля
    public float showDamageDuration = 0.1f; // Длительность эффекта попадания в секунду
    public float powerUpDropChance = 1f; // Вероятность сброса бонуса

    [Header("Set Dynamically")]
    public Color[] originalColors;
    public Material[] materials; // Все материалы игрового объекта и его потомков
    public bool showingDamage = false;
    public float damageDoneTime; // Время прекращения отображения эффекта
    public bool notifiedOfDestruction = false;

    protected BoundsCheck bndCheck;

    void Awake() {
        bndCheck = GetComponent<BoundsCheck>();
        // Получить материалы и цвет этого игрового объекта и его потомков
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for(int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }
    }

    // Это свойство: метод, действующий как поле
    public Vector3 pos {
        get {
            return (this.transform.position);
        }
        set {
            this.transform.position = value;
        }
    }

    void Update() {
        Move();

        if(showingDamage && Time.time > damageDoneTime)
        {
            UnShowDamage();
        }

        if( bndCheck != null && bndCheck.offDown) {
            // Корабль за нижней границей, поэтому его нужно уничтожить
            Destroy(gameObject);
        }
    }

    public virtual void Move() {
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    void OnCollisionEnter(Collision collision) {
        GameObject otherGO = collision.gameObject;
        switch (otherGO.tag)
        {
            case "ProjectileHero":
                Projectile p = otherGO.GetComponent<Projectile>();

                // Если вражеский корабль за границами экрана,
                //  не наносит ему повреждений.
                if (!bndCheck.isOnScreen)
                {
                    Destroy(otherGO);
                    break;
                }

                // Поразить вражеский корабль
                ShowDamage();
                // Получить разрущающую силу из WEAP_DICT в классе Main.
                health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                if(health <= 0)
                {
                    // Сообщить объекту-одиночке Main об уничтожении
                    if (!notifiedOfDestruction) Main.S.ShipDestroyed(this);
                    notifiedOfDestruction = true;
                    // Уничтожить вражеский корабль
                    Destroy(this.gameObject);
                }
                Destroy(otherGO);
                break;

            default:
                print("Enemy hit by non-ProjectileHero: " + otherGO.name);
                break;
        }
    }

    void ShowDamage()
    {
        foreach(Material m in materials) m.color = Color.red;
        showingDamage = true;
        damageDoneTime = Time.time + showDamageDuration;
    }

    void UnShowDamage()
    {
        for(int i=0; i<materials.Length; i++)
        {
            materials[i].color = originalColors[i];
        }
        showingDamage = false;
    }
}
