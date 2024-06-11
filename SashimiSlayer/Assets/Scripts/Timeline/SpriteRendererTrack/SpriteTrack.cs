using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.1394896f, 0.4411765f, 0.3413077f)]
[TrackClipType(typeof(SpritePlayableAsset))]
[TrackBindingType(typeof(SpriteRenderer))]
public class SpriteTrack : TrackAsset
{

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<SpriteTrackMixerBehaviour>.Create(graph, inputCount);
    }
    
    /// <summary>
    /// Ensures that the gathered properties are reset to their default values after preview ends
    /// </summary>
    /// <param name="director"></param>
    /// <param name="driver"></param>
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        SpriteRenderer trackBinding = director.GetGenericBinding(this) as SpriteRenderer;
        if (trackBinding == null)
            return;

        // The field names are the name of the backing serializable field. These can be found from the class source,
        // or from the unity scene file that contains an object of that type.
        driver.AddFromName<SpriteRenderer>(trackBinding.gameObject, "m_Sprite");
        driver.AddFromName<SpriteRenderer>(trackBinding.gameObject, "m_Color");

        base.GatherProperties(director, driver);
    }
}

