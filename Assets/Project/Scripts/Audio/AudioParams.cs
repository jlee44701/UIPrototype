using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Audio {
    public class AudioParams {
        [Serializable]
        public class Pitch {
            [Serializable]
            public enum Variation {
                Small,
                Medium,
                Large,
                VerySmall,
            }
            [Range(0,10)]
            public float _pitch = 1;
            
            public Pitch(float exact) {
                _pitch = exact;
            }

            public Pitch(float minRandom, float maxRandom) {
                _pitch = Random.Range(minRandom, maxRandom);
            }
            public Pitch Vary(Variation variation) {
                _pitch = variation switch {
                    Variation.VerySmall => Random.Range(0.95f, 1.05f),
                    Variation.Small => Random.Range(0.9f, 1.1f),
                    Variation.Medium => Random.Range(0.75f, 1.25f),
                    Variation.Large => Random.Range(0.5f, 1.5f),
                    _ => _pitch
                };
                return this;
            }
            public Pitch(Variation randomVariation) {
                Vary(randomVariation);
            }
        }

        [Serializable]
        public class Repetition {
            [FormerlySerializedAs("minRepetitionFrequency")]
            [Range(0,4)]
            public float _minRepetitionFrequency;

            public Repetition(float minRepetitionFrequency) {
                this._minRepetitionFrequency = minRepetitionFrequency;
            }
        }

        [Serializable]
        public class Randomization {
            [FormerlySerializedAs("noRepeating")]
            public bool _noRepeating;

            public Randomization(bool noRepeating = true) {
                this._noRepeating = noRepeating;
            }
        }

        [Serializable]
        public class Distortion {
            [FormerlySerializedAs("muffled")]
            public bool _muffled;
        }
    }

}