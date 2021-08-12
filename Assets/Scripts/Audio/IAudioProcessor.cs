using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Audio
{
    public interface IAudioProcessor
    {
        void Process(AudioBuffer input);
    }

    public abstract class AudioProcessor<Return> : MonoBehaviour, IAudioProcessor
    {
        [Range(1,8096)]
        public int bufferSize = 1;
        protected readonly Queue<Return> buffer = new Queue<Return>();

        [Serializable]
        public class CurrentEvent : UnityEvent<Return> { }
        [Serializable]
        public class QueueEvent : UnityEvent<Queue<Return>> { }

        public QueueEvent onInsert;
        public CurrentEvent onProcessed;
        public virtual void Process(AudioBuffer input)
        {
            var processed = Process1(input);
            while (buffer.Count >= bufferSize)
                buffer.Dequeue();
            buffer.Enqueue(processed);
            onInsert.Invoke(buffer);
            onProcessed.Invoke(processed);
        }

        protected abstract Return Process1(AudioBuffer data);
    }
}
