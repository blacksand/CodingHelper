using System;
using EnvDTE;
using EnvDTE80;

namespace Blacksand
{
    abstract class HelperCommand
    {
        public HelperCommand(string name, string text, string description)
        {
            Name = name;
            Text = text;
            Description = description;
        }

        abstract public object Execute(DTE2 application, AddIn addin, object varIn);

        public string Name { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public Command NamedCommand { get; set; }
    }
}
