using Cysharp.Threading.Tasks;
using Events;
using UnityEngine;

namespace Feel.Notes
{
    /// <summary>
    ///     Simple sprite flipbook that changes whenever a beat event is triggered
    /// </summary>
    public class BeatAnimatedSprite : MonoBehaviour
    {
        [Header("Depends")]

        [SerializeField]
        private IntEvent _incrementEvent;

        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [Header("Config")]

        [SerializeField]
        private Sprite[] _sprites;

        [SerializeField]
        private int _incrementInterval;

        [SerializeField]
        private bool _loop;

        [SerializeField]
        private float _delay;

        [SerializeField]
        private bool _animationEnabled;

        private int _firstIndex;

        private BeatAnimatedSprite _transitionTo;

        private void OnEnable()
        {
            _incrementEvent.AddListener(HandleEvent);
        }

        private void OnDisable()
        {
            _incrementEvent.RemoveListener(HandleEvent);
        }

        private void HandleEvent(int currentSubdiv)
        {
            if (!_animationEnabled)
            {
                return;
            }

            if (_firstIndex == -1)
            {
                _firstIndex = currentSubdiv;
            }

            int relativeIndex = currentSubdiv - _firstIndex;
            if (relativeIndex % _incrementInterval == 0)
            {
                int spriteIndex = relativeIndex / _incrementInterval;
                if (spriteIndex > _sprites.Length - 1)
                {
                    if (_transitionTo != null)
                    {
                        ForceTransition(_transitionTo, currentSubdiv);
                        _transitionTo = null;
                        return;
                    }

                    if (!_loop)
                    {
                        Stop();
                        return;
                    }
                }

                SetSprite(spriteIndex).Forget();
            }
        }

        private async UniTaskVoid SetSprite(int newIndex)
        {
            await UniTask.Delay((int)(_delay * 1000));

            // Prevent changing after transitioning away
            if (!_animationEnabled)
            {
                return;
            }

            _spriteRenderer.sprite = _sprites[newIndex % _sprites.Length];
        }

        /// <summary>
        ///     Setup a transition to another BeatAnimatedSprite when this one ends
        /// </summary>
        /// <param name="transitionTo"></param>
        public void SetupTransitionOnEnd(BeatAnimatedSprite transitionTo)
        {
            _transitionTo = transitionTo;
        }

        public void ForceTransition(BeatAnimatedSprite to, int currentSubdiv = -1)
        {
            if (!_animationEnabled)
            {
                return;
            }

            to.Play(currentSubdiv);
            Stop();
        }

        public void Play(int firstSubdiv = -1)
        {
            _animationEnabled = true;
            _firstIndex = firstSubdiv;
            _spriteRenderer.sprite = _sprites[0];
        }

        public void Stop()
        {
            _animationEnabled = false;
        }
    }
}