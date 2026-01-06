using System.Collections;
using UnityEngine;

public sealed class SfxChannel : MonoBehaviour
{
    [SerializeField] AudioSource m_AudioSource;
    [SerializeField] AudioLowPassFilter m_AudioLowPassFilter;

    Coroutine m_ReleaseCoroutine;
    bool m_IsReserved;

    public bool IsAvailable
    {
        get
        {
            if (!m_AudioSource)
                return false;

            if (m_IsReserved)
                return false;

            return !m_AudioSource.isPlaying;
        }
    }

    public void Play(
        AudioClip clip,
        Vector3 position,
        float volume,
        float normalizedSkipTime,
        float pitch,
        bool isLooping,
        bool isMuffled,
        float muffledCutoffFrequencyHz,
        System.Action<SfxChannel> onFinished)
    {
        if (!m_AudioSource || !clip)
            return;

        transform.position = position;

        m_AudioSource.Stop();
        m_AudioSource.pitch = pitch;
        m_AudioSource.loop = isLooping;

        if (m_AudioLowPassFilter)
        {
            m_AudioLowPassFilter.enabled = isMuffled;
            if (isMuffled)
            {
                m_AudioLowPassFilter.cutoffFrequency = muffledCutoffFrequencyHz;
            }
        }

        if (m_ReleaseCoroutine != null)
        {
            StopCoroutine(m_ReleaseCoroutine);
            m_ReleaseCoroutine = null;
        }
        m_IsReserved = true;

        var shouldSeek = normalizedSkipTime > 0f;
        if (isLooping || shouldSeek)
        {
            m_AudioSource.clip = clip;

            var clampedNormalizedSkipTime = Mathf.Clamp01(normalizedSkipTime);
            var targetSample = Mathf.FloorToInt(clip.samples * clampedNormalizedSkipTime);
            m_AudioSource.timeSamples = Mathf.Clamp(targetSample, 0, clip.samples - 1);

            m_AudioSource.volume = volume;
            m_AudioSource.Play();

            if (!isLooping)
            {
                var approximateDurationSeconds = clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
                m_ReleaseCoroutine = StartCoroutine(ReleaseAfterSeconds(approximateDurationSeconds, onFinished));
            }

            return;
        }

        m_AudioSource.volume = 1f;
        m_AudioSource.PlayOneShot(clip, volume);

        var approximateOneShotSeconds = clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
        m_ReleaseCoroutine = StartCoroutine(ReleaseAfterSeconds(approximateOneShotSeconds, onFinished));
    }

    public void StopAndRelease(System.Action<SfxChannel> onFinished)
    {
        if (m_ReleaseCoroutine != null)
        {
            StopCoroutine(m_ReleaseCoroutine);
            m_ReleaseCoroutine = null;
        }

        if (m_AudioSource)
            m_AudioSource.Stop();

        m_IsReserved = false;
        onFinished?.Invoke(this);
    }

    IEnumerator ReleaseAfterSeconds(float seconds, System.Action<SfxChannel> onFinished)
    {
        yield return new WaitForSeconds(seconds);
        m_IsReserved = false;
        onFinished?.Invoke(this);
        m_ReleaseCoroutine = null;
    }
}
