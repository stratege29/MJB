using UnityEngine;
using System.Collections;

public class BallEffects : MonoBehaviour
{
    [Header("Launch Effects")]
    public GameObject muzzleFlashPrefab;
    public ParticleSystem launchParticles;
    public float muzzleFlashDuration = 0.2f;
    
    [Header("Trail Effects")]
    public TrailRenderer ballTrail;
    public AnimationCurve trailWidthCurve = AnimationCurve.Linear(0, 1, 1, 0);
    public Gradient normalTrailGradient;
    public Gradient chargedTrailGradient;
    
    [Header("Impact Effects")]
    public ParticleSystem impactParticles;
    public GameObject impactShockwavePrefab;
    public float screenShakeIntensity = 0.1f;
    public float screenShakeDuration = 0.2f;
    
    [Header("Charging Effects")]
    public ParticleSystem chargingAura;
    public Light ballLight;
    public AnimationCurve chargingGlowCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    public AudioClip launchSound;
    public AudioClip chargedLaunchSound;
    public AudioClip impactSound;
    public AudioClip whizzSound;
    
    private Ball ballScript;
    private AudioSource audioSource;
    private MeshRenderer ballRenderer;
    private Material ballMaterial;
    private Color originalBallColor;
    private Camera playerCamera;
    
    // Effect state
    private bool isCharged = false;
    private float currentCharge = 0f;
    
    void Awake()
    {
        ballScript = GetComponent<Ball>();
        audioSource = GetComponent<AudioSource>();
        ballRenderer = GetComponent<MeshRenderer>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        SetupTrailEffect();
        SetupBallMaterial();
        SetupLighting();
    }
    
