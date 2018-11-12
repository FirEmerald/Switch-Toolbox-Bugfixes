﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Switch_Toolbox.Library.IO;

namespace Switch_Toolbox.Library
{
    public interface IFileFormat
    {
        string[] Description { get; set; }
        string[] Extension { get; set; }
        Type[] Types { get; } //Types hold menu extensions
        string Magic { get; set; }
        CompressionType CompressionType { get; set; }
        bool FileIsCompressed { get; set; }
        bool FileIsEdited { get; set; }
        bool CanSave { get; set; }
        bool IsActive { get; set; }
        bool UseEditMenu { get; set; }
        byte[] Data { get; set; }
        string FileName { get; set; }
        string FilePath { get; set; }
        TreeNode EditorRoot { get; set; }
        void Load();
        void Unload();
        byte[] Save();
        int Alignment { get; set; } //Alignment to save the file back. Also used for Yaz0 comp sometimes
    }
}
