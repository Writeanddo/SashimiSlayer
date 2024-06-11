
using TMPro;
using UnityEngine;
using UnityEngine.Playables;


    // The runtime instance of a the TextTrack. It is responsible for blending and setting the final data
    // on the Text binding
    public class SpriteTrackMixerBehaviour : PlayableBehaviour
    {
        Color m_DefaultColor;
        Sprite m_DefaultSprite;

        SpriteRenderer m_TrackBinding;

        // Called every frame that the timeline is evaluated. ProcessFrame is invoked after its' inputs.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            SetDefaults(playerData as SpriteRenderer);
            
            if (m_TrackBinding == null)
                return;

            int inputCount = playable.GetInputCount();

            Color blendedColor = Color.clear;
            float totalWeight = 0f;
            float greatestWeight = 0f;
            Sprite sprite = m_DefaultSprite;

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<SpritePlayableBehaviour> inputPlayable = (ScriptPlayable<SpritePlayableBehaviour>)playable.GetInput(i);
                SpritePlayableBehaviour input = inputPlayable.GetBehaviour();

                blendedColor += input.color * inputWeight;
                totalWeight += inputWeight;

                // use the sprite with the highest weight
                if (inputWeight > greatestWeight)
                {
                    sprite = input.sprite;
                    greatestWeight = inputWeight;
                }
            }

            // blend to the default values
            m_TrackBinding.color = Color.Lerp(m_DefaultColor, blendedColor, totalWeight);
            m_TrackBinding.sprite = sprite;
        }


        public override void OnPlayableDestroy(Playable playable)
        {
            RestoreDefaults();
        }

        void SetDefaults(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == m_TrackBinding)
                return;

            RestoreDefaults();

            m_TrackBinding = spriteRenderer;
            if (m_TrackBinding != null)
            {
                m_DefaultColor = m_TrackBinding.color;
                m_DefaultSprite = m_TrackBinding.sprite;
            }
        }

        void RestoreDefaults()
        {
            if (m_TrackBinding == null)
                return;

            m_TrackBinding.color = m_DefaultColor;
            m_TrackBinding.sprite = m_DefaultSprite;
        }
    }

