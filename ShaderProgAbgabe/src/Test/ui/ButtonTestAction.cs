using Example.src.controller.logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.Test.ui
{
    class ButtonTestAction : IAction
    {
        public bool Execute()
        {
            Console.WriteLine("Hello, This is the Action!");
            return true;
        }
    }
}
