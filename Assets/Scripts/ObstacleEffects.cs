using UnityEngine;
using System.Collections;

[System.Serializable]
public class EffectSettings
{
    [Header("Particle Settings")]
    public int particleCount = 10;
    public float particleSpeed = 5f;
    public float particleLifetime = 2f;
    public Vector3 particleScale = Vector3.one * 0.1f;
    public Color[] particleColors = { Color.white };
    
    [Header("Physics")]
    public bool usePhysics = true;
    public float explosionForce = 3f;
    public float upwardForce = 1f;
    
    [Header("Animation")]
    public float fadeSpeed = 1f;
    public bool rotateParticles = true;
    public float rotationSpeed = 180f;
}

public enum EffectType
{
    Destruction,
    Impact,
    Ricochet,
    WaterSpray,
    Smoke,
    Sparks,
    Glass,
    Wood,
    Metal,
    Plastic,
    Fabric,
    Stone
}

public class ObstacleEffects : MonoBehaviour
{
    [Header("Effect Configurations")]
    public EffectSettings destructionEffect;
    public EffectSettings impactEffect;
    public EffectSettings ricochetEffect;
    
    [Header("Material-Specific Effects")]
    public EffectSettings woodEffect;
    public EffectSettings metalEffect;
    public EffectSettings glassEffect;
    public EffectSettings plasticEffect;
    public EffectSettings stoneEffect;
    public EffectSettings fabricEffect;
    
    [Header("Special Effects")]
    public EffectSettings waterSprayEffect;
    public EffectSettings smokeEffect;
    public EffectSettings sparkEffect;
    
    [Header("Audio")]
    public AudioClip[] impactSounds;
    public AudioClip[] destructionSounds;
    public AudioClip[] ricochetSounds;
    public AudioClip[] materialSounds;
    
    [Header("Screen Effects")]
    public bool useScreenShake = false;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;
    
