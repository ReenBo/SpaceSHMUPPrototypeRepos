using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Set in Inspector")]
    // Удобное применение Vector2. х хранит минимальное значение, а у - максимальное
    //  значение для метода Random.Range(), который будет вызываться позже
    public Vector2 rotMinMax = new Vector2(15, 90);
    public Vector2 driftMinMax = new Vector2(.25f, 2);
    public float lifeTime = 6f; // Время в секудах существования PowerUp
    public float fadeTime = 4f;

    [Header("Set Dynamically")]
    public WeaponType type;      // Тип бонуса
    public GameObject cube;      // Ссылка на вложенный куб
    public TextMesh letter;      // Ссылка на TextMesh
    public Vector3 rotPerSecond; // Скорость вращения
    public float birthTime;

    private Rigidbody rigid;
    private BoundsCheck bndCheck;
    private Renderer cubeRend;

    void Awake()
    {
        // Получить ссылку на куб
        cube = transform.Find("Cube").gameObject;
        // Получить ссылки на другие компоненты
        letter = GetComponent<TextMesh>();
        rigid = GetComponent<Rigidbody>();
        bndCheck = GetComponent<BoundsCheck>();
        cubeRend = cube.GetComponent<Renderer>();

        // Выбрать случайную скорость
        Vector3 vel = Random.onUnitSphere; // Получить случайную скорость
        // Random.onUnitSphere возвращает вектор, указывающий на случайную точк, 
        //  находящуюся на поверхности сферы с радиусом 1м и с центром в начале координат
        vel.z = 0; // Отобразить vel на плоскочти XY
        vel.Normalize(); // Нормализация устанавливает длинну Vector3 равной 1м 
        vel *= Random.Range(driftMinMax.x, driftMinMax.y);
        rigid.velocity = vel;

        // Установить угол поворота этого игрового объекта равным 0,0,0.
        transform.rotation = Quaternion.identity; // Отсутствие поворота

        // Выбрать случайную скорость вращения для вложенного куба с использованием
        //  rotMinMax
        rotPerSecond = new Vector3(
            Random.Range(rotMinMax.x, rotMinMax.y),
            Random.Range(rotMinMax.x, rotMinMax.y),
            Random.Range(rotMinMax.x, rotMinMax.y));
        birthTime = Time.time;
    }

    void Update()
    {
        cube.transform.rotation = Quaternion.Euler(rotPerSecond * Time.time);

        // Эффект растворения куба PowerUp с течением времени
        // Со значениями default бонус существует 10 секунд, а затем прпадает за 4 секудны
        float u = (Time.time - (birthTime + lifeTime)) / fadeTime;
        // В течение lifeTime секунд значение u будет <=0. Затем оно станет положительным
        //  и через fadeTime секунд станет больше 1

        // Уничтожить бонус, если u >= 1
        if(u >= 1)
        {
            Destroy(this.gameObject);
            return;
        }

        // использовать u для определения альфа-значения кубаи буквы
        if(u > 0)
        {
            Color c = cubeRend.material.color;
            c.a = 1f - u;
            cubeRend.material.color = c;
            // Буква тоже должна растворяться, но медленее
            c = letter.color;
            c.a = 1f - (u * 0.5f);
            letter.color = c;
        }

        if(!bndCheck.isOnScreen) // За границы экрана
        {
            Destroy(gameObject);
        }
    }

    public void SetType(WeaponType wt)
    {
        // Получить WeaponDefinition из Main
        WeaponDefinition def = Main.GetWeaponDefinition(wt);
        // Установить цвет дочернего куба
        cubeRend.material.color = def.color;
        // letter.color = def.color; // Букву тоже можно окрасить в тот же цвет
        letter.text = def.letter;    // Установить отображаемую букву
        type = wt; // Установить фактический тип
    }

    public void AbsorbedBy(GameObject target)
    {
        Destroy(this.gameObject);
    }
}
