
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;


// Editor used by the TimelineEditor to customize the view of a TextPlayableAsset
[CustomTimelineEditor(typeof(SpritePlayableAsset))]
public class SpritePlayableAssetClipEditor : ClipEditor
{
    // Called when a clip value, it's attached PlayableAsset, or an animation curve on a template is changed from the TimelineEditor.
    // This is used to keep the displayName of the clip matching the text of the PlayableAsset.
    public override void OnClipChanged(TimelineClip clip)
    {
        var textPlayableasset = clip.asset as SpritePlayableAsset;
        if (textPlayableasset != null && textPlayableasset.template.sprite != null)
            clip.displayName = textPlayableasset.template.sprite.name;
    }
}

