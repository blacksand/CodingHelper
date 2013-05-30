using System;
using EnvDTE;
using EnvDTE80;

namespace Blacksand
{
    abstract class HelperCommand
    {
        public HelperCommand(string name, string description)
        {
            _name = name;
            _text = name;
            _descritption = description;
        }

        abstract public object Execute(DTE2 application, AddIn addin, object varIn);

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public string Name
        {
            get { return _name == null ? "" : _name; }
            set { _name = value; }
        }

        public string Description
        {
            get { return _descritption; }
            set { _descritption = value; }
        }

        private string _text;
        private string _name;
        private string _descritption;
    }
}
