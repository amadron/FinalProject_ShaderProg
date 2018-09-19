using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.controller.states
{
    interface IState
    {
        void OnStateEnter();
        void OnStateUpdate(float deltatime);
        void OnStateAbort();
        void OnStateExit();
    }
}
