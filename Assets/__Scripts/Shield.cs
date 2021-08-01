using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float rotationPerSecond = 0.1f;

    [Header("Set Dynamically")]
    public int levelShown = 0;

    // Скрытые переменные не появляющиеся в инспекторе
    Material mat;

    void Start() {
        mat = GetComponent<Renderer>().material;
    }

    void Update() {
        // прочитать текущую мощность защитного поля из объекта-одиночки Hero 
        int currlevel = Mathf.FloorToInt(Hero.S.shieldLevel);
        // Если она отличается от levelShown ... 
        if (levelShown != currlevel) {
            levelShown = currlevel;
            // Скорректировать смещение в текстуре, чтобы отобразить поле
            // с другой мощностью
            mat.mainTextureOffset = new Vector2(0.2f * levelShown, 0);
        }
        // Поворачивать поле в каждом кадре с постоянной скоростью
        float rZ = -(rotationPerSecond * Time.time * 360) % 360f;
        transform.rotation = Quaternion.Euler(0, 0, rZ);
    }
}
