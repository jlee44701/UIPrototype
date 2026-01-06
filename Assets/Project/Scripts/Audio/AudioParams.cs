using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Audio {
    public class AudioParams {
        [Serializable]
        public class Pitch {
            public enum Variation {
                Small,
                Medium,
                Large,
                VerySmall,
            }
            [Range(0,10)]
            public float pitch;

            public Pitch(float exact) {
                pitch = exact;
            }

            public Pitch(float minRandom, float maxRandom) {
                pitch = Random.Range(minRandom, maxRandom);
            }

            public Pitch(Variation randomVariation) {
                switch (randomVariation) {
                    case Variation.VerySmall:
                        pitch = Random.Range(0.95f, 1.05f);
                        break;
                    case Variation.Small:
                        pitch = Random.Range(0.9f, 1.1f);
                        break;
                    case Variation.Medium:
                        pitch = Random.Range(0.75f, 1.25f);
                        break;
                    case Variation.Large:
                        pitch = Random.Range(0.5f, 1.5f);
                        break;
                }
            }
        }

        [Serializable]
        public class Repetition {
            [Range(0,4)]
            public float minRepetitionFrequency;

            public Repetition(float minRepetitionFrequency) {
                this.minRepetitionFrequency = minRepetitionFrequency;
            }
        }

        [Serializable]
        public class Randomization {
            public bool noRepeating;

            public Randomization(bool noRepeating = true) {
                this.noRepeating = noRepeating;
            }
        }

        [Serializable]
        public class Distortion {
            public bool muffled;
        }
    }

}