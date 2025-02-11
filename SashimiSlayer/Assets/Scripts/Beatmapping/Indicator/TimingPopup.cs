using System.Collections.Generic;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using UnityEngine;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Beat note listener that displays the timing popup on interaction (e.g 'Early' or 'Perfect')
    /// </summary>
    public class TimingPopup : BeatNoteModule
    {
        [SerializeField]
        private ParticleSystem _earlyParticles;

        [SerializeField]
        private ParticleSystem _perfectParticles;

        [SerializeField]
        private ParticleSystem _lateParticles;

        [SerializeField]
        private ParticleSystem _missParticles;

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            beatNote.OnInteractionFinalResult += BeatNote_OnInteractionFinalResult;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            beatNote.OnInteractionFinalResult -= BeatNote_OnInteractionFinalResult;
        }

        private void BeatNote_OnInteractionFinalResult(BeatNote.NoteTickInfo tickinfo,
            NoteInteraction.FinalResult finalresult)
        {
            TimingWindow.Score timingResultScore = finalresult.TimingResult.Score;
            TimingWindow.Direction earlyOrLate = finalresult.TimingResult.Direction;

            if (finalresult.Successful)
            {
                if (timingResultScore == TimingWindow.Score.Pass)
                {
                    if (earlyOrLate == TimingWindow.Direction.Early)
                    {
                        _earlyParticles.Play();
                    }
                    else if (earlyOrLate == TimingWindow.Direction.Late)
                    {
                        _lateParticles.Play();
                    }
                }
                else if (timingResultScore == TimingWindow.Score.Perfect)
                {
                    _perfectParticles.Play();
                }
            }
            else
            {
                _missParticles.Play();
            }
        }
    }
}