using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Part - еще один сериализуемый класс подобно WeaponDefinition,
///  предназначеный для хранения данных
/// </summary>

[System.Serializable]
public class Part
{
    // Значение этих полей должны определяться в инспекторе
    public string name; // Имя части
    public float health; // Степень стойкости части
    public string[] protectedBy; // Другие части, защищающие эту

    // Эти два поля инициализируються автоматически в Statrt().
    // Кэширование, как здесь, ускоряет получение необходимых данных
    [HideInInspector] // Не позволяет следующему полю появиться в инспекторе
    public GameObject go; // Игровой объект этой части
    [HideInInspector]
    public Material mat; // Материал для отображения повреждений 
}

/// <summary>
/// Enemy_4 создаётся за верхней границей экрана, выбирает случайную точку
///  на экране и перемещается к ней. Переместившись, выбирает новую случайную
///  точку и продолжается двигаться, пока игрок его не уничтожит.
/// </summary>

public class Enemy_4 : Enemy
{
    [Header("Set in Inspector: Enemy_4")]
    public Part[] parts; // Массив частей, составляющих корабль

    private Vector3 p0, p1;     // Две точки для интерполяции
    private float timeStart;    // Время создания этого коробля
    private float duration = 4; // Продолжительность перемищения

    void Start()
    {
        // Начальная позициия уже выбрана в Main.SpawnEnemy(),
        //  поэтому запишем ее как начальное значение в р0 и р1
        p0 = p1 = pos;

        InitMovement();

        // Записать в кэш игровой объект и материал каждой части в parts
        Transform t;
        foreach (Part prt in parts)
        {
            t = transform.Find(prt.name);
            if (t != null)
            {
                prt.go = t.gameObject;
                prt.mat = prt.go.GetComponent<Renderer>().material;
            }
        }
    }

    void InitMovement()
    {
        p0 = p1; // Переписать p1 в p0
        // Выбрать новую точку p1 на экране
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        // Сбросить время
        timeStart = Time.time;
    }

    public override void Move()
    {
        // Этот метод переопределяет Enemy.Move() и реализует
        //  линейную интерполяцию
        float u = (Time.time - timeStart) / duration;

        if (u >= 1)
        {
            InitMovement();
            u = 0;
        }

        u = 1 - Mathf.Pow(1 - u, 2); // Применить плавное замедление
        pos = (1 - u) * p0 + u * p1;  // Простая линейная интерполяция
    }

    // Эти две функции выполняют поиск части в массиве parts
    //  по имени или ссылке на игровой объект
    Part FindPart(string n)
    {
        foreach (Part prt in parts)
        {
            if (prt.name == n) return prt;
        }
        return null;
    }

    Part FindPart(GameObject go)
    {
        foreach (Part prt in parts)
        {
            if (prt.go == go) return prt;
        }
        return null;
    }

    // Эти функции возвращают true, если данная часть уничтожена
    bool Destroyed(GameObject go)
    {
        return Destroyed(FindPart(go));
    }

    bool Destroyed(string n)
    {
        return Destroyed(FindPart(n));
    }

    bool Destroyed(Part prt)
    {
        // Если ссылка на часть не была переданна, то вернуть true
        if (prt == null) return true;
        // Вернуть результат сравнения: prt.health <= 0
        // Если prt.health <= 0, вернуть true 
        return prt.health <= 0;
    }

    // Окрашивает в красный только одну часть, а не весь корабль
    void ShowLocalizedDamage(Material m)
    {
        m.color = Color.red;
        damageDoneTime = Time.time + showDamageDuration;
        showingDamage = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero":
                Projectile p = other.GetComponent<Projectile>();

                // Если вражеский корабль за границами экрана,
                //  не наносит ему повреждений.
                if (!bndCheck.isOnScreen)
                {
                    Destroy(other);
                    break;
                }

                // Поразить вражеский корабль
                GameObject goHit = collision.contacts[0].thisCollider.gameObject;
                Part prtHit = FindPart(goHit);
                if (prtHit == null)
                {
                    goHit = collision.contacts[0].otherCollider.gameObject;
                    prtHit = FindPart(goHit);
                }
                // Проверить, защищена ли еще эта часть корабля
                if (prtHit.protectedBy != null)
                {
                    foreach (string s in prtHit.protectedBy)
                    {
                        // Если хотя бы одна часть еще не разрушена ...
                        if (!Destroyed(s))
                        {
                            // ... не наносить повреждений этой части
                            Destroy(other); // Уничтожить снаряд ProjectileHero
                            return; // выйти, не повреждая Enemy_4
                        }
                    }
                }

                // Эта часть не защищена, нанести ей повреждение
                // Получить разрущающую силу из Projectile.type и Main.WEAP_DICT
                prtHit.health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                // Показать эффект попадания в часть
                ShowLocalizedDamage(prtHit.mat);
                if (prtHit.health <= 0)
                {
                    // Вместо разрушения всего корабля, деактивировать
                    //  уничтоженную часть
                    prtHit.go.SetActive(false);
                }
                // Проверить был ли корабль полностью разрушен
                bool allDestroyed = true; // Предположить, что разрушен
                foreach (Part prt in parts)
                {
                    if (!Destroyed(prt)) // Если какая-то часть существует ...
                    {
                        allDestroyed = false; // записать false в allDestroyed
                        break; // и прервать цикл
                    }
                }
                if (allDestroyed) // Если корабль разрушен полностью ...
                { // ... уведомить объект-одиночку, что этот корабль разрушен 
                    Main.S.ShipDestroyed(this);
                    // Уничтожить этот объект Enemy
                    Destroy(this.gameObject);
                }
                Destroy(other); // Уничтожить снаряд ProjectileHero
                break;
        }
    }
}
