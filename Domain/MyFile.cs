using System;

namespace Domain
{
    public class MyFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Size { get; set; }
        public string FilePathToServer { get; set; }
    }
}