    void SetupTrailEffect()
    {
        if (ballTrail == null)
        {
            ballTrail = gameObject.AddComponent<TrailRenderer>();
        }
        
        ballTrail.time = 0.8f;
        ballTrail.widthCurve = trailWidthCurve;
        ballTrail.material = CreateTrailMaterial();
        
        // Setup gradients if not assigned
        if (normalTrailGradient.colorKeys.Length == 0)
        {
            normalTrailGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(Color.cyan, 0f);
            colorKeys[1] = new GradientColorKey(Color.blue, 0.5f);
            colorKeys[2] = new GradientColorKey(Color.white, 1f);
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(0f, 1f);
            
            normalTrailGradient.SetKeys(colorKeys, alphaKeys);
        }
        
        if (chargedTrailGradient.colorKeys.Length == 0)
        {
            chargedTrailGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(Color.yellow, 0f);
            colorKeys[1] = new GradientColorKey(Color.orange, 0.5f);
            colorKeys[2] = new GradientColorKey(Color.red, 1f);
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(0f, 1f);
            
            chargedTrailGradient.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    Material CreateTrailMaterial()
    {
        Material trailMat = new Material(Shader.Find("Sprites/Default"));
        trailMat.color = Color.cyan;
        return trailMat;
    }
    
    void SetupBallMaterial()
    {
        if (ballRenderer != null)
        {
            if (ballRenderer.material != null)
            {
                ballMaterial = ballRenderer.material;
                originalBallColor = ballMaterial.color;
            }
            else
            {
                ballMaterial = new Material(Shader.Find("Standard"));
                ballMaterial.color = Color.white;
                ballRenderer.material = ballMaterial;
                originalBallColor = Color.white;
            }
        }
    }
    
    void SetupLighting()
    {
        if (ballLight == null)
        {
            GameObject lightObj = new GameObject("BallLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            
            ballLight = lightObj.AddComponent<Light>();
            ballLight.type = LightType.Point;
            ballLight.color = Color.cyan;
            ballLight.intensity = 0f; // Start disabled
            ballLight.range = 5f;
            ballLight.enabled = false;
        }
    }
    
    public void PlayLaunchEffect(bool isChargedShot)
    {
        isCharged = isChargedShot;
        
        // Play launch sound
        AudioClip soundToPlay = isChargedShot ? chargedLaunchSound : launchSound;
        PlaySound(soundToPlay);
        
        // Create muzzle flash
        CreateMuzzleFlash();
        
        // Setup trail for shot type
        ConfigureTrailForShotType(isChargedShot);
        
        // Enable ball lighting for charged shots
        if (isChargedShot)
        {
            ballLight.enabled = true;
            ballLight.intensity = 2f;
            ballLight.color = Color.yellow;
        }
        
        // Start whizz sound loop
        if (whizzSound != null)
        {
            audioSource.clip = whizzSound;
            audioSource.loop = true;
            audioSource.volume = 0.3f;
            audioSource.Play();
        }
        
        Debug.Log($"⚽ Launch effects played - Charged: {isChargedShot}");
    }
    
    void CreateMuzzleFlash()
    {
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, transform.position, transform.rotation);
            Destroy(flash, muzzleFlashDuration);
        }
        else
        {
            // Create simple flash effect
            StartCoroutine(SimpleMuzzleFlash());
        }
    }
    
    IEnumerator SimpleMuzzleFlash()
    {
        // Create temporary flash sphere
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.transform.position = transform.position;
        flash.transform.localScale = Vector3.one * (isCharged ? 2f : 1.5f);
        
        Material flashMat = new Material(Shader.Find("Standard"));
        flashMat.color = isCharged ? Color.yellow : Color.cyan;
        flashMat.EnableKeyword("_EMISSION");
        flashMat.SetColor("_EmissionColor", flashMat.color * 3f);
        
        flash.GetComponent<MeshRenderer>().material = flashMat;
        flash.GetComponent<Collider>().enabled = false;
        
        // Animate flash
        float elapsed = 0f;
        Vector3 startScale = flash.transform.localScale;
        
        while (elapsed < muzzleFlashDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / muzzleFlashDuration;
            
            // Scale up and fade out
            flash.transform.localScale = startScale * (1f + normalizedTime);
            
            Color currentColor = flashMat.color;
            currentColor.a = 1f - normalizedTime;
            flashMat.color = currentColor;
            
            yield return null;
        }
        
        Destroy(flash);
    }
    
    void ConfigureTrailForShotType(bool isChargedShot)
    {
        if (ballTrail == null) return;
        
        if (isChargedShot)
        {
            ballTrail.colorGradient = chargedTrailGradient;
            ballTrail.startWidth = 0.6f;
            ballTrail.endWidth = 0.2f;
            ballTrail.time = 1.2f;
        }
        else
        {
            ballTrail.colorGradient = normalTrailGradient;
            ballTrail.startWidth = 0.4f;
            ballTrail.endWidth = 0.1f;
            ballTrail.time = 0.8f;
        }
    }
    
    public void PlayImpactEffect(Vector3 impactPoint, Vector3 impactNormal)
    {
        // Stop whizz sound
        if (audioSource.isPlaying && audioSource.clip == whizzSound)
        {
            audioSource.Stop();
        }
        
        // Play impact sound
        PlaySound(impactSound);
        
        // Create impact particles
        CreateImpactParticles(impactPoint, impactNormal);
        
        // Create shockwave for charged shots
        if (isCharged)
        {
            CreateShockwave(impactPoint);
        }
        
        // Screen shake
        if (playerCamera != null)
        {
            StartCoroutine(ScreenShake());
        }
        
        Debug.Log("💥 Impact effects played");
    }
    
    void CreateImpactParticles(Vector3 position, Vector3 normal)
    {
        if (impactParticles != null)
        {
            // Use assigned particle system
            impactParticles.transform.position = position;
            impactParticles.transform.LookAt(position + normal);
            impactParticles.Play();
        }
        else
        {
            // Create simple particle burst
            StartCoroutine(SimpleImpactParticles(position, normal));
        }
    }
    
    IEnumerator SimpleImpactParticles(Vector3 position, Vector3 normal)
    {
        int particleCount = isCharged ? 20 : 12;
        
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.transform.position = position;
            particle.transform.localScale = Vector3.one * Random.Range(0.05f, 0.15f);
            
            // Random color based on shot type
            Color particleColor = isCharged ? 
                Color.Lerp(Color.yellow, Color.orange, Random.Range(0f, 1f)) : 
                Color.Lerp(Color.cyan, Color.white, Random.Range(0f, 1f));
            
            Material particleMat = new Material(Shader.Find("Standard"));
            particleMat.color = particleColor;
            particleMat.EnableKeyword("_EMISSION");
            particleMat.SetColor("_EmissionColor", particleColor * 2f);
            
            particle.GetComponent<MeshRenderer>().material = particleMat;
            
            // Add physics
            Rigidbody particleRb = particle.AddComponent<Rigidbody>();
            particleRb.mass = 0.1f;
            
            Vector3 randomDirection = (normal + Random.insideUnitSphere * 0.5f).normalized;
            particleRb.AddForce(randomDirection * Random.Range(3f, 8f), ForceMode.Impulse);
            particleRb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
            
            // Destroy after short time
            Destroy(particle, Random.Range(1f, 2f));
        }
        
        yield return null;
    }
    
