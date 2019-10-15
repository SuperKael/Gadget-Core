using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace URP
{
    public abstract class CoroutineOverrideTemplate<T> : IEnumerator, IDisposable, IEnumerator<object> where T : MonoBehaviour
    {
        object IEnumerator.Current
        {
            get
            {
                return current;
            }
        }

        object IEnumerator<object>.Current
        {
            get
            {
                return current;
            }
        }

        private YieldInstruction current;
        private bool disposing;
        private int PC;

        private T instance;
        private FieldInfo[] parameters;

        public CoroutineOverrideTemplate(T instance, params FieldInfo[] parameters)
        {
            this.instance = instance;
            this.parameters = parameters;
        }

        public abstract YieldInstruction Next(int PC, T instance, ref object[] parameters);

        public virtual bool MoveNext()
        {
            if (PC >= 0)
            {
                object[] paramValues = new object[parameters.Length];
                for (int i = 0; i < paramValues.Length; i++) paramValues[i] = parameters[i].GetValue(instance);
                YieldInstruction insn = Next(PC, instance, ref paramValues);
                for (int i = 0; i < paramValues.Length; i++) parameters[i].SetValue(instance, paramValues[i]);
                if (insn != null)
                {
                    current = insn;
                }
                else
                {
                    return false;
                }
                if (!disposing)
                {
                    PC++;
                }
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            disposing = true;
            PC = -1;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
