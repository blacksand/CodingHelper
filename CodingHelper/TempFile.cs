using System;

namespace Blacksand
{
    class TempFile
    {
        public TempFile(string extName = ".txt")
        {
            _name = GetTempFileName(extName);
            AutoDelete = true;
        }

        ~TempFile()
        {
            if (AutoDelete)
            {
                Delete();
            }
        }

        public void Delete()
        {
            if (Name.Length > 0)
            {
                try
                {
                    System.IO.File.Delete(Name);
                }
                finally
                {
                    _name = null;
                }
            }
        }


        public string Path { get; set; }
        public bool AutoDelete { get; set; }

        public string Name
        {
            get { return _name == null ? "" : _name; }
            set { _name = value; }
        }

        public void Write(string text)
        {
            System.IO.File.WriteAllText(Name, text);
        }

        public string Read(string name = null)
        {
            if (name != null)
                _name = name;

            return System.IO.File.ReadAllText(Name);
        }

        private string GetTempFileName(string extName)
        {
            string tempDir = System.IO.Path.GetTempPath();

            if (!System.IO.Directory.Exists(tempDir))
            {
                System.IO.Directory.CreateDirectory(tempDir);
            }

            string filePath;
            Random rnd = new Random();
            
            do 
            {
                int code = rnd.Next(0, 0xFFFFF);
                string name = String.Format("BS_CodingHelper_{0:X5}{1}", code, extName);
                filePath = System.IO.Path.Combine(tempDir, name);
            }
            while (System.IO.File.Exists(filePath));

            return filePath;
        }

        string _name;
    }
}
