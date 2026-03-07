using UnityEngine;
using System.Collections;
using TMPro;

// На объекте должны быть:
// - Collider с IsTrigger = false (для raycast из Interaction)
// - Tag = "Soul"
// - ParticleSystem (дочерний объект Particles)
public class SoulPuddle : MonoBehaviour
{
    [Header("Настройки")]
    public int soulsAmount = 0;         // задаётся через Initialize()

    [Header("Визуал")]
    public TextMeshPro amountText;      // 3D текст над орбой (опционально)
    public float bobSpeed    = 1.5f;
    public float bobHeight   = 0.15f;
    public float rotateSpeed = 60f;

    [Header("Частицы — летят к игроку")]
    public ParticleSystem ambientParticles;   // фоновые частицы вокруг орбы
    public int   collectParticleCount = 20;   // сколько частиц выпустить при подборе
    public float particleFlySpeed     = 8f;   // скорость полёта к игроку
    public float particleFlyTime      = 0.6f; // время полёта

    [Header("Звук")]
    public AudioClip pickupSound;

    private SoulManager  _manager;
    private Transform    _player;
    private Vector3      _startPos;
    private AudioSource  _audioSource;
    private bool         _collected = false;

    // ── Вызывается из SoulManager при спавне ──
    public void Initialize(int amount, SoulManager manager)
    {
        soulsAmount = amount;
        _manager    = manager;

        if (amountText != null)
            amountText.text = $"✨ {amount:N0}";
    }

    void Start()
    {
    _startPos = transform.position;

    _audioSource = GetComponent<AudioSource>();
    if (_audioSource == null)
        _audioSource = gameObject.AddComponent<AudioSource>();
    _audioSource.playOnAwake  = false;
    _audioSource.spatialBlend = 1f;

    // Ищем игрока
    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
    if (playerObj != null)
    {
        _player  = playerObj.transform;
        // ── Если manager не задан через Initialize — ищем сами ──
        if (_manager == null)
            _manager = playerObj.GetComponent<SoulManager>();
    }

    if (amountText != null)
        amountText.text = $"✨ {soulsAmount:N0}";
    }

    void Update()
    {
        if (_collected) return;

        // Покачивание
        float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Вращение
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    // ── Вызывается из Interaction.cs по кнопке E ──
    public void Collect()
    {
        if (_collected) return;
        _collected = true;

        if (_audioSource != null && pickupSound != null)
            _audioSource.PlayOneShot(pickupSound);

        // Останавливаем фоновые частицы
        if (ambientParticles != null)
            ambientParticles.Stop();

        // Запускаем эффект полёта частиц к игроку
        if (_player != null)
            StartCoroutine(FlyParticlesToPlayer());
        else
            FinishCollect();
    }

    IEnumerator FlyParticlesToPlayer()
    {
        // Скрываем визуал орбы сразу
        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(false);
        if (amountText != null) amountText.gameObject.SetActive(false);

        // Спавним частицы которые летят к игроку
        for (int i = 0; i < collectParticleCount; i++)
        {
            StartCoroutine(SpawnFlyingParticle(i * 0.03f)); // небольшая задержка между частицами
        }

        // Ждём пока все долетят
        yield return new WaitForSeconds(particleFlyTime + collectParticleCount * 0.03f);

        FinishCollect();
    }

    IEnumerator SpawnFlyingParticle(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Создаём простой объект-частицу
        GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        particle.transform.position = transform.position + Random.insideUnitSphere * 0.3f;
        particle.transform.localScale = Vector3.one * Random.Range(0.04f, 0.1f);

        // Убираем коллайдер
        Destroy(particle.GetComponent<Collider>());

        // Синий материал
        Renderer rend = particle.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Standard")); // fallback
            mat.color = new Color(0.3f, 0.62f, 1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.3f, 0.62f, 1f) * 2f);
            rend.material = mat;
        }

        // Летим к игроку
        float elapsed = 0f;
        Vector3 startPos = particle.transform.position;

        while (elapsed < particleFlyTime && particle != null && _player != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / particleFlyTime;

            // Дуга полёта (поднимаемся и летим)
            Vector3 target = _player.position + Vector3.up * 1f;
            Vector3 arc    = Vector3.Lerp(startPos, target, t);
            arc.y += Mathf.Sin(t * Mathf.PI) * 0.5f;

            particle.transform.position = arc;
            particle.transform.localScale = Vector3.one * Mathf.Lerp(0.08f, 0.02f, t);

            yield return null;
        }

        if (particle != null) Destroy(particle);
    }

    void FinishCollect()
    {
        // Передаём души игроку
        if (_manager != null)
            _manager.RecoverSouls(soulsAmount);

        Destroy(gameObject, 0.1f);
    }
}