    void CreateShockwave(Vector3 position)
    {
        if (impactShockwavePrefab != null)
        {
            GameObject shockwave = Instantiate(impactShockwavePrefab, position, Quaternion.identity);
            Destroy(shockwave, 2f);
        }
        else
        {
            StartCoroutine(SimpleShockwave(position));
        }
    }
    
    IEnumerator SimpleShockwave(Vector3 position)
    {
        GameObject shockwave = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shockwave.transform.position = position;
        shockwave.transform.localScale = Vector3.zero;
        
        Material shockwaveMat = new Material(Shader.Find("Standard"));
        shockwaveMat.color = new Color(1f, 0.8f, 0f, 0.3f); // Semi-transparent gold
        shockwaveMat.EnableKeyword("_EMISSION");
        shockwaveMat.SetColor("_EmissionColor", Color.yellow);
        
        shockwave.GetComponent<MeshRenderer>().material = shockwaveMat;
        shockwave.GetComponent<Collider>().enabled = false;
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            // Expand and fade
            float scale = normalizedTime * 8f;
            shockwave.transform.localScale = Vector3.one * scale;
            
            Color currentColor = shockwaveMat.color;
            currentColor.a = (1f - normalizedTime) * 0.3f;
            shockwaveMat.color = currentColor;
            
            yield return null;
        }
        
        Destroy(shockwave);
    }
    
    IEnumerator ScreenShake()
    {
        Vector3 originalPosition = playerCamera.transform.position;
        float elapsed = 0f;
        
        while (elapsed < screenShakeDuration)
        {
            elapsed += Time.deltaTime;
            
            float strength = screenShakeIntensity * (1f - elapsed / screenShakeDuration);
            Vector3 shakeOffset = Random.insideUnitSphere * strength;
            shakeOffset.z = 0; // Don't shake on Z axis
            
            playerCamera.transform.position = originalPosition + shakeOffset;
            
            yield return null;
        }
        
        playerCamera.transform.position = originalPosition;
    }
    
    public void UpdateChargingEffect(float chargeLevel)
    {
        currentCharge = chargeLevel;
        
        // Update ball material
        if (ballMaterial != null)
        {
            float glowIntensity = chargingGlowCurve.Evaluate(chargeLevel);
            Color glowColor = Color.Lerp(originalBallColor, Color.yellow, glowIntensity);
            
            ballMaterial.color = glowColor;
            
            if (glowIntensity > 0.1f)
            {
                ballMaterial.EnableKeyword("_EMISSION");
                ballMaterial.SetColor("_EmissionColor", Color.yellow * glowIntensity * 2f);
            }
        }
        
        // Update light intensity
        if (ballLight != null && chargeLevel > 0.1f)
        {
            ballLight.enabled = true;
            ballLight.intensity = chargeLevel * 3f;
            ballLight.color = Color.Lerp(Color.white, Color.yellow, chargeLevel);
        }
        
        // Particle effects for charging
        if (chargingAura != null)
        {
            if (chargeLevel > 0.1f && !chargingAura.isPlaying)
            {
                chargingAura.Play();
            }
            else if (chargeLevel <= 0.1f && chargingAura.isPlaying)
            {
                chargingAura.Stop();
            }
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    void OnDestroy()
    {
        // Clean up any lingering effects
        if (ballLight != null)
        {
            ballLight.enabled = false;
        }
        
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}