    private AudioSource audioSource;
    private Camera playerCamera;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find player camera for screen shake
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        InitializeDefaultEffects();
    }
    
    void InitializeDefaultEffects()
    {
        // Initialize effect settings if they are null
        if (destructionEffect == null) destructionEffect = new EffectSettings();
        if (impactEffect == null) impactEffect = new EffectSettings();
        if (ricochetEffect == null) ricochetEffect = new EffectSettings();
        if (woodEffect == null) woodEffect = new EffectSettings();
        if (metalEffect == null) metalEffect = new EffectSettings();
        if (glassEffect == null) glassEffect = new EffectSettings();
        if (plasticEffect == null) plasticEffect = new EffectSettings();
        if (stoneEffect == null) stoneEffect = new EffectSettings();
        if (fabricEffect == null) fabricEffect = new EffectSettings();
        if (waterSprayEffect == null) waterSprayEffect = new EffectSettings();
        if (smokeEffect == null) smokeEffect = new EffectSettings();
        if (sparkEffect == null) sparkEffect = new EffectSettings();
        
        // Initialize default destruction effect if not set
        if (destructionEffect.particleColors == null || destructionEffect.particleColors.Length == 0)
        {
            destructionEffect.particleColors = new Color[] { Color.gray, Color.white };
        }
        
        // Initialize material-specific effects
        InitializeMaterialEffects();
    }
    
    void InitializeMaterialEffects()
    {
        // Wood effect - brown particles
        if (woodEffect.particleColors == null || woodEffect.particleColors.Length == 0)
        {
            woodEffect.particleColors = new Color[] { 
                new Color(0.6f, 0.4f, 0.2f), // Brown
                new Color(0.8f, 0.6f, 0.3f)  // Light brown
            };
        }
        
        // Metal effect - gray/silver particles with sparks
        if (metalEffect.particleColors == null || metalEffect.particleColors.Length == 0)
        {
            metalEffect.particleColors = new Color[] { 
                Color.gray,
                Color.white,
                new Color(1f, 1f, 0f) // Yellow sparks
            };
        }
        
        // Glass effect - clear/white particles
        if (glassEffect.particleColors == null || glassEffect.particleColors.Length == 0)
        {
            glassEffect.particleColors = new Color[] { 
                new Color(0.9f, 0.9f, 1f, 0.7f), // Light blue
                new Color(1f, 1f, 1f, 0.8f)      // White
            };
        }
        
        // Plastic effect - colorful particles
        if (plasticEffect.particleColors == null || plasticEffect.particleColors.Length == 0)
        {
            plasticEffect.particleColors = new Color[] { 
                Color.red, Color.blue, Color.green, Color.yellow
            };
        }
        
        // Stone effect - gray/brown dust
        if (stoneEffect.particleColors == null || stoneEffect.particleColors.Length == 0)
        {
            stoneEffect.particleColors = new Color[] { 
                new Color(0.5f, 0.5f, 0.5f), // Gray
                new Color(0.4f, 0.3f, 0.2f)  // Brown dust
            };
        }
        
        // Fabric effect - fiber particles
        if (fabricEffect.particleColors == null || fabricEffect.particleColors.Length == 0)
        {
            fabricEffect.particleColors = new Color[] { 
                Color.white, 
                new Color(0.9f, 0.9f, 0.8f) // Off-white
            };
        }
    }
    
    public void PlayEffect(EffectType effectType, Vector3 position, Vector3 normal = default)
    {
        EffectSettings settings = GetEffectSettings(effectType);
        
        if (settings != null)
        {
            StartCoroutine(CreateEffect(settings, position, normal));
        }
        
        // Play appropriate sound
        PlayEffectSound(effectType);
        
        // Screen shake if enabled
        if (useScreenShake && (effectType == EffectType.Destruction || effectType == EffectType.Impact))
        {
            StartCoroutine(ScreenShake());
        }
    }
    
    EffectSettings GetEffectSettings(EffectType effectType)
    {
        switch (effectType)
        {
            case EffectType.Destruction: return destructionEffect;
            case EffectType.Impact: return impactEffect;
            case EffectType.Ricochet: return ricochetEffect;
            case EffectType.WaterSpray: return waterSprayEffect;
            case EffectType.Smoke: return smokeEffect;
            case EffectType.Sparks: return sparkEffect;
            case EffectType.Glass: return glassEffect;
            case EffectType.Wood: return woodEffect;
            case EffectType.Metal: return metalEffect;
            case EffectType.Plastic: return plasticEffect;
            case EffectType.Stone: return stoneEffect;
            case EffectType.Fabric: return fabricEffect;
            default: return destructionEffect;
        }
    }
    
    IEnumerator CreateEffect(EffectSettings settings, Vector3 position, Vector3 normal)
    {
        GameObject[] particles = new GameObject[settings.particleCount];
        
        // Create particles
        for (int i = 0; i < settings.particleCount; i++)
        {
            particles[i] = CreateParticle(settings, position, normal);
        }
        
        // Animate particles
        float elapsed = 0f;
        
        while (elapsed < settings.particleLifetime)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / settings.particleLifetime;
            
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] != null)
                {
                    AnimateParticle(particles[i], settings, normalizedTime);
                }
            }
            
            yield return null;
        }
        
        // Clean up particles
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] != null)
            {
                Destroy(particles[i]);
            }
        }
    }
    
    GameObject CreateParticle(EffectSettings settings, Vector3 position, Vector3 normal)
    {
        GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        particle.transform.position = position + Random.insideUnitSphere * 0.5f;
        particle.transform.localScale = settings.particleScale;
        particle.transform.rotation = Random.rotation;
        
        // Set random color from available colors
        Color particleColor = settings.particleColors[Random.Range(0, settings.particleColors.Length)];
        
        Material particleMaterial = new Material(Shader.Find("Standard"));
        particleMaterial.color = particleColor;
        
        // Add emission for bright particles
        if (particleColor.r + particleColor.g + particleColor.b > 2f)
        {
            particleMaterial.EnableKeyword("_EMISSION");
            particleMaterial.SetColor("_EmissionColor", particleColor * 0.5f);
        }
        
        particle.GetComponent<MeshRenderer>().material = particleMaterial;
        
        // Add physics if enabled
        if (settings.usePhysics)
        {
            Rigidbody rb = particle.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            
            // Calculate explosion direction
            Vector3 explosionDir = (particle.transform.position - position).normalized;
            if (explosionDir == Vector3.zero)
            {
                explosionDir = Random.insideUnitSphere.normalized;
            }
            
            // Add upward bias
            explosionDir += Vector3.up * settings.upwardForce;
            
            // Apply force
            rb.AddForce(explosionDir * settings.explosionForce * Random.Range(0.5f, 1.5f), ForceMode.Impulse);
            
            // Add random torque for rotation
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }
        
        return particle;
    }
    
    void AnimateParticle(GameObject particle, EffectSettings settings, float normalizedTime)
    {
        // Fade out over time
        MeshRenderer renderer = particle.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Color currentColor = renderer.material.color;
            currentColor.a = 1f - (normalizedTime * settings.fadeSpeed);
            renderer.material.color = currentColor;
        }
        
        // Rotate if enabled
        if (settings.rotateParticles)
        {
            particle.transform.Rotate(Vector3.one * settings.rotationSpeed * Time.deltaTime);
        }
        
        // Scale down over time for some effects
        float scaleMultiplier = Mathf.Lerp(1f, 0.1f, normalizedTime * 0.5f);
        particle.transform.localScale = settings.particleScale * scaleMultiplier;
    }
    
    void PlayEffectSound(EffectType effectType)
    {
        AudioClip[] soundArray = null;
        
        switch (effectType)
        {
            case EffectType.Impact:
                soundArray = impactSounds;
                break;
            case EffectType.Destruction:
                soundArray = destructionSounds;
                break;
            case EffectType.Ricochet:
                soundArray = ricochetSounds;
                break;
            default:
                soundArray = materialSounds;
                break;
        }
        
        if (soundArray != null && soundArray.Length > 0 && audioSource != null)
        {
            AudioClip clipToPlay = soundArray[Random.Range(0, soundArray.Length)];
            audioSource.PlayOneShot(clipToPlay);
        }
    }
    
    IEnumerator ScreenShake()
    {
        if (playerCamera == null) yield break;
        
        Vector3 originalPosition = playerCamera.transform.position;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            
            float strength = shakeIntensity * (1f - elapsed / shakeDuration);
            Vector3 shakeOffset = Random.insideUnitSphere * strength;
            shakeOffset.z = 0; // Don't shake on Z axis
            
            playerCamera.transform.position = originalPosition + shakeOffset;
            
            yield return null;
        }
        
        playerCamera.transform.position = originalPosition;
    }
    
    // Special effect methods
    public void CreateWaterSpray(Vector3 position, Vector3 direction, float intensity = 1f)
    {
        StartCoroutine(CreateWaterSprayEffect(position, direction, intensity));
    }
    
    IEnumerator CreateWaterSprayEffect(Vector3 position, Vector3 direction, float intensity)
    {
        int dropletCount = Mathf.RoundToInt(waterSprayEffect.particleCount * intensity);
        
        for (int i = 0; i < dropletCount; i++)
        {
            GameObject droplet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            droplet.transform.position = position;
            droplet.transform.localScale = Vector3.one * Random.Range(0.05f, 0.15f);
            
            // Water material
            Material waterMat = new Material(Shader.Find("Standard"));
            waterMat.color = new Color(0.3f, 0.6f, 1f, 0.7f);
            waterMat.SetFloat("_Metallic", 0f);
            waterMat.SetFloat("_Glossiness", 0.8f);
            droplet.GetComponent<MeshRenderer>().material = waterMat;
            
            // Physics
            Rigidbody dropletRb = droplet.AddComponent<Rigidbody>();
            dropletRb.mass = 0.05f;
            
            Vector3 sprayDirection = direction + Random.insideUnitSphere * 0.5f;
            sprayDirection.y = Mathf.Abs(sprayDirection.y); // Ensure upward component
            
            dropletRb.AddForce(sprayDirection * intensity * 8f, ForceMode.Impulse);
            
            // Gravity effect
            dropletRb.useGravity = true;
            
            // Destroy after falling
            Destroy(droplet, 3f);
            
            // Small delay between droplets for realistic spray
            yield return new WaitForSeconds(0.01f);
        }
    }
    
    public void CreateSmokeCloud(Vector3 position, float duration = 3f)
    {
        StartCoroutine(CreateSmokeEffect(position, duration));
    }
    
    IEnumerator CreateSmokeEffect(Vector3 position, float duration)
    {
        int smokeParticleCount = smokeEffect.particleCount;
        GameObject[] smokeParticles = new GameObject[smokeParticleCount];
        
        // Create smoke particles
        for (int i = 0; i < smokeParticleCount; i++)
        {
            GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smoke.transform.position = position + Random.insideUnitSphere * 1f;
            smoke.transform.localScale = Vector3.one * Random.Range(0.5f, 1.5f);
            
            // Smoke material
            Material smokeMat = new Material(Shader.Find("Standard"));
            smokeMat.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            smoke.GetComponent<MeshRenderer>().material = smokeMat;
            
            smokeParticles[i] = smoke;
        }
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            for (int i = 0; i < smokeParticles.Length; i++)
            {
                if (smokeParticles[i] != null)
                {
                    // Move smoke upward and outward
                    smokeParticles[i].transform.position += Vector3.up * 2f * Time.deltaTime;
                    smokeParticles[i].transform.position += Random.insideUnitSphere * 0.5f * Time.deltaTime;
                    
                    // Scale up and fade out
                    float scale = Mathf.Lerp(0.5f, 2f, normalizedTime);
                    smokeParticles[i].transform.localScale = Vector3.one * scale;
                    
                    Material mat = smokeParticles[i].GetComponent<MeshRenderer>().material;
                    Color color = mat.color;
                    color.a = Mathf.Lerp(0.5f, 0f, normalizedTime);
                    mat.color = color;
                }
            }
            
            yield return null;
        }
        
        // Clean up
        for (int i = 0; i < smokeParticles.Length; i++)
        {
            if (smokeParticles[i] != null)
            {
                Destroy(smokeParticles[i]);
            }
        }
    }
    
    public void CreateSparkShower(Vector3 position, int sparkCount = 15)
    {
        for (int i = 0; i < sparkCount; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spark.transform.position = position;
            spark.transform.localScale = Vector3.one * Random.Range(0.02f, 0.08f);
            
            // Bright spark material
            Material sparkMat = new Material(Shader.Find("Standard"));
            sparkMat.color = Color.white;
            sparkMat.EnableKeyword("_EMISSION");
            sparkMat.SetColor("_EmissionColor", Color.yellow * 2f);
            spark.GetComponent<MeshRenderer>().material = sparkMat;
            
            // Spark physics
            Rigidbody sparkRb = spark.AddComponent<Rigidbody>();
            sparkRb.mass = 0.01f;
            Vector3 sparkDirection = Random.insideUnitSphere;
            sparkDirection.y = Mathf.Abs(sparkDirection.y); // Upward bias
            sparkRb.AddForce(sparkDirection * 8f, ForceMode.Impulse);
            
            // Quick fade and destroy
            StartCoroutine(FadeAndDestroy(spark, 0.5f));
        }
    }
    
    IEnumerator FadeAndDestroy(GameObject obj, float fadeTime)
    {
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        Color originalColor = renderer.material.color;
        
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeTime);
            
            Color currentColor = originalColor;
            currentColor.a = alpha;
            renderer.material.color = currentColor;
            
            yield return null;
        }
        
        Destroy(obj);
    }
}